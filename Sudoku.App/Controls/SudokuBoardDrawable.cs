using Sudoku.Core.Game;

namespace Sudoku.App.Controls;

public class SudokuBoardDrawable : IDrawable
{
    public GameState? State { get; set; }
    public GameSettings Settings { get; set; } = new();
    public int SelectedRow { get; set; } = -1;
    public int SelectedCol { get; set; } = -1;
    public bool IsDarkTheme { get; set; } = true;

    // Dark theme colors — warm earth tones
    private static readonly Color s_darkBackground = Color.FromArgb("#1e1b18");
    private static readonly Color s_darkSelectedCell = Color.FromArgb("#3a3228");
    private static readonly Color s_darkRelatedCell = Color.FromArgb("#2c2422");
    private static readonly Color s_darkSameNumber = Color.FromArgb("#2d3328");
    private static readonly Color s_darkErrorCell = Color.FromArgb("#8b4040");
    private static readonly Color s_darkThinLine = Color.FromArgb("#4a4038");
    private static readonly Color s_darkThickLine = Color.FromArgb("#7a7068");
    private static readonly Color s_darkGivenText = Color.FromArgb("#e0d8cf");
    private static readonly Color s_darkPlayerText = Color.FromArgb("#9aab88");
    private static readonly Color s_darkCandidateText = Color.FromArgb("#7a7068");

    // Light theme colors — warm earth tones
    private static readonly Color s_lightBackground = Color.FromArgb("#f5f0eb");
    private static readonly Color s_lightSelectedCell = Color.FromArgb("#ddd2c4");
    private static readonly Color s_lightRelatedCell = Color.FromArgb("#f0ddda");
    private static readonly Color s_lightSameNumber = Color.FromArgb("#d8e0d0");
    private static readonly Color s_lightErrorCell = Color.FromArgb("#e8c4c0");
    private static readonly Color s_lightThinLine = Color.FromArgb("#c4b5a4");
    private static readonly Color s_lightThickLine = Color.FromArgb("#5a4e44");
    private static readonly Color s_lightGivenText = Color.FromArgb("#3d3632");
    private static readonly Color s_lightPlayerText = Color.FromArgb("#7d8c6e");
    private static readonly Color s_lightCandidateText = Color.FromArgb("#8a7e74");

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (State == null) return;

        var size = Math.Min(dirtyRect.Width, dirtyRect.Height);
        if (size <= 0) return;

        var cellSize = size / 9f;
        var offsetX = (dirtyRect.Width - size) / 2f;
        var offsetY = (dirtyRect.Height - size) / 2f;

