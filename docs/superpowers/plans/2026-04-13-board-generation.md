# Board Generation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add Sudoku puzzle generation with 180-degree rotational symmetry and difficulty-based clue count ranges.

**Architecture:** New `Generators/` namespace mirroring the existing `Solvers/` pattern. `IGenerator` interface with `SudokuGenerator` implementation that takes an `ISolver` dependency. Generation works in two phases: fill a complete board via diagonal-box seeding + solver, then remove symmetric cell pairs until reaching the target clue count while maintaining solution uniqueness.

**Tech Stack:** C# / .NET 8.0, NUnit 4, FluentAssertions, BenchmarkDotNet

---

## File Map

| Action | File | Responsibility |
|--------|------|----------------|
| Create | `Sudoku.Core/Difficulty.cs` | `Difficulty` enum (Easy-Evil) |
| Create | `Sudoku.Core/Generators/IGenerator.cs` | Generator interface |
| Create | `Sudoku.Core/Generators/SudokuGenerator.cs` | Generation algorithm |
| Modify | `Sudoku.Core/SudokuBoard.cs` | Add `ClueCount` property |
| Create | `Sudoku.Tests/Generators/SudokuGeneratorTests.cs` | Generator tests |
| Modify | `Sudoku.Benchmarks/SudokuBenchmarks.cs` | Generation benchmarks |

---

### Task 1: Add ClueCount to SudokuBoard

We need a way to count non-zero cells. The existing `SudokuBoard` has `IsFilled()` but no clue count.

**Files:**
- Modify: `Sudoku.Core/SudokuBoard.cs:34`
- Modify: `Sudoku.Tests/SudokuBoardTests.cs` (add test)

- [ ] **Step 1: Write the failing test**

Add to the bottom of `Sudoku.Tests/SudokuBoardTests.cs`:

```csharp
[Test]
public void ClueCount_ReturnsNumberOfNonZeroCells()
{
    var board = new SudokuBoard();
    board[0, 0] = 5;
    board[4, 4] = 3;
    board[8, 8] = 7;

    board.ClueCount.Should().Be(3);
}

[Test]
public void ClueCount_EmptyBoard_ReturnsZero()
{
    var board = new SudokuBoard();

    board.ClueCount.Should().Be(0);
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test Sudoku.Tests --filter "FullyQualifiedName~ClueCount" -v n`
Expected: Build failure — `SudokuBoard` does not contain `ClueCount`

- [ ] **Step 3: Implement ClueCount**

Add the following property to `Sudoku.Core/SudokuBoard.cs`, right after the `IsFilled()` method (line 35):

```csharp
public int ClueCount =>
    Sudoku.Cast<int>().Count(c => c > 0);
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test Sudoku.Tests --filter "FullyQualifiedName~ClueCount" -v n`
Expected: 2 tests pass

- [ ] **Step 5: Commit**

```bash
git add Sudoku.Core/SudokuBoard.cs Sudoku.Tests/SudokuBoardTests.cs
git commit -m "Add ClueCount property to SudokuBoard"
```

---

### Task 2: Add Difficulty Enum

**Files:**
- Create: `Sudoku.Core/Difficulty.cs`

- [ ] **Step 1: Create the enum**

Create `Sudoku.Core/Difficulty.cs`:

```csharp
namespace Sudoku.Core;

public enum Difficulty
{
    Easy,
    Medium,
    Hard,
    Expert,
    Evil
}
```

- [ ] **Step 2: Verify it builds**

Run: `dotnet build Sudoku.Core`
Expected: Build succeeded

- [ ] **Step 3: Commit**

```bash
git add Sudoku.Core/Difficulty.cs
git commit -m "Add Difficulty enum"
```

---

### Task 3: Add IGenerator Interface

**Files:**
- Create: `Sudoku.Core/Generators/IGenerator.cs`

- [ ] **Step 1: Create the interface**

Create `Sudoku.Core/Generators/IGenerator.cs`:

```csharp
namespace Sudoku.Core.Generators;

public interface IGenerator
{
    SudokuBoard Generate(Difficulty difficulty);
}
```

