using System.Text.Json;

namespace Sudoku.Core;

public static class Persistence
{
    public static SudokuJsonFile LoadFromJson(string path)
    {
        string json = File.ReadAllText(path);

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        return JsonSerializer.Deserialize<SudokuJsonFile>(json, options)
            ?? new SudokuJsonFile();
    }
}

public class SudokuJsonFile
{
    public SudokuBoard[] Easy { get; set; } = Array.Empty<SudokuBoard>();
    public SudokuBoard[] Medium { get; set; } = Array.Empty<SudokuBoard>();
    public SudokuBoard[] Hard { get; set; } = Array.Empty<SudokuBoard>();
    public SudokuBoard[] Expert { get; set; } = Array.Empty<SudokuBoard>();
    public SudokuBoard[] Evil { get; set; } = Array.Empty<SudokuBoard>();
}

public class SudokuBoard
{
    public int Iterations { get; set; }
    public int[][] Sudoku { get; set; } = new int[9][];
    public int[][] Solution { get; set; } = new int[9][];
    public int Id { get; set; }
}