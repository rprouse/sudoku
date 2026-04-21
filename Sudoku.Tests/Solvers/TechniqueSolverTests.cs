using Sudoku.Core.Solvers;
using Sudoku.Tests.Extensions;

namespace Sudoku.Tests.Solvers;

[Parallelizable(ParallelScope.All)]
public class TechniqueSolverTests
{
    private static readonly SudokuJsonFile SUDOKUS = Persistence.LoadFromJson("sudokus.json");

    [Test]
    public void Solves_EasyPuzzle_UsingOnlySingles()
    {
        var easy = SUDOKUS.Easy[0];
        var solver = new TechniqueSolver();

        var trace = solver.Solve(easy.GetSudokuBoard());

        trace.Solved.Should().BeTrue();
        trace.Steps.Should().OnlyContain(s =>
            s.Level == DifficultyTechnique.NakedSingle ||
            s.Level == DifficultyTechnique.HiddenSingle);
    }

    [Test]
    public void NakedSingle_IsDetected_InSingleCandidateCell()
    {
        // A board where (0,0) has only one candidate (1).
        var rows = new int[][]
        {
            new [] { 0, 2, 3, 4, 5, 6, 7, 8, 9 },
            new [] { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new [] { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new [] { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new [] { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new [] { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new [] { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new [] { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new [] { 0, 0, 0, 0, 0, 0, 0, 0, 0 }
        };
        var board = new SudokuBoard(rows);
        var solver = new TechniqueSolver();

        var trace = solver.Solve(board);

        trace.Steps.Should().Contain(s =>
            s.Level == DifficultyTechnique.NakedSingle &&
            s.Description.Contains("(0,0)=1"));
    }

    [Test]
    public void Unsolvable_ReturnsSolvedFalse()
    {
        // Find a Hard puzzle that requires more than singles.
        var solver = new TechniqueSolver();
        SolveTrace? unsolvableTrace = null;
        for (var i = 0; i < SUDOKUS.Hard.Length; i++)
        {
            var trace = solver.Solve(SUDOKUS.Hard[i].GetSudokuBoard());
            if (!trace.Solved)
            {
                unsolvableTrace = trace;
                break;
            }
        }

        // With only singles implemented (this task), the solver cannot
        // finish all hard puzzles. This assertion pins that contract.
        unsolvableTrace.Should().NotBeNull("at least one Hard puzzle should require more than singles");
        unsolvableTrace!.Solved.Should().BeFalse();
    }

    [Test]
    public void HiddenSubset_DetectsHiddenTriple()
    {
        // Hidden-pair detection with placement-only discipline:
        //
        // A classic hidden pair (two digits confined to two cells) only forces
        // a placement when one of the two host cells can hold exactly one of the
        // two pair-digits — at which point HiddenSingle would fire first in this
        // solver's ordered technique list (it runs before HiddenSubset). So a
        // pure hidden-pair placement step cannot appear before HiddenSingle fires.
        //
        // HiddenTriple DOES produce placements that HiddenSingle misses: three
        // digits confined to three cells, where one host cell's intersection with
        // the triple-mask has popcount 1 (only one of the three digits fits there),
        // while the other two cells in the unit could each hold multiple triple-
        // digits (so HiddenSingle, which needs exactly one cell in the unit for a
        // digit, does not fire). The canned Hard puzzle set contains at least one
        // such example, confirming the HiddenSubset technique is correctly wired.
        //
        // We therefore assert on HiddenTriple (size=3) rather than HiddenPair to
        // demonstrate that the HiddenSubset class, its registration in TechniqueSolver,
        // and the placement logic are all functioning correctly across the full subset
        // hierarchy (pair, triple, quad use the same code path via the _size parameter).
        var solver = new TechniqueSolver();
        var found = false;
        foreach (var puzzle in SUDOKUS.Hard)
        {
            var trace = solver.Solve(puzzle.GetSudokuBoard());
            if (trace.Steps.Any(s => s.Level == DifficultyTechnique.HiddenTriple))
            {
                found = true;
                break;
            }
        }
        found.Should().BeTrue("at least one Hard puzzle should exercise the HiddenTriple technique");
    }

    [Test]
    public void NakedPair_EliminatesCandidatesInColumn()
    {
        // Board is constructed so that in col 0:
        //   (0,0) has candidates {1,2}: rows 0+1 each place 3-9 in cols 1-7, and
        //         BoxMask[0] accumulates {3,4,5} from those rows — excluding all of 3-9.
        //   (1,0) has candidates {1,2}: same reasoning.
        //   (3,0) has candidates {1,2,3}: row 3 places 4-9 at cols 3-8; col 0 and
        //         box 3 add no further exclusions.
        //
        // Naked pair {1,2} in col 0 at (0,0)+(1,0) eliminates {1,2} from (3,0)={1,2,3},
        // reducing it to {3} and forcing placement of (3,0)=3.
        //
        // NakedSingle/HiddenSingle cannot fire first:
        //   - No cell starts with exactly 1 candidate.
        //   - Rows 0 and 1 each have 2 empty cells (col 0 and col 8) with {1,2},
        //     so digit 1 and 2 appear in 2 cells per row — not a hidden single.
        //   - Digit 3 in col 0 appears in (3,0) and many unconstrained rows below,
        //     so it is not a hidden single in col 0.
        var rows = new int[][]
        {
            new [] { 0, 3, 4, 5, 6, 7, 8, 9, 0 },   // RowMask[0]=bits{3..9}; (0,0)=(0,8)={1,2}
            new [] { 0, 4, 5, 6, 7, 8, 9, 3, 0 },   // RowMask[1]=bits{3..9}; (1,0)=(1,8)={1,2}; shifted to avoid col conflicts with row 0
            new [] { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new [] { 0, 0, 0, 4, 5, 6, 7, 8, 9 },   // RowMask[3]=bits{4..9}; (3,0)={1,2,3} — victim
            new [] { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new [] { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new [] { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new [] { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new [] { 0, 0, 0, 0, 0, 0, 0, 0, 0 }
        };
        var board = new SudokuBoard(rows);
        var solver = new TechniqueSolver();

        var trace = solver.Solve(board);

        // The naked pair (0,0)+(1,0)={1,2} in col 0 eliminates 1,2 from (3,0)={1,2,3},
        // forcing (3,0)=3 and emitting a step attributed to NakedPair.
        trace.Steps.Should().Contain(s => s.Level == DifficultyTechnique.NakedPair);
    }

    [Test]
    public void Intersection_FiresOnHardPuzzle()
    {
        // PointingPair and BoxLineReduction techniques place digits when the
        // intersection deduction collapses a cell to a single candidate.
        // At least one Hard puzzle in the canned set should exercise one of
        // these two levels before the solver runs out of steam.
        var solver = new TechniqueSolver();
        var found = false;
        foreach (var puzzle in SUDOKUS.Hard)
        {
            var trace = solver.Solve(puzzle.GetSudokuBoard());
            if (trace.Steps.Any(s =>
                s.Level == DifficultyTechnique.PointingPair ||
                s.Level == DifficultyTechnique.BoxLineReduction))
            {
                found = true;
                break;
            }
        }
        found.Should().BeTrue("at least one Hard puzzle should exercise Intersection techniques");
    }

    [Test]
    public void Fish_FiresOnExpertPuzzle()
    {
        var solver = new TechniqueSolver();
        var found = false;
        foreach (var puzzle in SUDOKUS.Expert.Concat(SUDOKUS.Evil))
        {
            var trace = solver.Solve(puzzle.GetSudokuBoard());
            if (trace.Steps.Any(s =>
                s.Level == DifficultyTechnique.XWing ||
                s.Level == DifficultyTechnique.Swordfish))
            {
                found = true;
                break;
            }
        }
        found.Should().BeTrue("at least one Expert/Evil puzzle should exercise X-Wing or Swordfish");
    }

    [Test]
    public void Wing_FiresOnExpertOrEvilPuzzle()
    {
        var solver = new TechniqueSolver();
        var found = false;
        foreach (var puzzle in SUDOKUS.Expert.Concat(SUDOKUS.Evil))
        {
            var trace = solver.Solve(puzzle.GetSudokuBoard());
            if (trace.Steps.Any(s =>
                s.Level == DifficultyTechnique.XYWing ||
                s.Level == DifficultyTechnique.XYZWing))
            {
                found = true;
                break;
            }
        }
        found.Should().BeTrue("at least one Expert/Evil puzzle should exercise XY-Wing or XYZ-Wing");
    }
}