- [ ] **Step 2: Verify it builds**

Run: `dotnet build Sudoku.Core`
Expected: Build succeeded

- [ ] **Step 3: Commit**

```bash
git add Sudoku.Core/Generators/IGenerator.cs
git commit -m "Add IGenerator interface"
```

---

### Task 4: Implement SudokuGenerator — Full Board Generation

This task implements Phase 1 only: generating a complete, valid, filled board. We test this via a private helper exposed through the public `Generate` method.

**Files:**
- Create: `Sudoku.Core/Generators/SudokuGenerator.cs`
- Create: `Sudoku.Tests/Generators/SudokuGeneratorTests.cs`

- [ ] **Step 1: Write the failing test**

Create `Sudoku.Tests/Generators/SudokuGeneratorTests.cs`:

```csharp
using Sudoku.Core.Generators;
using Sudoku.Core.Solvers;

namespace Sudoku.Tests.Generators;

[Parallelizable(ParallelScope.All)]
public class SudokuGeneratorTests
{
    [Test]
    public void Generate_ReturnsValidPuzzle()
    {
        var generator = new SudokuGenerator(new SimpleSolver(), new Random(42));

        var puzzle = generator.Generate(Difficulty.Easy);
        var solution = new SimpleSolver().Solve(puzzle);

        solution.IsFilled().Should().BeTrue();
        solution.IsValid().Should().BeTrue();
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test Sudoku.Tests --filter "FullyQualifiedName~SudokuGeneratorTests" -v n`
Expected: Build failure — `SudokuGenerator` class does not exist

- [ ] **Step 3: Implement SudokuGenerator with full board generation**

Create `Sudoku.Core/Generators/SudokuGenerator.cs`:

```csharp
using Sudoku.Core.Solvers;

namespace Sudoku.Core.Generators;

public class SudokuGenerator : IGenerator
{
    private readonly ISolver _solver;
    private readonly Random _random;

    public SudokuGenerator(ISolver solver, Random? random = null)
    {
        _solver = solver;
        _random = random ?? Random.Shared;
    }

    public SudokuBoard Generate(Difficulty difficulty)
    {
        var board = GenerateFullBoard();
        RemoveClues(board, difficulty);
        return board;
    }

    private SudokuBoard GenerateFullBoard()
    {
        var board = new SudokuBoard();
        FillDiagonalBoxes(board);
        return _solver.Solve(board);
    }

    private void FillDiagonalBoxes(SudokuBoard board)
    {
        // The three diagonal 3x3 boxes (top-left, center, bottom-right)
        // don't constrain each other, so random fill is always valid.
        for (var box = 0; box < 3; box++)
        {
            var digits = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Shuffle(digits);
            var idx = 0;
            var startRow = box * 3;
            var startCol = box * 3;
            for (var r = startRow; r < startRow + 3; r++)
            {
                for (var c = startCol; c < startCol + 3; c++)
                {
                    board[r, c] = digits[idx++];
                }
            }
        }
    }

    private void RemoveClues(SudokuBoard board, Difficulty difficulty)
    {
        // Stub — implemented in Task 5
    }

    private void Shuffle<T>(T[] array)
    {
        for (var i = array.Length - 1; i > 0; i--)
        {
            var j = _random.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test Sudoku.Tests --filter "FullyQualifiedName~Generate_ReturnsValidPuzzle" -v n`
Expected: PASS (Easy removes no clues yet, but the solved board is valid)

- [ ] **Step 5: Commit**

```bash
git add Sudoku.Core/Generators/SudokuGenerator.cs Sudoku.Tests/Generators/SudokuGeneratorTests.cs
git commit -m "Add SudokuGenerator with full board generation"
```

---

### Task 5: Implement Symmetric Clue Removal

This task implements Phase 2: removing clue pairs with 180-degree rotational symmetry until reaching the target clue count.

**Files:**
- Modify: `Sudoku.Core/Generators/SudokuGenerator.cs`
- Modify: `Sudoku.Tests/Generators/SudokuGeneratorTests.cs`

