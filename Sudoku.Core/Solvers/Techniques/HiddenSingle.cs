namespace Sudoku.Core.Solvers.Techniques;

internal sealed class HiddenSingle : ITechnique
{
    public DifficultyTechnique Level => DifficultyTechnique.HiddenSingle;
    public string Name => "Hidden Single";

    public bool TryApply(ref SolverState state, out SolveStep step)
    {
        // For each unit (row, column, box), for each digit not yet placed,
        // check whether exactly one empty cell in the unit can hold it.
        for (var unit = 0; unit < 9; unit++)
        {
            if (TryHiddenSingleInRow(ref state, unit, out step)) return true;
            if (TryHiddenSingleInCol(ref state, unit, out step)) return true;
            if (TryHiddenSingleInBox(ref state, unit, out step)) return true;
        }
        step = default!;
        return false;
    }

    private bool TryHiddenSingleInRow(ref SolverState state, int r, out SolveStep step)
    {
        for (var d = 1; d <= 9; d++)
        {
            var bit = 1 << (d - 1);
            if ((state.RowMask[r] & bit) != 0) continue; // digit already placed in row
            var candidateCol = -1;
            var count = 0;
            for (var c = 0; c < 9; c++)
            {
                if (state.Grid[r, c] != 0) continue;
                if ((state.UnitCandidates(r, c) & bit) == 0) continue;
                candidateCol = c;
                count++;
                if (count > 1) break;
            }
            if (count == 1)
            {
                state.Place(r, candidateCol, d);
                step = new SolveStep(Name, Level, $"Hidden single in row {r}: ({r},{candidateCol})={d}");
                return true;
            }
        }
        step = default!;
        return false;
    }

    private bool TryHiddenSingleInCol(ref SolverState state, int c, out SolveStep step)
    {
        for (var d = 1; d <= 9; d++)
        {
            var bit = 1 << (d - 1);
            if ((state.ColMask[c] & bit) != 0) continue;
            var candidateRow = -1;
            var count = 0;
            for (var r = 0; r < 9; r++)
            {
                if (state.Grid[r, c] != 0) continue;
                if ((state.UnitCandidates(r, c) & bit) == 0) continue;
                candidateRow = r;
                count++;
                if (count > 1) break;
            }
            if (count == 1)
            {
                state.Place(candidateRow, c, d);
                step = new SolveStep(Name, Level, $"Hidden single in col {c}: ({candidateRow},{c})={d}");
                return true;
            }
        }
        step = default!;
        return false;
    }

    private bool TryHiddenSingleInBox(ref SolverState state, int box, out SolveStep step)
    {
        var startR = (box / 3) * 3;
        var startC = (box % 3) * 3;
        for (var d = 1; d <= 9; d++)
        {
            var bit = 1 << (d - 1);
            if ((state.BoxMask[box] & bit) != 0) continue;
            var candidateR = -1;
            var candidateC = -1;
            var count = 0;
            for (var r = startR; r < startR + 3; r++)
            {
                for (var c = startC; c < startC + 3; c++)
                {
                    if (state.Grid[r, c] != 0) continue;
                    if ((state.UnitCandidates(r, c) & bit) == 0) continue;
                    candidateR = r;
                    candidateC = c;
                    count++;
                    if (count > 1) break;
                }
                if (count > 1) break;
            }
            if (count == 1)
            {
                state.Place(candidateR, candidateC, d);
                step = new SolveStep(Name, Level, $"Hidden single in box {box}: ({candidateR},{candidateC})={d}");
                return true;
            }
        }
        step = default!;
        return false;
    }
}
