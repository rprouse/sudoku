namespace Sudoku.Core;

public class SudokuBoard
{
    private int[,] Sudoku { get; }

    public int this[int row, int column]
    {
        get
        {
            return Sudoku[row, column];
        }
        set
        {
            Sudoku[row, column] = value;
        }
    }

    public SudokuBoard()
    {
        Sudoku = new int[9, 9];
    }

    public SudokuBoard(SudokuBoard copy)
    {
        Sudoku = CloneArray(copy.Sudoku);
    }

    public SudokuBoard(int[][] sudoku)
    {
        Sudoku = CloneArray(sudoku);
    }

    public bool IsFilled() =>
        Sudoku.Cast<int>().All(c => c > 0 && c <= 9);

    public bool IsValid() =>
        IsValidRows() && IsValidColumns() && IsValidBoxes();

    private static int[,] CloneArray(int[][] sudoku)
    {
        if (sudoku.Length != 9 || sudoku.Any(r => r.Length != 9))
        {
            throw new ArgumentException("Sudoku board must be 9x9.");
        }
        var clone = new int[9, 9];
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                clone[i, j] = sudoku[i][j];
            }
        }
        return clone;
    }

    private static int[,] CloneArray(int[,] sudoku)
    {
        var clone = new int[9, 9];
        Buffer.BlockCopy(sudoku, 0, clone, 0, sudoku.Length * sizeof(int));
        return clone;
    }

    private IEnumerable<int> Row(int row) =>
        Enumerable.Range(0, 9).Select(col => Sudoku[row, col]);

    private IEnumerable<int> Column(int col) =>
        Enumerable.Range(0, 9).Select(row => Sudoku[row, col]);

    public bool IsValidRows()
    {
        for (int r = 0; r < 9; r++)
        {
            int zeroes = Row(r).Count(cell => cell == 0);
            if (Row(r).Where(cell => cell > 0 && cell <= 9).Distinct().Count() != 9 - zeroes)
            {
                return false;
            }
        }
        return true;
    }

    public bool IsValidColumns()
    {
        for (int c = 0; c < 9; c++)
        {
            int zeroes = Column(c).Count(cell => cell == 0);
            if (Column(c).Where(cell => cell > 0 && cell <= 9).Distinct().Count() != 9 - zeroes)
            {
                return false;
            }
        }
        return true;
    }

    public bool IsValidBoxes()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int zeroes = GetBox(i, j).Count(c => c == 0);
                if (GetBox(i, j).Where(s => s > 0 && s <= 9).Distinct().Count() != 9 - zeroes)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public IEnumerable<int> GetBox(int row, int column)
    {
        int boxRow = row * 3;
        int boxColumn = column * 3;
        for (int r = boxRow; r < boxRow + 3; r++)
        {
            for (int c = boxColumn; c < boxColumn + 3; c++)
            {
                yield return Sudoku[r, c];
            }
        }
    }
}
