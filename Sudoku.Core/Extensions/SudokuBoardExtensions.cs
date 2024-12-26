namespace Sudoku.Core.Extensions;

public static class SudokuBoardExtensions
{
    public static bool IsFilled(this int[][] sudoku) =>
        sudoku.All(r => r.All(c => c > 0 && c <= 9));

    public static bool IsValid(this int[][] sudoku) =>
        IsValidRows(sudoku) && IsValidColumns(sudoku) && IsValidBoxes(sudoku);


    public static int[][] CloneArray(this int[][] sudoku) =>
        sudoku.Select(a => a.Select(i => i).ToArray()).ToArray();

    public static bool IsValidRows(this int[][] sudoku)
    {
        for (int i = 0; i < 9; i++)
        {
            int zeroes = sudoku[i].Count(c => c == 0);
            if (sudoku[i].Where(s => s > 0 && s <= 9).Distinct().Count() != 9 - zeroes)
            {
                return false;
            }
        }
        return true;
    }

    public static bool IsValidColumns(this int[][] sudoku)
    {
        for (int i = 0; i < 9; i++)
        {
            int zeroes = sudoku.Count(r => r[i] == 0);
            if (sudoku.Select(r => r[i]).Where(s => s > 0 && s <= 9).Distinct().Count() != 9 - zeroes)
            {
                return false;
            }
        }
        return true;
    }

    public static bool IsValidBoxes(this int[][] sudoku)
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int zeroes = sudoku.GetBox(i, j).Count(c => c == 0);
                if (sudoku.GetBox(i, j).Where(s => s > 0 && s <= 9).Distinct().Count() != 9 - zeroes)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public static IEnumerable<int> GetBox(this int[][] sudoku, int row, int column)
    {
        int boxRow = row * 3;
        int boxColumn = column * 3;
        for (int i = boxRow; i < boxRow + 3; i++)
        {
            for (int j = boxColumn; j < boxColumn + 3; j++)
            {
                yield return sudoku[i][j];
            }
        }
    }
}
