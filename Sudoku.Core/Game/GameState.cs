namespace Sudoku.Core.Game;

public class GameState
{
    public CellState[,] Cells { get; set; } = new CellState[9, 9];
    public int[][] Puzzle { get; set; } = Array.Empty<int[]>();
    public int[][] Solution { get; set; } = Array.Empty<int[]>();
    public Difficulty Difficulty { get; set; }
    public int ElapsedSeconds { get; set; }
    public int ErrorCount { get; set; }
    public Stack<UndoAction> UndoStack { get; set; } = new();

    public GameState()
    {
        for (var r = 0; r < 9; r++)
        {
            for (var c = 0; c < 9; c++)
            {
                Cells[r, c] = new CellState();
            }
        }
    }

    public GameState(SudokuBoard puzzle, SudokuBoard solution, Difficulty difficulty) : this()
    {
        Difficulty = difficulty;
        Puzzle = BoardToJagged(puzzle);
        Solution = BoardToJagged(solution);

        for (var r = 0; r < 9; r++)
        {
            for (var c = 0; c < 9; c++)
            {
                Cells[r, c].Value = puzzle[r, c];
                Cells[r, c].IsGiven = puzzle[r, c] != 0;
            }
        }
    }

    public void PlaceNumber(int row, int col, int number, GameSettings? settings = null)
    {
        var cell = Cells[row, col];
        if (cell.IsGiven) return;

        PushUndo(row, col);
        cell.Value = number;
        cell.ClearCandidates();

        if (settings?.ShowErrors == true)
        {
            cell.IsError = number != Solution[row][col];
            if (cell.IsError) ErrorCount++;
        }
        else
        {
            cell.IsError = false;
        }

        if (settings?.AutoRemoveCandidates == true)
        {
            RemoveCandidateFromRelatedCells(row, col, number);
        }
    }

    public void ClearCell(int row, int col)
    {
        var cell = Cells[row, col];
        if (cell.IsGiven) return;

        PushUndo(row, col);
        cell.Value = 0;
        cell.IsError = false;
    }

    public void ToggleCandidate(int row, int col, int number)
    {
        var cell = Cells[row, col];
        if (cell.IsGiven || cell.HasValue) return;

        PushUndo(row, col);
        cell.ToggleCandidate(number);

        if (!cell.HasCandidate(number))
            cell.ExcludedCandidates[number - 1] = true;
        else
            cell.ExcludedCandidates[number - 1] = false;
    }

    public void ClearCandidates(int row, int col)
    {
        var cell = Cells[row, col];
        if (cell.IsGiven || cell.HasValue) return;

        PushUndo(row, col);
        for (var i = 0; i < 9; i++)
        {
            if (cell.Candidates[i]) cell.ExcludedCandidates[i] = true;
        }
        cell.ClearCandidates();
    }

    public void Undo()
    {
        if (UndoStack.Count == 0) return;

        var action = UndoStack.Pop();
        var cell = Cells[action.Row, action.Col];
        cell.Value = action.PreviousValue;
        cell.IsError = false;
        Array.Copy(action.PreviousCandidates, cell.Candidates, 9);
        Array.Copy(action.PreviousExcludedCandidates, cell.ExcludedCandidates, 9);
    }

    public void ComputeAutoCandidates()
    {
        for (var r = 0; r < 9; r++)
        {
            for (var c = 0; c < 9; c++)
            {
                var cell = Cells[r, c];
                if (cell.IsGiven || cell.HasValue) continue;

                for (var n = 1; n <= 9; n++)
                {
                    var isValid = !IsNumberInRow(r, n)
                                  && !IsNumberInColumn(c, n)
                                  && !IsNumberInBox(r, c, n);
                    cell.Candidates[n - 1] = isValid && !cell.ExcludedCandidates[n - 1];
                }
            }
        }
    }

    public void ClearAutoCandidates()
    {
        for (var r = 0; r < 9; r++)
        {
            for (var c = 0; c < 9; c++)
            {
                var cell = Cells[r, c];
                if (!cell.IsGiven && !cell.HasValue)
                    cell.ClearCandidates();
            }
        }
    }

    public bool IsComplete()
    {
        for (var r = 0; r < 9; r++)
        {
            for (var c = 0; c < 9; c++)
            {
                if (Cells[r, c].Value != Solution[r][c]) return false;
            }
        }
        return true;
    }

    private void PushUndo(int row, int col)
    {
        var cell = Cells[row, col];
        UndoStack.Push(new UndoAction(
            row, col,
            cell.Value,
            cell.CloneCandidates(),
            cell.CloneExcludedCandidates()));
    }

    private void RemoveCandidateFromRelatedCells(int row, int col, int number)
    {
        for (var i = 0; i < 9; i++)
        {
            RemoveCandidateIfEmpty(row, i, number);
            RemoveCandidateIfEmpty(i, col, number);
        }

        var boxRow = (row / 3) * 3;
        var boxCol = (col / 3) * 3;
        for (var r = boxRow; r < boxRow + 3; r++)
        {
            for (var c = boxCol; c < boxCol + 3; c++)
            {
                RemoveCandidateIfEmpty(r, c, number);
            }
        }
    }

    private void RemoveCandidateIfEmpty(int row, int col, int number)
    {
        var cell = Cells[row, col];
        if (!cell.IsGiven && !cell.HasValue)
        {
            cell.Candidates[number - 1] = false;
            cell.ExcludedCandidates[number - 1] = true;
        }
    }

    private bool IsNumberInRow(int row, int number)
    {
        for (var c = 0; c < 9; c++)
        {
            if (Cells[row, c].Value == number) return true;
        }
        return false;
    }

    private bool IsNumberInColumn(int col, int number)
    {
        for (var r = 0; r < 9; r++)
        {
            if (Cells[r, col].Value == number) return true;
        }
        return false;
    }

    private bool IsNumberInBox(int row, int col, int number)
    {
        var boxRow = (row / 3) * 3;
        var boxCol = (col / 3) * 3;
        for (var r = boxRow; r < boxRow + 3; r++)
        {
            for (var c = boxCol; c < boxCol + 3; c++)
            {
                if (Cells[r, c].Value == number) return true;
            }
        }
        return false;
    }

    private static int[][] BoardToJagged(SudokuBoard board)
    {
        var jagged = new int[9][];
        for (var r = 0; r < 9; r++)
        {
            jagged[r] = new int[9];
            for (var c = 0; c < 9; c++)
            {
                jagged[r][c] = board[r, c];
            }
        }
        return jagged;
    }
}
