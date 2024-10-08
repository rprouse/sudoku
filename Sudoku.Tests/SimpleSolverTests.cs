using Sudoku.Core;
using Sudoku.Tests.Extensions;

namespace Sudoku.Tests;

public class SimpleSolverTests
{
    [Test]
    public void CanSolveEasySudoku()
    {
        var sudokus = Persistence.LoadFromJson("sudokus.json");
        var easySudoku = sudokus.Easy[0];
        var solution = SimpleSolver.Solve(easySudoku.Sudoku);

        solution.ShouldBeEqualTo(easySudoku.Solution);
    }
}
