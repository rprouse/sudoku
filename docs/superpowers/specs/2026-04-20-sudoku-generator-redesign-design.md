# Sudoku Generator Redesign — Design Spec

**Date:** 2026-04-20
**Branch:** `feature/generator-redesign`
**Status:** Approved — ready for implementation plan

## Problem

The current puzzle generator has two observable issues:

1. **Generation is slow.** Hard and Evil puzzles can take more than a minute.
2. **Difficulty is inaccurate.** The hardest tiers are often easy to solve in practice.

Both stem from the same pair of root causes: the brute-force solver (`SimpleSolver`) is LINQ-heavy and allocates aggressively on every placement, and difficulty is measured by clue count alone — a well-known poor proxy for actual solving difficulty.

## Goals

- **Performance:** generation completes in under 1 s for Gentle/Steady/Challenging, under 3 s for Deep, under 5 s for Profound on a typical development machine.
- **Difficulty accuracy:** difficulty is determined by the hardest *human-solvable technique* required to solve the puzzle, not by clue count.
- **Solvability guarantee:** every generated puzzle is solvable using pure logic (no trial-and-error required).
- **Preserve existing public contracts:** `ISolver`, `IGenerator`, and the seeded-`Random` determinism still work; MAUI DI requires only a small, clear update.

## Non-goals

