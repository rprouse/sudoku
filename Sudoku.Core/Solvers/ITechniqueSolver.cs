namespace Sudoku.Core.Solvers;

public interface ITechniqueSolver
{
    SolveTrace Solve(SudokuBoard board);
}
