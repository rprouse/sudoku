using BenchmarkDotNet.Attributes;

using Sudoku.Core;
using Sudoku.Core.Generators;
using Sudoku.Core.Solvers;

namespace Sudoku.Benchmarks;

public class SudokuBenchmarks
{
    private readonly ISolver _solver = new SimpleSolver();
    private readonly IGenerator _generator = new SudokuGenerator(new SimpleSolver());
    private readonly SudokuJsonFile _sudokus = Persistence.LoadFromJson("sudokus.json");

    [Benchmark]
    [ArgumentsSource(nameof(SudokuBoards))]
    public void SimpleSolveSudoku(SudokuJsonBoard board)
    {
        var _ = _solver.Solve(board.GetSudokuBoard());
    }

    [Benchmark]
    [Arguments(Difficulty.Easy)]
    [Arguments(Difficulty.Medium)]
    [Arguments(Difficulty.Hard)]
    [Arguments(Difficulty.Expert)]
    [Arguments(Difficulty.Evil)]
    public void GenerateSudoku(Difficulty difficulty)
    {
        _generator.Generate(difficulty);
    }

    public IEnumerable<SudokuJsonBoard> SudokuBoards() =>
        _sudokus.Easy
            .Union(_sudokus.Medium)
            .Union(_sudokus.Hard)
            .Union(_sudokus.Expert)
            .Union(_sudokus.Evil);
}
