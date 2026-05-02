using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Sudoku.App.Services;

namespace Sudoku.App.ViewModels;

public partial class KoanViewModel : ObservableObject
{
    private readonly KoanService _koanService;

    [ObservableProperty]
    public partial string KoanText { get; set; } = "";

    [ObservableProperty]
    public partial double KoanFontSize { get; set; } = 26;

    public KoanViewModel(KoanService koanService) => _koanService = koanService;

    public async Task LoadAsync()
    {
        if (!string.IsNullOrEmpty(KoanText)) return;  // load once per instance

        try
        {
            var koan = await _koanService.GetRandomAsync();
            var lineCount = koan.Text.Split('|').Length;
            KoanText = koan.Text.Replace("|", Environment.NewLine);
            KoanFontSize = lineCount switch
            {
                1 => 32,
                2 => 26,
                3 => 22,
                _ => 20
            };
        }
        catch (Exception)
        {
            // Asset missing or corrupt — bail straight to the puzzle.
            // The koan is decoration; failing to render one must not block play.
            await Shell.Current.GoToAsync($"../{nameof(Views.GamePage)}");
        }
    }

    [RelayCommand]
    private static async Task Begin() =>
        await Shell.Current.GoToAsync($"../{nameof(Views.GamePage)}");
}
