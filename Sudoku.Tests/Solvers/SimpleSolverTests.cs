using Sudoku.Core.Solvers;

namespace Sudoku.Tests.Solvers;

public class SimpleSolverTests : SolverContractTests
{
    protected override ISolver CreateSolver() => new SimpleSolver();
}
