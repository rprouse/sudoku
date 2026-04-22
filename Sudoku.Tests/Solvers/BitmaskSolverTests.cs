using Sudoku.Core.Solvers;

namespace Sudoku.Tests.Solvers;

public class BitmaskSolverTests : SolverContractTests
{
    protected override ISolver CreateSolver() => new BitmaskSolver();
}
