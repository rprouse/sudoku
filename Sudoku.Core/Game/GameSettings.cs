namespace Sudoku.Core.Game;

public class GameSettings
{
    public bool IsDarkTheme { get; set; } = true;
    public bool HighlightRelatedCells { get; set; } = true;
    public bool HighlightSameNumbers { get; set; } = true;
    public bool ShowErrors { get; set; } = true;
    public bool AutoRemoveCandidates { get; set; }
}
