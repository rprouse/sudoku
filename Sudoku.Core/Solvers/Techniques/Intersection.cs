namespace Sudoku.Core.Solvers.Techniques;

internal sealed class Intersection : ITechnique
{
    private readonly bool _pointing;
    public DifficultyTechnique Level { get; }
    public string Name { get; }

    public Intersection(bool pointing)
    {
        _pointing = pointing;
        Level = pointing ? DifficultyTechnique.PointingPair : DifficultyTechnique.BoxLineReduction;
        Name = pointing ? "Pointing Pair" : "Box/Line Reduction";
    }

    public bool TryApply(ref SolverState state, out SolveStep step)
    {
        if (_pointing)
        {
            return TryPointing(ref state, out step);
        }
        return TryBoxLineReduction(ref state, out step);
    }

    private bool TryPointing(ref SolverState state, out SolveStep step)
    {
        for (var box = 0; box < 9; box++)
        {
            for (var d = 1; d <= 9; d++)
            {
                var bit = 1 << (d - 1);
                if ((state.BoxMask[box] & bit) != 0) continue;

                var startR = (box / 3) * 3;
                var startC = (box % 3) * 3;
                var rows = new HashSet<int>();
                var cols = new HashSet<int>();
                for (var r = startR; r < startR + 3; r++)
                {
                    for (var c = startC; c < startC + 3; c++)
                    {
                        if (state.Grid[r, c] != 0) continue;
                        if ((state.UnitCandidates(r, c) & bit) == 0) continue;
                        rows.Add(r);
                        cols.Add(c);
                    }
                }

                if (rows.Count == 1 && cols.Count >= 2)
                {
                    var targetRow = rows.First();
                    if (TryEliminateInLine(ref state, d, targetRow, isRow: true, excludeBox: box, out step))
                        return true;
                }
                if (cols.Count == 1 && rows.Count >= 2)
                {
                    var targetCol = cols.First();
                    if (TryEliminateInLine(ref state, d, targetCol, isRow: false, excludeBox: box, out step))
                        return true;
                }
            }
        }
        step = default!;
        return false;
    }

    private bool TryBoxLineReduction(ref SolverState state, out SolveStep step)
    {
        // For each row/col and each unplaced digit, if all empty cells in the
        // line that can hold the digit share a single box, eliminate the digit
        // from the rest of that box.
        for (var line = 0; line < 9; line++)
        {
            for (var d = 1; d <= 9; d++)
            {
                var bit = 1 << (d - 1);

                if ((state.RowMask[line] & bit) == 0)
                {
                    if (TryLineToBox(ref state, d, bit, line, isRow: true, out step)) return true;
                }
                if ((state.ColMask[line] & bit) == 0)
                {
                    if (TryLineToBox(ref state, d, bit, line, isRow: false, out step)) return true;
                }
            }
        }
        step = default!;
        return false;
    }

    private bool TryLineToBox(ref SolverState state, int digit, int bit, int line, bool isRow, out SolveStep step)
    {
        var boxes = new HashSet<int>();
        for (var k = 0; k < 9; k++)
        {
            var (r, c) = isRow ? (line, k) : (k, line);
            if (state.Grid[r, c] != 0) continue;
            if ((state.UnitCandidates(r, c) & bit) == 0) continue;
            boxes.Add(SolverState.BoxIndex(r, c));
            if (boxes.Count > 1) break;
        }
        if (boxes.Count != 1) { step = default!; return false; }

        var targetBox = boxes.First();
        var startR = (targetBox / 3) * 3;
        var startC = (targetBox % 3) * 3;
        for (var r = startR; r < startR + 3; r++)
        {
            for (var c = startC; c < startC + 3; c++)
            {
                if (state.Grid[r, c] != 0) continue;
                if (isRow && r == line) continue;
                if (!isRow && c == line) continue;
                var cands = state.UnitCandidates(r, c);
                if ((cands & bit) == 0) continue;
                var newCands = cands & ~bit;
                if (SolverState.PopCount(newCands) == 1)
                {
                    var d = SolverState.LowestBitIndex(newCands) + 1;
                    state.Place(r, c, d);
                    step = new SolveStep(Name, Level,
                        $"{Name}: digit {digit} in {(isRow ? "row" : "col")} {line} confined to box {targetBox}; placed ({r},{c})={d}");
                    return true;
                }
            }
        }
        step = default!;
        return false;
    }

    private bool TryEliminateInLine(ref SolverState state, int digit, int line, bool isRow, int excludeBox, out SolveStep step)
    {
        var bit = 1 << (digit - 1);
        for (var k = 0; k < 9; k++)
        {
            var (r, c) = isRow ? (line, k) : (k, line);
            if (SolverState.BoxIndex(r, c) == excludeBox) continue;
            if (state.Grid[r, c] != 0) continue;
            var cands = state.UnitCandidates(r, c);
            if ((cands & bit) == 0) continue;
            var newCands = cands & ~bit;
            if (SolverState.PopCount(newCands) == 1)
            {
                var d = SolverState.LowestBitIndex(newCands) + 1;
                state.Place(r, c, d);
                step = new SolveStep(Name, Level,
                    $"{Name}: digit {digit} in box {excludeBox} confined to {(isRow ? "row" : "col")} {line}; placed ({r},{c})={d}");
                return true;
            }
        }
        step = default!;
        return false;
    }
}
