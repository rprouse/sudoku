using Sudoku.Core.Generators;
using Sudoku.Core.Solvers;

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
}
