using Sudoku.Core.Generators;
using Sudoku.Core.Solvers;
using Sudoku.Tests.Extensions;

namespace Sudoku.Tests.Generators;

[Parallelizable(ParallelScope.All)]
public class SudokuGeneratorTests
{
    [Test]
    public void Generate_ReturnsValidPuzzle()
    {
        var generator = new SudokuGenerator(new SimpleSolver(), new Random(42));

        var puzzle = generator.Generate(Difficulty.Easy);
        var solution = new SimpleSolver().Solve(puzzle);

        solution.IsFilled().Should().BeTrue();
        solution.IsValid().Should().BeTrue();
    }

    [TestCase(Difficulty.Easy, 40, 46)]
    [TestCase(Difficulty.Medium, 33, 39)]
    [TestCase(Difficulty.Hard, 28, 32)]
    [TestCase(Difficulty.Expert, 24, 27)]
    [TestCase(Difficulty.Evil, 20, 27)]
    public void Generate_ClueCountWithinRange(Difficulty difficulty, int min, int max)
    {
        var generator = new SudokuGenerator(new SimpleSolver(), new Random(42));

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
        var generator = new SudokuGenerator(new SimpleSolver(), new Random(42));

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
        var generator = new SudokuGenerator(new SimpleSolver(), new Random(42));

        var puzzle = generator.Generate(difficulty);

        new SimpleSolver().IsUnique(puzzle).Should().BeTrue();
    }

    [Test]
    public void Generate_WithSameSeed_ProducesSameBoard()
    {
        var generator1 = new SudokuGenerator(new SimpleSolver(), new Random(123));
        var generator2 = new SudokuGenerator(new SimpleSolver(), new Random(123));

        var puzzle1 = generator1.Generate(Difficulty.Medium);
        var puzzle2 = generator2.Generate(Difficulty.Medium);

        puzzle1.ShouldBeEqualTo(puzzle2);
    }
}
