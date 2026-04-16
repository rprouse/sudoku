using CommunityToolkit.Mvvm.ComponentModel;

using Sudoku.App.Services;
using Sudoku.Core.Game;

namespace Sudoku.App.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settingsService;
    private GameSettings _settings = new();

    [ObservableProperty]
    public partial bool IsDarkTheme { get; set; } = true;

    [ObservableProperty]
    public partial bool HighlightRelatedCells { get; set; } = true;

    [ObservableProperty]
    public partial bool HighlightSameNumbers { get; set; } = true;

    [ObservableProperty]
    public partial bool ShowErrors { get; set; } = true;

    public SettingsViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;
        LoadSettings();
    }

    private void LoadSettings()
    {
        _settings = _settingsService.Load();
        IsDarkTheme = _settings.IsDarkTheme;
        HighlightRelatedCells = _settings.HighlightRelatedCells;
        HighlightSameNumbers = _settings.HighlightSameNumbers;
        ShowErrors = _settings.ShowErrors;
    }

    partial void OnIsDarkThemeChanged(bool value)
    {
        _settings.IsDarkTheme = value;
        SaveAndApplyTheme();
    }

    partial void OnHighlightRelatedCellsChanged(bool value) { _settings.HighlightRelatedCells = value; Save(); }
    partial void OnHighlightSameNumbersChanged(bool value) { _settings.HighlightSameNumbers = value; Save(); }
    partial void OnShowErrorsChanged(bool value) { _settings.ShowErrors = value; Save(); }

    private void Save() => _settingsService.Save(_settings);

    private void SaveAndApplyTheme()
    {
        Save();
        if (Application.Current != null)
            Application.Current.UserAppTheme = _settings.IsDarkTheme ? AppTheme.Dark : AppTheme.Light;
    }
}
