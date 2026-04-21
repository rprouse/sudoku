using Sudoku.Core.Generators;
using Sudoku.Core.Solvers;
using Sudoku.Tests.Extensions;

namespace Sudoku.Tests.Generators;

[Parallelizable(ParallelScope.All)]
public class DifficultyGraderTests
{
    private static readonly SudokuJsonFile SUDOKUS = Persistence.LoadFromJson("sudokus.json");

    [Test]
    public void Grade_EasyPuzzle_ReturnsEasy()
    {
        var grader = new DifficultyGrader(new TechniqueSolver());
        var puzzle = SUDOKUS.Easy[0].GetSudokuBoard();

        var grade = grader.Grade(puzzle);

        grade.Should().Be(Difficulty.Easy);
    }

    [Test]
    public void Grade_UnsolvableByLogic_ReturnsNull()
    {
        // Create a non-unique (underdetermined) board by removing cells from an Evil puzzle.
        // Non-unique puzzles cannot be solved by logic alone.
        var puzzle = SUDOKUS.Evil[0].GetSudokuBoard();
        puzzle[0, 0] = 0;
        puzzle[0, 1] = 0;
        puzzle[0, 2] = 0;
        puzzle[0, 3] = 0;
        puzzle[0, 4] = 0;
        puzzle[0, 5] = 0;

        var grader = new DifficultyGrader(new TechniqueSolver());
        var grade = grader.Grade(puzzle);

        grade.Should().BeNull();
    }

    [Test]
    public void Grade_AlreadySolvedPuzzle_ReturnsEasy()
    {
        // A fully-filled valid board has 0 solve steps. Grader should still return Easy
        // (the lowest tier) as a sensible default rather than null.
        var solution = SUDOKUS.Easy[0].GetSolutionBoard();

        var grader = new DifficultyGrader(new TechniqueSolver());
        var grade = grader.Grade(solution);

        grade.Should().Be(Difficulty.Easy);
    }
}