- Relaxing or varying symmetry per tier (explicitly deferred — see "Future optimizations").
- Changing the enum values of `Difficulty` (they remain `Easy`, `Medium`, `Hard`, `Expert`, `Evil` because they're serialized in saved games). Display names continue to come from `DifficultyExtensions.DisplayName()`.
- Adding a hint system in the app (the `TechniqueSolver` makes this feasible later but is out of scope here).
- Changes to the MAUI UI beyond a DI wiring update.

## Decisions (already made during brainstorming)

1. **Difficulty metric:** hybrid — hardest technique required as primary, with a secondary clue-count range as a sanity filter.
2. **Symmetry:** strict 180° rotational symmetry at every tier (current behavior). Relaxation for the hardest tiers is documented as a future optimization in README.
3. **Time budget:** "Comfortable" — <1 s (Gentle/Steady/Challenging), <3 s (Deep), <5 s (Profound).
4. **Technique tier mapping:** conventional (see table below).
5. **Generation strategy:** Strategy 1 ("Grade and stop") — regrade after each successful pair removal; stop when the next removal would push past the target tier.
6. **`SimpleSolver` disposition:** kept as a reference implementation in `Sudoku.Core/Solvers/`, used by tests and benchmarks but not by the MAUI app.

## Difficulty tier mapping

| Tier (display name)     | Hardest technique required                               | What it adds                    |
| ----------------------- | -------------------------------------------------------- | ------------------------------- |
| Easy (Gentle)           | Naked Single / Hidden Single                             | —                               |
| Medium (Steady)         | Naked Pair / Hidden Pair                                 | Basic subset reasoning          |
| Hard (Challenging)      | Pointing Pair, Box/Line Reduction, Naked/Hidden Triple   | Intersection reasoning          |
| Expert (Deep)           | X-Wing, XY-Wing, Simple Coloring                         | Fish + wing patterns            |
| Evil (Profound)         | Swordfish, XYZ-Wing, X-Chain                             | Larger fish + basic chains      |

**Rule:** tier = the hardest technique that was *actually required* to solve the puzzle, not merely applicable. A puzzle solvable with only singles is Easy even if an X-Wing opportunity exists.

## Clue-count ranges (secondary filter)

Updated from the current ranges to better match typical clue densities per tier:

| Tier     | Clue range |
| -------- | ---------- |
| Easy     | 40–50      |
| Medium   | 32–40      |
| Hard     | 27–34      |
| Expert   | 24–29      |
| Evil     | 22–27      |

These ranges overlap slightly at tier boundaries, and that's intentional: the tier (hardest technique required) is the primary acceptance criterion, and the clue range exists only as a sanity filter to reject extreme mismatches — e.g., an "Easy" puzzle that happens to have only 24 clues because nothing harder than singles was ever required. With overlapping ranges, the grader makes the final call at the boundaries.

## Architecture

```
Sudoku.Core/
├─ Solvers/
│   ├─ ISolver.cs                  (unchanged)
│   ├─ SimpleSolver.cs             (kept, unchanged — reference implementation)
│   ├─ BitmaskSolver.cs            (NEW — fast brute-force, MAUI default)
│   ├─ ITechniqueSolver.cs         (NEW)
│   ├─ TechniqueSolver.cs          (NEW — emits a SolveTrace)
│   ├─ SolveStep.cs                (NEW)
│   ├─ SolveTrace.cs               (NEW)
│   └─ Techniques/
│       ├─ ITechnique.cs           (NEW)
│       ├─ NakedSingle.cs          (NEW)
│       ├─ HiddenSingle.cs         (NEW)
│       ├─ NakedSubset.cs          (NEW — pair/triple/quad)
│       ├─ HiddenSubset.cs         (NEW)
│       ├─ Intersection.cs         (NEW — pointing + box/line)
│       ├─ Fish.cs                 (NEW — X-Wing, Swordfish)
│       ├─ Wing.cs                 (NEW — XY-Wing, XYZ-Wing)
│       └─ Coloring.cs             (NEW — simple coloring, X-chain)
├─ Generators/
│   ├─ IGenerator.cs               (unchanged)
│   ├─ SudokuGenerator.cs          (UPDATED — Strategy 1 loop)
│   └─ DifficultyGrader.cs         (NEW)
└─ Difficulty.cs                   (unchanged)

Sudoku.Tests/
└─ Solvers/
    ├─ SolverContractTests.cs      (NEW — abstract base)
    ├─ SimpleSolverTests.cs        (REDUCED — 4-line concrete subclass)
    ├─ BitmaskSolverTests.cs       (NEW — 4-line concrete subclass)
    ├─ TechniqueSolverTests.cs     (NEW)
    └─ …
└─ Generators/
    ├─ SudokuGeneratorTests.cs     (UPDATED)
    └─ DifficultyGraderTests.cs    (NEW)

Sudoku.App/
└─ MauiProgram.cs                  (UPDATED — DI now uses BitmaskSolver + DifficultyGrader)
```

### BitmaskSolver

Replaces the brute-force hot path. Implements `ISolver`, public API unchanged.

**Internal state (not exposed):**

```csharp
private struct SolverState
{
    public int[,] grid;
    public short[] rowMask;   // bit d-1 set if digit d used in row
    public short[] colMask;
    public short[] boxMask;
    public int emptyCount;
}
```

**Algorithm:**

1. Ingest the `SudokuBoard` into `SolverState` once.
2. Recursive backtrack:
   - Pick the empty cell with fewest candidates (MRV). Candidates = `~(rowMask[r] | colMask[c] | boxMask[b]) & 0x1FF`.
   - For each candidate bit: set the digit, XOR the bit into row/col/box masks, recurse, undo on backtrack.
   - If any empty cell has zero candidates, fail immediately.
3. `Solve(board)`: return first solution found.
4. `IsUnique(board)`: continue after the first solution; return `false` as soon as a second is found.

**Why this is dramatically faster than `SimpleSolver`:**

- No per-placement `IsValid()` scan of 9 rows × 9 cols × 9 boxes with LINQ. Validity is implicit in the masks.
- No `SudokuBoard` allocation during recursion — one working state is mutated and undone on backtrack.
- MRV prunes the tree aggressively: most-constrained cells often have only 1–2 candidates, so bad branches fail fast.

### TechniqueSolver and ITechnique

The engine that grades puzzles. Applies human techniques in order of difficulty; emits a `SolveTrace`.

```csharp
public interface ITechnique
{
    DifficultyTechnique Level { get; }
    string Name { get; }
    bool TryApply(SolverState state, out SolveStep step);
}

public record SolveStep(string Technique, DifficultyTechnique Level, string Description);

public record SolveTrace(IReadOnlyList<SolveStep> Steps, bool Solved)
{
    public DifficultyTechnique? HardestTechnique { get; }
}
```

**`DifficultyTechnique` enum** (finer-grained than `Difficulty`):

```csharp
public enum DifficultyTechnique
{
    NakedSingle, HiddenSingle,                         // → Easy
    NakedPair, HiddenPair,                             // → Medium
    NakedTriple, HiddenTriple, NakedQuad, HiddenQuad,
    PointingPair, BoxLineReduction,                    // → Hard
    XWing, XYWing, SimpleColoring,                     // → Expert
    Swordfish, XYZWing, XChain                         // → Evil
}
```

**Solver state for techniques:** extended with per-cell candidate masks (a `short[9,9]` of 9-bit masks) in addition to row/col/box masks, since techniques like hidden pair and X-Wing reason about per-cell candidates directly.

**Solve loop:**

1. Build initial state with candidate masks.
2. Walk techniques in order (easiest first).
3. First technique whose `TryApply` returns true: record the `SolveStep`, apply the change (set a digit and/or eliminate candidates), restart the loop from the top.
4. If no technique applies:
   - If the board is full: `Solved = true`.
   - Otherwise: `Solved = false` (puzzle would need guessing — no brute-force fallback).
5. Return `SolveTrace`.

**Why "restart from top" matters:** grading by "hardest technique used" requires that simpler deductions are surfaced via their simpler techniques. Without restart, a one-off X-Wing followed by 40 naked singles would still be graded correctly, but we must not incorrectly attribute those singles to X-Wing.

### DifficultyGrader

Thin wrapper:

```csharp
public class DifficultyGrader
{
    public DifficultyGrader(ITechniqueSolver solver);
    public Difficulty? Grade(SudokuBoard puzzle);   // null = unsolvable by pure logic → reject
}
```

Runs `TechniqueSolver.Solve`, takes the hardest `DifficultyTechnique` in the trace (or returns `null` if `Solved == false`), and maps it to the `Difficulty` tier using the mapping table above.

### SudokuGenerator (Strategy 1 loop)

Constructor:

```csharp
public SudokuGenerator(ISolver solver, DifficultyGrader grader, Random? random = null)
```

`Generate(difficulty)`:

1. Up to `MaxAttempts` (= 10):
   - `TryGenerate(difficulty)` — returns a puzzle or `null`.
   - Return the puzzle on the first non-null result.
2. If all attempts fail: throw `InvalidOperationException("Generator exceeded max attempts.")`.

`TryGenerate(target)`:

1. Build a full random solution with `BitmaskSolver` (seeded, deterministic). The "fill 3 diagonal boxes first" pattern is no longer needed — the bitmask solver is fast enough to fill an empty board directly via MRV + shuffled branch order.
2. Build the 41 symmetric pairs (40 pair removals + 1 center cell); shuffle.
3. For each pair, call `TryRemovePair(board, r1, c1, r2, c2, target)`:
   - Tentatively remove both cells.
   - If `ISolver.IsUnique` returns false: restore both cells.
   - Else regrade; if grade is `null` or greater than target tier: restore both cells.
   - Else keep the removal.
4. After all pairs tried: regrade final board.
   - If grade equals target AND clue count is within the tier's range: return the puzzle.
   - Else: return `null` (retry).

**Expensive-check-last ordering:** `IsUnique` (milliseconds on `BitmaskSolver`) is called before `Grade` (tens of milliseconds on `TechniqueSolver`), so bad removals fail fast without paying the grader cost.

### MAUI DI update (`MauiProgram.cs`)

```csharp
builder.Services.AddSingleton<ISolver, BitmaskSolver>();
builder.Services.AddSingleton<ITechniqueSolver, TechniqueSolver>();
builder.Services.AddSingleton<DifficultyGrader>();
builder.Services.AddSingleton<SudokuGenerator>();
```

No other app changes. `MenuViewModel` already takes a `SudokuGenerator` + `ISolver` and will get them correctly.

## Testing

### Shared solver contract (`SolverContractTests`)

Abstract `[TestFixture]` with `protected abstract ISolver CreateSolver()`. Both solvers are required to be observationally identical for any valid puzzle. Tests:

- `SolvingAlreadySolvedSudokuReturnsSameSudoku`
- `SolvingUniqueSudokuReturnsOneSolution`
- `SolvingNonuniqueSudokuReturnsMoreThanOneSolution`
- `CanSolveEasySudoku` … `CanSolveEvilSudoku`
- `[Explicit] CanSolveAllSudokus` (full canned-puzzle run)

Concrete subclasses (`SimpleSolverTests`, `BitmaskSolverTests`) are ~4 lines each, overriding the factory. Each concrete fixture may add solver-specific tests as needed.

### TechniqueSolverTests

- One hand-crafted board per technique, asserting that exactly the expected technique is the one that makes progress.
- One full-puzzle smoke test per tier (solve a known puzzle of that tier, assert `Solved == true` and hardest technique is within the tier's set).

### DifficultyGraderTests

- Grade the canned puzzles from `sudokus.json`; assert each lands in a plausible tier (range assertion — the canned puzzles came from a third-party source with its own labels, so exact match isn't guaranteed).
- Grade a puzzle that requires guessing; assert `Grade` returns `null`.

### SudokuGeneratorTests (updated)

Keep:

- `Generate_ReturnsValidPuzzle`
- `Generate_HasRotationalSymmetry` (strict at every tier, consistent with the "strict symmetry" decision)
- `Generate_HasUniqueSolution`
- `Generate_WithSameSeed_ProducesSameBoard`

Update:

- `Generate_ClueCountWithinRange` — new ranges from the table above.

Add:

- `Generate_GradesAsTargetTier` — for each `Difficulty`, generate and assert `grader.Grade(puzzle) == difficulty`. This is the new core contract.

### Benchmarks (`SudokuBenchmarks.cs`)

- Keep `GenerateSudoku` — now measures the redesigned path.
- Add `BitmaskSolveSudoku` alongside `SimpleSolveSudoku` to make the speedup visible.
- Add `GradePuzzle` over the canned-puzzle set.

Time-budget targets are documented in README; they are not enforced as test failures, since BenchmarkDotNet runs are separate from the unit-test pipeline.

## Error handling

- `DifficultyGrader.Grade` returns `Difficulty?`. `null` means "unsolvable by pure logic" → generator rejects.
- `SudokuGenerator.Generate` throws `InvalidOperationException("Generator exceeded max attempts.")` if all `MaxAttempts` attempts fail. This is a safety valve; with Strategy 1 it should not fire in practice.
- **No silent brute-force fallback in the technique solver.** If techniques exhaust and the board is not full, we return `Solved = false`. Brute-forcing would invalidate the "logic-only" guarantee.
- Solvers preserve existing throw-on-invalid-board behavior.

## README additions

Three new sections in the top-level `README.md`:

1. **Performance targets** — the time budgets from the "Goals" section.
2. **Difficulty grading** — the tier table, with a short explanation of the "hardest technique actually required" rule.
3. **Future optimizations** — the deferred decision (relax symmetry for Deep/Profound), including rationale: strict symmetry caps the space of reachable puzzles slightly at the hardest tiers but preserves the game's visual aesthetic; if users report that Deep/Profound puzzles still feel too easy after the technique-grading change lands, relaxation is the next lever. The future change would be ~15 lines in `SudokuGenerator.RemoveClues`: a second pass that attempts individual-cell removals after the symmetric-pair pass, gated by a per-difficulty `SymmetryPolicy` enum.

## CLAUDE.md additions

Document the new gotchas:

- `BitmaskSolver` is the app-default `ISolver`; `SimpleSolver` is kept for tests and benchmarks only.
- Adding a new technique requires: (1) a new file in `Sudoku.Core/Solvers/Techniques/`, (2) an entry in `DifficultyTechnique`, (3) registration in `TechniqueSolver`'s technique list (order matters — easiest first), (4) an entry in `DifficultyGrader`'s technique-to-tier map, (5) a test in `TechniqueSolverTests`.
- The technique solver never brute-forces. If adding a technique that "just guesses," it doesn't belong here.

## Out of scope / explicitly deferred

- **Symmetry relaxation** (option B from brainstorming). Documented as future optimization.
- **User-configurable symmetry setting.** Not planned.
- **Hint / step-by-step solver UI.** `TechniqueSolver` makes this feasible but is not part of this spec.
- **More advanced techniques** (Jellyfish, ALS, Forcing Chains, Uniqueness Rectangles, Death Blossom). The mapping stops at X-Chain/Swordfish/XYZ-Wing for Evil; harder puzzles than that get rejected by the grader. Adding more techniques later is mechanical.
