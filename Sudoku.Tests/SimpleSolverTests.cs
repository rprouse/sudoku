using Sudoku.Tests.Extensions;

namespace Sudoku.Tests;

public class SimpleSolverTests
{
    private SudokuJsonFile _sudokus;

    [SetUp]
    public void Setup()
    {
        _sudokus = Persistence.LoadFromJson("sudokus.json");
    }

    [Test]
    public void SolvingAlreadySolvedSudokuReturnsSameSudoku()
    {
        var easySudoku = _sudokus.Easy[0];
        var solution = SimpleSolver.Solve(easySudoku.GetSolutionBoard());

        solution.ShouldBeEqualTo(easySudoku.GetSolutionBoard());
    }

    [Test]
    public void CanSolveEasySudoku()
    {
        var easySudoku = _sudokus.Easy[0];
        var solution = SimpleSolver.Solve(easySudoku.GetSudokuBoard());

        solution.ShouldBeEqualTo(easySudoku.GetSolutionBoard());
    }

    [Test]
    public void CanSolveMediumSudoku()
    {
        var easySudoku = _sudokus.Medium[0];
        var solution = SimpleSolver.Solve(easySudoku.GetSudokuBoard());

        solution.ShouldBeEqualTo(easySudoku.GetSolutionBoard());
    }

    [Test]
    public void CanSolveHardSudoku()
    {
        var easySudoku = _sudokus.Hard[0];
        var solution = SimpleSolver.Solve(easySudoku.GetSudokuBoard());

        solution.ShouldBeEqualTo(easySudoku.GetSolutionBoard());
    }

    [Test]
    public void CanSolveExpertSudoku()
    {
        var easySudoku = _sudokus.Expert[0];
        var solution = SimpleSolver.Solve(easySudoku.GetSudokuBoard());

        solution.ShouldBeEqualTo(easySudoku.GetSolutionBoard());
    }

    [Test]
    public void CanSolveEvilSudoku()
    {
        var easySudoku = _sudokus.Evil[0];
        var solution = SimpleSolver.Solve(easySudoku.GetSudokuBoard());

        solution.ShouldBeEqualTo(easySudoku.GetSolutionBoard());
    }
}
