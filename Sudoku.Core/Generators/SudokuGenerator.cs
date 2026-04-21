using Sudoku.Core.Solvers;

namespace Sudoku.Core.Generators;

public class SudokuGenerator : IGenerator
{
    private const int MaxAttempts = 10;

    private readonly ISolver _solver;
    private readonly DifficultyGrader _grader;
    private readonly Random _random;

    public SudokuGenerator(ISolver solver, DifficultyGrader grader, Random? random = null)
    {
        _solver = solver;
        _grader = grader;
        _random = random ?? Random.Shared;
    }

    public SudokuBoard Generate(Difficulty difficulty)
    {
        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            var puzzle = TryGenerate(difficulty);
            if (puzzle is not null) return puzzle;
        }
        throw new InvalidOperationException("Generator exceeded max attempts.");
    }

    private SudokuBoard? TryGenerate(Difficulty target)
    {
        var full = BuildFullSolution();
        var working = new SudokuBoard(full);

        var pairs = BuildSymmetricPairs();
        Shuffle(pairs);

        foreach (var (r1, c1, r2, c2) in pairs)
        {
            TryRemovePair(working, r1, c1, r2, c2, target);
        }

        var grade = _grader.Grade(working);
        var (minClues, maxClues) = GetClueRange(target);
        if (grade == target && working.ClueCount >= minClues && working.ClueCount <= maxClues)
        {
            return working;
        }
        return null;
    }

    private SudokuBoard BuildFullSolution()
    {
        var board = new SudokuBoard();
        var startR = _random.Next(9);
        var startC = _random.Next(9);
        board[startR, startC] = _random.Next(1, 10);
        return _solver.Solve(board);
    }

    private void TryRemovePair(SudokuBoard board, int r1, int c1, int r2, int c2, Difficulty target)
    {
        var v1 = board[r1, c1];
        var v2 = (r1 == r2 && c1 == c2) ? v1 : board[r2, c2];

        // Don't remove if we're already at the minimum clue count for this tier.
        var (minClues, _) = GetClueRange(target);
        var cellsRemoved = (r1 == r2 && c1 == c2) ? 1 : 2;
        if (board.ClueCount - cellsRemoved < minClues)
            return;

        board[r1, c1] = 0;
        if (r1 != r2 || c1 != c2) board[r2, c2] = 0;

        if (!_solver.IsUnique(board))
        {
            board[r1, c1] = v1;
            if (r1 != r2 || c1 != c2) board[r2, c2] = v2;
            return;
        }

        var grade = _grader.Grade(board);
        if (grade is null || grade > target)
        {
            board[r1, c1] = v1;
            if (r1 != r2 || c1 != c2) board[r2, c2] = v2;
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
                if (visited[r, c]) continue;

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
        Difficulty.Easy => (40, 50),
        Difficulty.Medium => (32, 40),
        Difficulty.Hard => (27, 34),
        Difficulty.Expert => (24, 29),
        Difficulty.Evil => (22, 27),
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
