using Sudoku.Core.Game;

namespace Sudoku.App.Services;

public class KoanService
{
    private List<Koan>? _koans;
    private readonly Random _random = new();

    public async Task<Koan> GetRandomAsync()
    {
        if (_koans == null) await LoadAsync();
        return _koans![_random.Next(_koans.Count)];
    }

    private async Task LoadAsync()
    {
        await using var stream = await FileSystem.OpenAppPackageFileAsync("koans.json");
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();
        _koans = KoanParser.FilterPlayablePool(KoanParser.Parse(json));
    }
}
