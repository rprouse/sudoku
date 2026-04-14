# Sudoku Board Generation Design

## Overview

Add the ability to generate Sudoku puzzles of a given difficulty with 180-degree rotational symmetry, a standard property of published Sudoku puzzles.

## Public API

### Difficulty Enum

```csharp
// Sudoku.Core/Difficulty.cs
namespace Sudoku.Core;

public enum Difficulty { Easy, Medium, Hard, Expert, Evil }
```

Matches the five existing tiers used in the JSON persistence layer (`SudokuJsonFile`).

### IGenerator Interface

```csharp
// Sudoku.Core/Generators/IGenerator.cs
namespace Sudoku.Core.Generators;

public interface IGenerator
{
    SudokuBoard Generate(Difficulty difficulty);
}
```

### SudokuGenerator Implementation

```csharp
// Sudoku.Core/Generators/SudokuGenerator.cs
namespace Sudoku.Core.Generators;

public class SudokuGenerator : IGenerator
{
    private readonly ISolver _solver;
    private readonly Random _random;

    public SudokuGenerator(ISolver solver, Random? random = null);

    public SudokuBoard Generate(Difficulty difficulty);
}
```

- Takes an `ISolver` dependency for solving and uniqueness checking.
- Optional `Random` parameter for deterministic/testable generation. Defaults to `Random.Shared`.
- Returns a puzzle board with clues and empty cells (0s). The caller solves it themselves if they want the solution.

## Algorithm

### Phase 1: Generate a Complete Board

1. Create an empty `SudokuBoard`.
2. Fill the three diagonal 3x3 boxes (top-left, center, bottom-right) with randomly shuffled 1-9. These boxes don't constrain each other, so random fill is always valid.
3. Call `_solver.Solve()` to fill the remaining cells.

### Phase 2: Remove Clues with 180-Degree Rotational Symmetry

1. Build a list of the 41 unique symmetric positions:
   - 36 pairs: `(r,c)` and `(8-r, 8-c)` where the two cells differ.
   - 1 center cell: `(4,4)` (its 180-degree mirror is itself).
2. Shuffle this list randomly.
3. Walk through the shuffled list:
   - Remove the pair (or single center cell) by setting cells to 0.
   - Check `_solver.IsUnique()`.
   - If not unique, restore the values and skip this pair.
   - If still unique, keep the removal.
4. Stop when the clue count drops to or below the **maximum** of the target range for the difficulty tier (i.e., stop removing once inside the range).
5. If the pair list is exhausted before reaching the target minimum, accept the result (the puzzle is still valid, just slightly easier than intended).

### Clue Count Ranges

| Difficulty | Target Clues | Pairs Removed (approx) |
|------------|-------------|------------------------|
| Easy       | 40-46       | 14-17                  |
| Medium     | 33-39       | 18-22                  |
| Hard       | 28-32       | 23-26                  |
| Expert     | 24-27       | 27-29                  |
| Evil       | 20-23       | 29-31                  |

These ranges are starting points and may be tuned based on results.

### Symmetry

180-degree rotational symmetry means: if cell `(r, c)` is a given clue, then cell `(8-r, 8-c)` is also a given clue. Clues are always removed in symmetric pairs, enforcing this invariant by construction.

## File Organization

```
Sudoku.Core/
  Difficulty.cs                    # Enum
  Generators/
    IGenerator.cs                  # Interface
    SudokuGenerator.cs             # Implementation

Sudoku.Tests/
  Generators/
    SudokuGeneratorTests.cs        # Tests

Sudoku.Benchmarks/
  SudokuBenchmarks.cs              # Add generation benchmarks
```

Mirrors the existing `Solvers/` folder structure.

## Testing Strategy

- Generated board `IsValid()` after solving
- Clue count falls within the target range for the requested difficulty
- 180-degree rotational symmetry: if `board[r,c] == 0` then `board[8-r, 8-c] == 0`
- Generated puzzle has a unique solution (`IsUnique()`)
- Seeded `Random` produces deterministic output
- Tests across all 5 difficulty tiers

## Benchmarks

- Add generation benchmarks per difficulty tier to `SudokuBenchmarks.cs`.

## Design Decisions

- **Clue count for difficulty** (not solver iterations): Clue count is the natural stopping condition since we remove clues in pairs. Solver iteration count from brute-force backtracking doesn't correlate well with human-perceived difficulty.
- **Simple pair removal** (no backtracking): Walk the shuffled pair list once. If we can't reach the target, accept the result. Backtracking can be added later if Evil-tier generation proves unreliable.
- **Instance class with ISolver dependency** (not static): Follows existing `SimpleSolver` pattern. Testable, injectable, swappable solver.
- **Diagonal box seeding**: The three diagonal 3x3 boxes are mutually independent, so filling them randomly is always valid and provides good entropy for the solver.
