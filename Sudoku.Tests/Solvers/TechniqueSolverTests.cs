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
}
