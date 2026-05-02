using System.Text.Json;

namespace Sudoku.Core.Game;

public static class KoanParser
{
    public static List<Koan> Parse(string json) =>
        JsonSerializer.Deserialize<List<Koan>>(json, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize koans.");

    public static List<Koan> FilterPlayablePool(IEnumerable<Koan> koans) =>
        // Sharp koans are excluded for now; can be re-enabled later via tone weighting.
        koans.Where(k => k.Tone != "sharp").ToList();

    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNameCaseInsensitive = true };
}