        DrawCellBackgrounds(canvas, cellSize, offsetX, offsetY);
        DrawGridLines(canvas, size, cellSize, offsetX, offsetY);
        DrawNumbers(canvas, cellSize, offsetX, offsetY);
    }

    private void DrawCellBackgrounds(ICanvas canvas, float cellSize, float offsetX, float offsetY)
    {
        var bg = IsDarkTheme ? s_darkBackground : s_lightBackground;
        var selected = IsDarkTheme ? s_darkSelectedCell : s_lightSelectedCell;
        var related = IsDarkTheme ? s_darkRelatedCell : s_lightRelatedCell;
        var sameNum = IsDarkTheme ? s_darkSameNumber : s_lightSameNumber;
        var error = IsDarkTheme ? s_darkErrorCell : s_lightErrorCell;

        var selectedValue = (SelectedRow >= 0 && SelectedCol >= 0)
            ? State!.Cells[SelectedRow, SelectedCol].Value
            : 0;

        for (var r = 0; r < 9; r++)
        {
            for (var c = 0; c < 9; c++)
            {
                var cell = State!.Cells[r, c];
                var color = GetCellColor(r, c, cell, selectedValue, bg, selected, related, sameNum, error);
                canvas.FillColor = color;
                canvas.FillRectangle(offsetX + c * cellSize, offsetY + r * cellSize, cellSize, cellSize);
            }
        }
    }

    private Color GetCellColor(int row, int col, CellState cell, int selectedValue,
        Color bg, Color selected, Color related, Color sameNum, Color error)
    {
        // Priority: Error > Selected > Same Number > Related > Default
        if (cell.IsError && Settings.ShowErrors) return error;
        if (row == SelectedRow && col == SelectedCol) return selected;
        if (Settings.HighlightSameNumbers && selectedValue != 0 && cell.Value == selectedValue) return sameNum;
        if (Settings.HighlightRelatedCells && SelectedRow >= 0 && SelectedCol >= 0)
        {
            var sameRow = row == SelectedRow;
            var sameCol = col == SelectedCol;
            var sameBox = (row / 3 == SelectedRow / 3) && (col / 3 == SelectedCol / 3);
            if (sameRow || sameCol || sameBox) return related;
        }
        return bg;
    }

    private void DrawGridLines(ICanvas canvas, float size, float cellSize, float offsetX, float offsetY)
    {
        var thinColor = IsDarkTheme ? s_darkThinLine : s_lightThinLine;
        var thickColor = IsDarkTheme ? s_darkThickLine : s_lightThickLine;

        canvas.StrokeColor = thinColor;
        canvas.StrokeSize = 1;
        for (var i = 1; i < 9; i++)
        {
            if (i % 3 == 0) continue;
            canvas.DrawLine(offsetX + i * cellSize, offsetY, offsetX + i * cellSize, offsetY + size);
            canvas.DrawLine(offsetX, offsetY + i * cellSize, offsetX + size, offsetY + i * cellSize);
        }

        canvas.StrokeColor = thickColor;
        canvas.StrokeSize = 2;
        for (var i = 0; i <= 9; i += 3)
        {
            canvas.DrawLine(offsetX + i * cellSize, offsetY, offsetX + i * cellSize, offsetY + size);
            canvas.DrawLine(offsetX, offsetY + i * cellSize, offsetX + size, offsetY + i * cellSize);
        }
    }

    private void DrawNumbers(ICanvas canvas, float cellSize, float offsetX, float offsetY)
    {
        var givenColor = IsDarkTheme ? s_darkGivenText : s_lightGivenText;
        var playerColor = IsDarkTheme ? s_darkPlayerText : s_lightPlayerText;
        var candidateColor = IsDarkTheme ? s_darkCandidateText : s_lightCandidateText;

        for (var r = 0; r < 9; r++)
        {
            for (var c = 0; c < 9; c++)
            {
                var cell = State!.Cells[r, c];
                var x = offsetX + c * cellSize;
                var y = offsetY + r * cellSize;

                if (cell.HasValue)
                {
                    canvas.FontColor = cell.IsGiven ? givenColor : playerColor;
                    canvas.FontSize = cellSize * 0.55f;
                    canvas.DrawString(cell.Value.ToString(), x, y, cellSize, cellSize,
                        HorizontalAlignment.Center, VerticalAlignment.Center);
                }
                else
                {
                    canvas.FontColor = candidateColor;
                    canvas.FontSize = cellSize * 0.25f;
                    var miniSize = cellSize / 3f;
                    for (var n = 1; n <= 9; n++)
                    {
                        if (!cell.HasCandidate(n)) continue;
                        var miniRow = (n - 1) / 3;
                        var miniCol = (n - 1) % 3;
                        canvas.DrawString(n.ToString(),
                            x + miniCol * miniSize, y + miniRow * miniSize, miniSize, miniSize,
                            HorizontalAlignment.Center, VerticalAlignment.Center);
                    }
                }
            }
        }
    }

    public (int row, int col) HitTest(PointF point, float viewWidth, float viewHeight)
    {
        var size = Math.Min(viewWidth, viewHeight);
        var cellSize = size / 9f;
        var offsetX = (viewWidth - size) / 2f;
        var offsetY = (viewHeight - size) / 2f;

        var col = (int)((point.X - offsetX) / cellSize);
        var row = (int)((point.Y - offsetY) / cellSize);

        if (row < 0 || row > 8 || col < 0 || col > 8) return (-1, -1);
        return (row, col);
    }
}
