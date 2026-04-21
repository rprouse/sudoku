namespace Sudoku.Core.Solvers;

public class BitmaskSolver : ISolver
{
    public int Iterations { get; private set; }

    public SudokuBoard Solve(SudokuBoard sudoku)
    {
        Iterations = 0;
        var state = SolverState.FromBoard(sudoku);
        if (!Search(ref state, out var solved))
        {
            throw new InvalidOperationException("No solution found.");
        }
        return solved.ToBoard();
    }

    public bool IsUnique(SudokuBoard sudoku)
    {
        Iterations = 0;
        var state = SolverState.FromBoard(sudoku);
        var found = 0;
        CountSolutions(ref state, cap: 2, ref found);
        if (found == 0) throw new InvalidOperationException("No solution found.");
        return found == 1;
    }

    // Returns true on the first solution found; 'solved' holds a snapshot.
    private bool Search(ref SolverState state, out SolverState solved)
    {
        if (state.EmptyCount == 0)
        {
            solved = CloneSolvedState(state);
            return true;
        }

        if (!PickMrvCell(in state, out var bestR, out var bestC, out var mask))
        {
            solved = default;
            return false; // a cell has zero candidates
        }

        while (mask != 0)
        {
            var digit = SolverState.LowestBitIndex(mask) + 1;
            state.Place(bestR, bestC, digit);
            Iterations++;
            if (Search(ref state, out solved))
            {
                state.Remove(bestR, bestC, digit);
                return true;
            }
            state.Remove(bestR, bestC, digit);
            mask &= mask - 1;
        }

        solved = default;
        return false;
    }

    private void CountSolutions(ref SolverState state, int cap, ref int foundCount)
    {
        if (foundCount >= cap) return;

        if (state.EmptyCount == 0)
        {
            foundCount++;
            return;
        }

        if (!PickMrvCell(in state, out var bestR, out var bestC, out var mask))
        {
            return;
        }

        while (mask != 0 && foundCount < cap)
        {
            var digit = SolverState.LowestBitIndex(mask) + 1;
            state.Place(bestR, bestC, digit);
            Iterations++;
            CountSolutions(ref state, cap, ref foundCount);
            state.Remove(bestR, bestC, digit);
            mask &= mask - 1;
        }
    }

    private static bool PickMrvCell(in SolverState state, out int bestR, out int bestC, out int bestMask)
    {
        bestR = -1;
        bestC = -1;
        bestMask = 0;
        var bestCount = int.MaxValue;
        for (var r = 0; r < 9; r++)
        {
            for (var c = 0; c < 9; c++)
            {
                if (state.Grid[r, c] != 0) continue;
                var m = state.UnitCandidates(r, c);
                var cnt = SolverState.PopCount(m);
                if (cnt == 0) { bestMask = 0; bestR = r; bestC = c; return false; }
                if (cnt < bestCount)
                {
                    bestCount = cnt;
                    bestR = r;
                    bestC = c;
                    bestMask = m;
                    if (cnt == 1) return true;
                }
            }
        }
        return bestR >= 0;
    }

    // Called only when EmptyCount == 0: every cell's CellCandidates entry is
    // already zero, so we allocate a fresh zeroed array instead of cloning.
    private static SolverState CloneSolvedState(in SolverState source) =>
        new()
        {
            Grid = (int[,])source.Grid.Clone(),
            RowMask = (int[])source.RowMask.Clone(),
            ColMask = (int[])source.ColMask.Clone(),
            BoxMask = (int[])source.BoxMask.Clone(),
            CellCandidates = new int[9, 9],
            EmptyCount = 0
        };
}
