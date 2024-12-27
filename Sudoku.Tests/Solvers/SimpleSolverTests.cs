using Sudoku.Core.Solvers;
using Sudoku.Tests.Extensions;

namespace Sudoku.Tests.Solvers;

public class SimpleSolverTests
{
    private ISolver _solver;
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
    public void SolvingUniqueSudokuReturnsOneSolution()
    {
        var sudoku = _sudokus.Easy[0];

        _solver.IsUnique(sudoku.GetSudokuBoard()).Should().BeTrue();
    }

    [Test]
    public void SolvingNonuniqueSudokuReturnsMoreThanOneSolution()
    {
        SudokuBoard sudoku = _sudokus.Evil[0].GetSudokuBoard();
        sudoku.Sudoku[0][0] = 0;

        _solver.IsUnique(sudoku).Should().BeFalse();
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
