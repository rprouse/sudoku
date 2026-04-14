namespace Sudoku.Core.Solvers;

public class SimpleSolver : ISolver
{
    public int Iterations { get; private set; }

    public bool IsUnique(SudokuBoard sudoku)
    {
        Iterations = 0;
        var copy = new SudokuBoard(sudoku);
        var solutions = BruteForceSolve(copy).Take(2).ToList();
        if (solutions.Count == 1)
        {
            return true;
        }
        if (solutions.Count > 1)
        {
            return false;
        }
        throw new InvalidOperationException("No solution found.");
    }

    public SudokuBoard Solve(SudokuBoard sudoku) =>
        BruteForceSolve(sudoku).FirstOrDefault() ??
            throw new InvalidOperationException("No solution found.");

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
                if (sudoku[i, j] == 0)
                {
                    for (int k = 1; k <= 9; k++)
                    {
                        sudoku[i, j] = k;
                        if (sudoku.IsValid())
                        {
                            Iterations++;
                            foreach (SudokuBoard solution in BruteForceSolve(sudoku))
                            {
                                yield return new SudokuBoard(solution);
                            }
                        }
                        sudoku[i, j] = 0;
                    }
                    yield break;
                }
            }
        }
    }
}