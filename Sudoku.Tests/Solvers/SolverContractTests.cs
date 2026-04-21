using Sudoku.Core.Solvers;
using Sudoku.Tests.Extensions;

namespace Sudoku.Tests.Solvers;

[Parallelizable(ParallelScope.All)]
public abstract class SolverContractTests
{
    private ISolver _solver = null!;
    private SudokuJsonFile _sudokus = null!;

    private static readonly SudokuJsonFile SUDOKUS = Persistence.LoadFromJson("sudokus.json");

    protected abstract ISolver CreateSolver();

    [SetUp]
    public void Setup()
    {
        _solver = CreateSolver();
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
        sudoku[0, 0] = 0;

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
        var mediumSudoku = _sudokus.Medium[0];
        var solution = _solver.Solve(mediumSudoku.GetSudokuBoard());

        solution.ShouldBeEqualTo(mediumSudoku.GetSolutionBoard());
    }

    [Test]
    public void CanSolveHardSudoku()
    {
        var hardSudoku = _sudokus.Hard[0];
        var solution = _solver.Solve(hardSudoku.GetSudokuBoard());

        solution.ShouldBeEqualTo(hardSudoku.GetSolutionBoard());
    }

    [Test]
    public void CanSolveExpertSudoku()
    {
        var expertSudoku = _sudokus.Expert[0];
        var solution = _solver.Solve(expertSudoku.GetSudokuBoard());

        solution.ShouldBeEqualTo(expertSudoku.GetSolutionBoard());
    }

    [Test]
    public void CanSolveEvilSudoku()
    {
        var evilSudoku = _sudokus.Evil[0];
        var solution = _solver.Solve(evilSudoku.GetSolutionBoard());

        solution.ShouldBeEqualTo(evilSudoku.GetSolutionBoard());
    }

    [Explicit("This test is too slow to run on every build.")]
    [TestCaseSource(nameof(SudokuBoards))]
    public void CanSolveAllSudokus(SudokuJsonBoard board)
    {
        var solution = _solver.Solve(board.GetSudokuBoard());

        solution.ShouldBeEqualTo(board.GetSolutionBoard());
    }

    public static IEnumerable<SudokuJsonBoard> SudokuBoards() =>
        SUDOKUS.Easy
            .Union(SUDOKUS.Medium)
            .Union(SUDOKUS.Hard)
            .Union(SUDOKUS.Expert)
            .Union(SUDOKUS.Evil);
}
