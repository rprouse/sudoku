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
    public SudokuJsonBoard[] Easy { get; set; } = Array.Empty<SudokuJsonBoard>();
    public SudokuJsonBoard[] Medium { get; set; } = Array.Empty<SudokuJsonBoard>();
    public SudokuJsonBoard[] Hard { get; set; } = Array.Empty<SudokuJsonBoard>();
    public SudokuJsonBoard[] Expert { get; set; } = Array.Empty<SudokuJsonBoard>();
    public SudokuJsonBoard[] Evil { get; set; } = Array.Empty<SudokuJsonBoard>();
}

public class SudokuJsonBoard
{
    public int Id { get; set; }
    public int Iterations { get; set; }
    public int[][] Sudoku { get; set; } = new int[9][];
    public int[][] Solution { get; set; } = new int[9][];

    public SudokuBoard GetSudokuBoard() => new(Sudoku);
    public SudokuBoard GetSolutionBoard() => new(Solution);

    override public string ToString() => $"Sudoku: {Id}, Iterations: {Iterations}";
}