- [ ] **Step 1: Write failing tests for clue removal**

Add these tests to `Sudoku.Tests/Generators/SudokuGeneratorTests.cs`:

```csharp
[TestCase(Difficulty.Easy, 40, 46)]
[TestCase(Difficulty.Medium, 33, 39)]
[TestCase(Difficulty.Hard, 28, 32)]
[TestCase(Difficulty.Expert, 24, 27)]
[TestCase(Difficulty.Evil, 20, 23)]
public void Generate_ClueCountWithinRange(Difficulty difficulty, int min, int max)
{
    var generator = new SudokuGenerator(new SimpleSolver(), new Random(42));

    var puzzle = generator.Generate(difficulty);

    puzzle.ClueCount.Should().BeInRange(min, max);
}

[TestCase(Difficulty.Easy)]
[TestCase(Difficulty.Medium)]
[TestCase(Difficulty.Hard)]
[TestCase(Difficulty.Expert)]
[TestCase(Difficulty.Evil)]
public void Generate_HasRotationalSymmetry(Difficulty difficulty)
{
    var generator = new SudokuGenerator(new SimpleSolver(), new Random(42));

    var puzzle = generator.Generate(difficulty);

    for (var r = 0; r < 9; r++)
    {
        for (var c = 0; c < 9; c++)
        {
            var isEmpty = puzzle[r, c] == 0;
            var mirrorEmpty = puzzle[8 - r, 8 - c] == 0;
            isEmpty.Should().Be(mirrorEmpty,
                $"cell ({r},{c}) and its mirror ({8 - r},{8 - c}) should both be empty or both be filled");
        }
    }
}

[TestCase(Difficulty.Easy)]
[TestCase(Difficulty.Medium)]
[TestCase(Difficulty.Hard)]
[TestCase(Difficulty.Expert)]
[TestCase(Difficulty.Evil)]
public void Generate_HasUniqueSolution(Difficulty difficulty)
{
    var generator = new SudokuGenerator(new SimpleSolver(), new Random(42));

    var puzzle = generator.Generate(difficulty);

    new SimpleSolver().IsUnique(puzzle).Should().BeTrue();
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test Sudoku.Tests --filter "FullyQualifiedName~SudokuGeneratorTests" -v n`
Expected: `Generate_ClueCountWithinRange` fails (board is fully filled, 81 clues). Symmetry and uniqueness may pass trivially on a full board.

- [ ] **Step 3: Implement RemoveClues**

Replace the `RemoveClues` stub in `Sudoku.Core/Generators/SudokuGenerator.cs` with the full implementation. Replace from the `RemoveClues` method to the end of the `Shuffle` method:

```csharp
    private void RemoveClues(SudokuBoard board, Difficulty difficulty)
    {
        var (min, max) = GetClueRange(difficulty);
        var pairs = BuildSymmetricPairs();
        Shuffle(pairs);

        foreach (var (r1, c1, r2, c2) in pairs)
        {
            if (board.ClueCount <= max)
                break;

            var val1 = board[r1, c1];
            var val2 = (r1 == r2 && c1 == c2) ? val1 : board[r2, c2];

            board[r1, c1] = 0;
            if (r1 != r2 || c1 != c2)
                board[r2, c2] = 0;

            if (!_solver.IsUnique(board))
            {
                board[r1, c1] = val1;
                if (r1 != r2 || c1 != c2)
                    board[r2, c2] = val2;
            }
        }
    }

    private static (int r1, int c1, int r2, int c2)[] BuildSymmetricPairs()
    {
        var pairs = new List<(int r1, int c1, int r2, int c2)>();
        var visited = new bool[9, 9];

        for (var r = 0; r < 9; r++)
        {
            for (var c = 0; c < 9; c++)
            {
                if (visited[r, c])
                    continue;

                var mr = 8 - r;
                var mc = 8 - c;
                visited[r, c] = true;
                visited[mr, mc] = true;
                pairs.Add((r, c, mr, mc));
            }
        }

        return pairs.ToArray();
    }

    private static (int min, int max) GetClueRange(Difficulty difficulty) => difficulty switch
    {
        Difficulty.Easy => (40, 46),
        Difficulty.Medium => (33, 39),
        Difficulty.Hard => (28, 32),
        Difficulty.Expert => (24, 27),
        Difficulty.Evil => (20, 23),
        _ => throw new ArgumentOutOfRangeException(nameof(difficulty))
    };

    private void Shuffle<T>(T[] array)
    {
        for (var i = array.Length - 1; i > 0; i--)
        {
            var j = _random.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test Sudoku.Tests --filter "FullyQualifiedName~SudokuGeneratorTests" -v n`
