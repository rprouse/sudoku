using Sudoku.Core.Solvers;
using Sudoku.Tests.Extensions;

namespace Sudoku.Tests.Solvers;

public class SimpleSolverTests
{
    private SimpleSolver _solver;
    private SudokuJsonFile _sudokus;

    [SetUp]
    public void Setup()
    {
        _solver = new SimpleSolver();
        _sudokus = Persistence.LoadFromJson("sudokus.json");
    }

    [TearDown]
    public void TearDown()
    {
        TestContext.WriteLine($"Iterations: {_solver.Iterations}");
    }

    [Test]
    public void SolvingAlreadySolvedSudokuReturnsSameSudoku()
    {
        var easySudoku = _sudokus.Easy[0];
        var solution = _solver.Solve(easySudoku.GetSolutionBoard());

        solution.ShouldBeEqualTo(easySudoku.GetSolutionBoard());
    }

    [Test]
    public void CanSolveEasySudoku()
    {
        var easySudoku = _sudokus.Easy[0];
        var solution = _solver.Solve(easySudoku.GetSudokuBoard());

        solution.ShouldBeEqualTo(easySudoku.GetSolutionBoard());
    }

    [Test]
    public void CanSolveMediumSudoku()
    {
        var easySudoku = _sudokus.Medium[0];
        var solution = _solver.Solve(easySudoku.GetSudokuBoard());

        solution.ShouldBeEqualTo(easySudoku.GetSolutionBoard());
    }

    [Test]
    public void CanSolveHardSudoku()
    {
        var easySudoku = _sudokus.Hard[0];
        var solution = _solver.Solve(easySudoku.GetSudokuBoard());

        solution.ShouldBeEqualTo(easySudoku.GetSolutionBoard());
    }

    [Test]
    public void CanSolveExpertSudoku()
    {
        var easySudoku = _sudokus.Expert[0];
        var solution = _solver.Solve(easySudoku.GetSudokuBoard());

        solution.ShouldBeEqualTo(easySudoku.GetSolutionBoard());
    }

    [Test]
    public void CanSolveEvilSudoku()
    {
        var easySudoku = _sudokus.Evil[0];
        var solution = _solver.Solve(easySudoku.GetSudokuBoard());

        solution.ShouldBeEqualTo(easySudoku.GetSolutionBoard());
    }
}
