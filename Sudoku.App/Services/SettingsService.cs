using Sudoku.Core.Game;

namespace Sudoku.App.Services;

public class SettingsService
{
    private const string IsDarkThemeKey = "IsDarkTheme";
    private const string HighlightRelatedCellsKey = "HighlightRelatedCells";
    private const string HighlightSameNumbersKey = "HighlightSameNumbers";
    private const string ShowErrorsKey = "ShowErrors";
    private const string AutoRemoveCandidatesKey = "AutoRemoveCandidates";

    public GameSettings Load()
    {
        return new GameSettings
        {
            IsDarkTheme = Preferences.Default.Get(IsDarkThemeKey, true),
            HighlightRelatedCells = Preferences.Default.Get(HighlightRelatedCellsKey, true),
            HighlightSameNumbers = Preferences.Default.Get(HighlightSameNumbersKey, true),
            ShowErrors = Preferences.Default.Get(ShowErrorsKey, true),
            AutoRemoveCandidates = Preferences.Default.Get(AutoRemoveCandidatesKey, false)
        };
    }

    public void Save(GameSettings settings)
    {
        Preferences.Default.Set(IsDarkThemeKey, settings.IsDarkTheme);
        Preferences.Default.Set(HighlightRelatedCellsKey, settings.HighlightRelatedCells);
        Preferences.Default.Set(HighlightSameNumbersKey, settings.HighlightSameNumbers);
        Preferences.Default.Set(ShowErrorsKey, settings.ShowErrors);
        Preferences.Default.Set(AutoRemoveCandidatesKey, settings.AutoRemoveCandidates);
    }
}
