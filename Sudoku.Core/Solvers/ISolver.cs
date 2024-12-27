namespace Sudoku.Core.Solvers;

public interface ISolver
{
    int Iterations { get; }

    bool IsUnique(SudokuBoard sudoku);

    SudokuBoard Solve(SudokuBoard sudoku);
}
