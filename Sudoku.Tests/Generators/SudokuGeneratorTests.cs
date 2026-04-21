using Sudoku.Core.Generators;
using Sudoku.Core.Solvers;
using Sudoku.Tests.Extensions;

namespace Sudoku.Tests.Generators;

[Parallelizable(ParallelScope.All)]
public class SudokuGeneratorTests
{
    private static SudokuGenerator CreateGenerator(int seed) =>
        new SudokuGenerator(new BitmaskSolver(), new DifficultyGrader(new TechniqueSolver()), new Random(seed));

    [Test]
    public void Generate_ReturnsValidPuzzle()
    {
        var generator = CreateGenerator(42);

        var puzzle = generator.Generate(Difficulty.Easy);
        var solution = new BitmaskSolver().Solve(puzzle);

        solution.IsFilled().Should().BeTrue();
        solution.IsValid().Should().BeTrue();
    }

    [TestCase(Difficulty.Easy, 40, 50)]
    [TestCase(Difficulty.Medium, 32, 40)]
    [TestCase(Difficulty.Hard, 27, 34)]
    [TestCase(Difficulty.Expert, 24, 29)]
    [TestCase(Difficulty.Evil, 22, 27)]
    public void Generate_ClueCountWithinRange(Difficulty difficulty, int min, int max)
    {
        var generator = CreateGenerator(42);

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
        var generator = CreateGenerator(42);

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
        var generator = CreateGenerator(42);

        var puzzle = generator.Generate(difficulty);

        new BitmaskSolver().IsUnique(puzzle).Should().BeTrue();
    }

    [TestCase(Difficulty.Easy)]
    [TestCase(Difficulty.Medium)]
    [TestCase(Difficulty.Hard)]
    [TestCase(Difficulty.Expert)]
    [TestCase(Difficulty.Evil)]
    public void Generate_GradesAsTargetTier(Difficulty difficulty)
    {
        var generator = CreateGenerator(42);
        var grader = new DifficultyGrader(new TechniqueSolver());

        var puzzle = generator.Generate(difficulty);

        grader.Grade(puzzle).Should().Be(difficulty);
    }

    [Test]
    public void Generate_WithSameSeed_ProducesSameBoard()
    {
        var generator1 = CreateGenerator(123);
        var generator2 = CreateGenerator(123);

        var puzzle1 = generator1.Generate(Difficulty.Medium);
        var puzzle2 = generator2.Generate(Difficulty.Medium);

        puzzle1.ShouldBeEqualTo(puzzle2);
    }
}
