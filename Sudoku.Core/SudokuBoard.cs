namespace Sudoku.Core;

public class SudokuBoard
{
    private readonly int[,] _board;

    // 9 bits for each cell, 1-9
    private readonly int[,] _possibilities = new int[9, 9];

    public int this[int row, int column]
    {
        get
        {
            return _board[row, column];
        }
        set
        {
            _board[row, column] = value;
        }
    }

    public IEnumerable<int> GetPossibilities(int row, int column)
    {
        for (int i = 1; i <= 9; i++)
        {
            if ((_possibilities[row, column] & (1 << i)) > 0)
            {
                yield return i;
            }
        }
    }

    public SudokuBoard()
    {
        _board = new int[9, 9];
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                // Set all possibilities to 1-9
                _possibilities[r, c] = 0b00000001_11111111;
            }
        }
    }

    public SudokuBoard(SudokuBoard copy)
    {
        _board = CloneArray(copy._board);
        CalculatePossibilites();
    }

    public SudokuBoard(int[][] sudoku)
    {
        _board = CloneArray(sudoku);
        CalculatePossibilites();
    }

    private void CalculatePossibilites()
    {
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                if (_board[r, c] > 0)
                {
                    _possibilities[r, c] = 0;
                    continue;
                }
                int row = GetRow(r).Where(cell => cell > 0).Aggregate(0, (acc, cell) => acc | (1 << cell));
                int col = GetColumn(c).Where(cell => cell > 0).Aggregate(0, (acc, cell) => acc | (1 << cell));
                int box = GetBox(r / 3, c / 3).Where(cell => cell > 0).Aggregate(0, (acc, cell) => acc | (1 << cell));
                _possibilities[r, c] = ~(row | col | box);
            }
        }
    }

    public bool IsFilled() =>
        _board.Cast<int>().All(c => c > 0 && c <= 9);

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

    public bool IsValidRows()
    {
        for (int r = 0; r < 9; r++)
        {
            int zeroes = GetRow(r).Count(cell => cell == 0);
            if (GetRow(r).Where(cell => cell > 0 && cell <= 9).Distinct().Count() != 9 - zeroes)
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
            int zeroes = GetColumn(c).Count(cell => cell == 0);
            if (GetColumn(c).Where(cell => cell > 0 && cell <= 9).Distinct().Count() != 9 - zeroes)
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

    private IEnumerable<int> GetRow(int row) =>
        Enumerable.Range(0, 9).Select(col => _board[row, col]);

    private IEnumerable<int> GetColumn(int col) =>
        Enumerable.Range(0, 9).Select(row => _board[row, col]);

    public IEnumerable<int> GetBox(int row, int column)
    {
        int boxRow = row * 3;
        int boxColumn = column * 3;
        for (int r = boxRow; r < boxRow + 3; r++)
        {
            for (int c = boxColumn; c < boxColumn + 3; c++)
            {
                yield return _board[r, c];
            }
        }
    }
}
