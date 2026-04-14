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
        var (min, max) = GetClueRange(difficulty);
        var pairs = BuildSymmetricPairs();
        Shuffle(pairs);

        foreach (var (r1, c1, r2, c2) in pairs)
        {
            if (board.ClueCount <= max)
                break;

            var val1 = board[r1, c1];
            var val2 = (r1 == r2 && c1 == c2) ? val1 : board[r2, c2];

            board[r1, c1] = 0;
            if (r1 != r2 || c1 != c2)
                board[r2, c2] = 0;

            if (!_solver.IsUnique(board))
            {
                board[r1, c1] = val1;
                if (r1 != r2 || c1 != c2)
                    board[r2, c2] = val2;
            }
        }
    }

    private static (int r1, int c1, int r2, int c2)[] BuildSymmetricPairs()
    {
        var pairs = new List<(int r1, int c1, int r2, int c2)>();
        var visited = new bool[9, 9];

        for (var r = 0; r < 9; r++)
        {
            for (var c = 0; c < 9; c++)
            {
                if (visited[r, c])
                    continue;

                var mr = 8 - r;
                var mc = 8 - c;
                visited[r, c] = true;
                visited[mr, mc] = true;
                pairs.Add((r, c, mr, mc));
            }
        }

        return pairs.ToArray();
    }

    private static (int min, int max) GetClueRange(Difficulty difficulty) => difficulty switch
    {
        Difficulty.Easy => (40, 46),
        Difficulty.Medium => (33, 39),
        Difficulty.Hard => (28, 32),
        Difficulty.Expert => (24, 27),
        Difficulty.Evil => (20, 27),
        _ => throw new ArgumentOutOfRangeException(nameof(difficulty))
    };

    private void Shuffle<T>(T[] array)
    {
        for (var i = array.Length - 1; i > 0; i--)
        {
            var j = _random.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}
