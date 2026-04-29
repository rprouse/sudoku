using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sudoku.Core.Game;

public class GamePersistenceService
{
    private static readonly JsonSerializerOptions s_options = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new CellStateArrayConverter(), new UndoStackConverter() }
    };

    private readonly string _saveDirectory;

    public GamePersistenceService(string saveDirectory)
    {
        _saveDirectory = saveDirectory;
    }

    private string SavePath => Path.Combine(_saveDirectory, "game_state.json");

    public async Task SaveAsync(GameState state)
    {
        if (state.IsComplete())
            return;

        var json = JsonSerializer.Serialize(state, s_options);
        await File.WriteAllTextAsync(SavePath, json);
    }

    public async Task<GameState?> LoadAsync()
    {
        if (!File.Exists(SavePath))
            return null;

        var json = await File.ReadAllTextAsync(SavePath);
        return JsonSerializer.Deserialize<GameState>(json, s_options);
    }

    public void Delete()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }

    public bool HasSavedGame() => File.Exists(SavePath);
}

public class CellStateArrayConverter : JsonConverter<CellState[,]>
{
    public override CellState[,] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var flat = JsonSerializer.Deserialize<CellState[]>(ref reader, options) ?? Array.Empty<CellState>();
        var cells = new CellState[9, 9];
        for (var i = 0; i < flat.Length && i < 81; i++)
            cells[i / 9, i % 9] = flat[i];
        return cells;
    }

    public override void Write(Utf8JsonWriter writer, CellState[,] value, JsonSerializerOptions options)
    {
        var flat = new CellState[81];
        for (var r = 0; r < 9; r++)
            for (var c = 0; c < 9; c++)
                flat[r * 9 + c] = value[r, c];
        JsonSerializer.Serialize(writer, flat, options);
    }
}

public class UndoStackConverter : JsonConverter<Stack<UndoAction>>
{
    public override Stack<UndoAction> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var list = JsonSerializer.Deserialize<List<UndoAction>>(ref reader, options) ?? new List<UndoAction>();
        list.Reverse();
        return new Stack<UndoAction>(list);
    }

    public override void Write(Utf8JsonWriter writer, Stack<UndoAction> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.ToArray(), options);
    }
}
