using Sudoku.Core.Solvers;

namespace Sudoku.Core.Generators;

public class SudokuGenerator : IGenerator
{
    private readonly ISolver _solver;
    private readonly Random _random;

    public SudokuGenerator(ISolver solver, Random? random = null)
    {
        _solver = solver;
        _random = random ?? Random.Shared;
    }

    public SudokuBoard Generate(Difficulty difficulty)
    {
        var board = GenerateFullBoard();
        RemoveClues(board, difficulty);
        return board;
    }

    private SudokuBoard GenerateFullBoard()
    {
        var board = new SudokuBoard();
        FillDiagonalBoxes(board);
        return _solver.Solve(board);
    }

    private void FillDiagonalBoxes(SudokuBoard board)
    {
        // The three diagonal 3x3 boxes (top-left, center, bottom-right)
        // don't constrain each other, so random fill is always valid.
        for (var box = 0; box < 3; box++)
        {
            var digits = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Shuffle(digits);
            var idx = 0;
            var startRow = box * 3;
            var startCol = box * 3;
            for (var r = startRow; r < startRow + 3; r++)
            {
                for (var c = startCol; c < startCol + 3; c++)
                {
                    board[r, c] = digits[idx++];
                }
            }
        }
    }

    private void RemoveClues(SudokuBoard board, Difficulty difficulty)
    {
        // Stub — implemented in Task 5
    }

    private void Shuffle<T>(T[] array)
    {
        for (var i = array.Length - 1; i > 0; i--)
        {
            var j = _random.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}
