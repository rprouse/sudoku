namespace Sudoku.Core;

public class SudokuBoard
{
    public int[][] Sudoku { get; private set; }

    public SudokuBoard()
    {
        Sudoku = new int[9][];
        for (int i = 0; i < 9; i++)
        {
            Sudoku[i] = new int[9];
        }
    }

    public SudokuBoard(int[][] sudoku)
    {
        if (sudoku.Length != 9 || sudoku.Any(r => r.Length != 9))
        {
            throw new ArgumentException("Sudoku board must be 9x9.");
        }
        Sudoku = CloneArray(sudoku);
    }

    public bool IsFilled() =>
        Sudoku.All(r => r.All(c => c > 0 && c <= 9));

    public bool IsValid() =>
        IsValidRows() && IsValidColumns() && IsValidBoxes();


    private static int[][] CloneArray(int[][] sudoku) =>
        sudoku.Select(a => a.Select(i => i).ToArray()).ToArray();

    public bool IsValidRows()
    {
        for (int i = 0; i < 9; i++)
        {
            int zeroes = Sudoku[i].Count(c => c == 0);
            if (Sudoku[i].Where(s => s > 0 && s <= 9).Distinct().Count() != 9 - zeroes)
            {
                return false;
            }
        }
        return true;
    }

    public bool IsValidColumns()
    {
        for (int i = 0; i < 9; i++)
        {
            int zeroes = Sudoku.Count(r => r[i] == 0);
            if (Sudoku.Select(r => r[i]).Where(s => s > 0 && s <= 9).Distinct().Count() != 9 - zeroes)
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
        for (int i = boxRow; i < boxRow + 3; i++)
        {
            for (int j = boxColumn; j < boxColumn + 3; j++)
            {
                yield return Sudoku[i][j];
            }
        }
    }
}
