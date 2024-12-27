using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using Sudoku.Core;
using Sudoku.Core.Solvers;

var _ = BenchmarkRunner.Run(typeof(Program).Assembly);

public class SudokuBenchmarks
{
    private readonly ISolver _solver = new SimpleSolver();
    private readonly SudokuJsonFile _sudokus = Persistence.LoadFromJson("sudokus.json");

    [Benchmark]
    [ArgumentsSource(nameof(SudokuBoards))]
    public void SimpleSolveSudoku(SudokuJsonBoard board)
    {
        var _ = _solver.Solve(board.GetSudokuBoard());
    }

    public IEnumerable<SudokuJsonBoard> SudokuBoards() =>
        _sudokus.Easy
            .Union(_sudokus.Medium)
            .Union(_sudokus.Hard)
            .Union(_sudokus.Expert)
            .Union(_sudokus.Evil);
}