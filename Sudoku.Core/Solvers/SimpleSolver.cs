namespace Sudoku.Core.Solvers;

public class SimpleSolver : ISolver
{
    public int Iterations { get; private set; }

    public SudokuBoard Solve(SudokuBoard sudoku)
    {
        return BruteForceSolve(sudoku).FirstOrDefault() ??
            throw new InvalidOperationException("No solution found.");
    }

    private IEnumerable<SudokuBoard> BruteForceSolve(SudokuBoard sudoku)
    {
        if (sudoku.IsFilled())
        {
            yield return sudoku;
            yield break;
        }
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (sudoku.Sudoku[i][j] == 0)
                {
                    for (int k = 1; k <= 9; k++)
                    {
                        sudoku.Sudoku[i][j] = k;
                        if (sudoku.IsValid())
                        {
                            foreach (SudokuBoard solution in BruteForceSolve(sudoku))
                            {
                                Iterations++;
                                yield return solution;
                            }
                        }
                        sudoku.Sudoku[i][j] = 0;
                    }
                    yield break;
                }
            }
        }
    }
}