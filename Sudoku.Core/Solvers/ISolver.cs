namespace Sudoku.Core.Solvers;

public interface ISolver
{
    int Iterations { get; }
    SudokuBoard Solve(SudokuBoard sudoku);
}