Expected: All tests pass

- [ ] **Step 5: Run full test suite**

Run: `dotnet test`
Expected: All tests pass (no regressions)

- [ ] **Step 6: Commit**

```bash
git add Sudoku.Core/Generators/SudokuGenerator.cs Sudoku.Tests/Generators/SudokuGeneratorTests.cs
git commit -m "Implement symmetric clue removal for board generation"
```

---

### Task 6: Deterministic Seeding Test

Verify that the same seed produces the same board.

**Files:**
- Modify: `Sudoku.Tests/Generators/SudokuGeneratorTests.cs`

- [ ] **Step 1: Write the failing test**

Add to `SudokuGeneratorTests.cs`:

```csharp
[Test]
public void Generate_WithSameSeed_ProducesSameBoard()
{
    var generator1 = new SudokuGenerator(new SimpleSolver(), new Random(123));
    var generator2 = new SudokuGenerator(new SimpleSolver(), new Random(123));

    var puzzle1 = generator1.Generate(Difficulty.Medium);
    var puzzle2 = generator2.Generate(Difficulty.Medium);

    puzzle1.ShouldBeEqualTo(puzzle2);
}
```

Note: this test uses the existing `ShouldBeEqualTo` extension. Add `using Sudoku.Tests.Extensions;` to the top of the file if not already present.

- [ ] **Step 2: Run test to verify it passes**

Run: `dotnet test Sudoku.Tests --filter "FullyQualifiedName~WithSameSeed" -v n`
Expected: PASS (deterministic by construction — `Random` is injected)

- [ ] **Step 3: Commit**

```bash
git add Sudoku.Tests/Generators/SudokuGeneratorTests.cs
git commit -m "Add deterministic seeding test for generator"
```

---

### Task 7: Add Generation Benchmarks

**Files:**
- Modify: `Sudoku.Benchmarks/SudokuBenchmarks.cs`

- [ ] **Step 1: Add generation benchmarks**

Add the following to `Sudoku.Benchmarks/SudokuBenchmarks.cs`. Add `using Sudoku.Core.Generators;` to the top, then add a new field and benchmark method:

Add a new field after the existing `_sudokus` field:

```csharp
private readonly IGenerator _generator = new SudokuGenerator(new SimpleSolver());
```

Add a new benchmark method after the existing `SimpleSolveSudoku` method:

```csharp
[Benchmark]
[Arguments(Difficulty.Easy)]
[Arguments(Difficulty.Medium)]
[Arguments(Difficulty.Hard)]
[Arguments(Difficulty.Expert)]
[Arguments(Difficulty.Evil)]
public void GenerateSudoku(Difficulty difficulty)
{
    _generator.Generate(difficulty);
}
```

- [ ] **Step 2: Verify it builds**

Run: `dotnet build Sudoku.Benchmarks -c Release`
Expected: Build succeeded

- [ ] **Step 3: Commit**

```bash
git add Sudoku.Benchmarks/SudokuBenchmarks.cs
git commit -m "Add generation benchmarks per difficulty tier"
```

---

### Task 8: Final Verification

- [ ] **Step 1: Run full test suite**

Run: `dotnet test`
Expected: All tests pass

- [ ] **Step 2: Build entire solution**

Run: `dotnet build`
Expected: Build succeeded, no warnings

- [ ] **Step 3: Verify benchmark builds**

Run: `dotnet build Sudoku.Benchmarks -c Release`
Expected: Build succeeded
