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
                var rowBits = 0;
                var colBits = 0;
                for (var r = startR; r < startR + 3; r++)
                {
                    for (var c = startC; c < startC + 3; c++)
                    {
                        if (state.Grid[r, c] != 0) continue;
                        if ((state.CellCandidates[r, c] & bit) == 0) continue;
                        rowBits |= 1 << r;
                        colBits |= 1 << c;
                    }
                }

                var rowCount = SolverState.PopCount(rowBits);
                var colCount = SolverState.PopCount(colBits);

                if (rowCount == 1 && colCount >= 2)
                {
                    var targetRow = SolverState.LowestBitIndex(rowBits);
                    if (TryEliminateInLine(ref state, d, targetRow, isRow: true, excludeBox: box, out step))
                        return true;
                }
                if (colCount == 1 && rowCount >= 2)
                {
                    var targetCol = SolverState.LowestBitIndex(colBits);
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
        var boxBits = 0;
        for (var k = 0; k < 9; k++)
        {
            var (r, c) = isRow ? (line, k) : (k, line);
            if (state.Grid[r, c] != 0) continue;
            if ((state.CellCandidates[r, c] & bit) == 0) continue;
            boxBits |= 1 << SolverState.BoxIndex(r, c);
            if (SolverState.PopCount(boxBits) > 1) break;
        }
        if (SolverState.PopCount(boxBits) != 1) { step = default!; return false; }

        var targetBox = SolverState.LowestBitIndex(boxBits);
        var startR = (targetBox / 3) * 3;
        var startC = (targetBox % 3) * 3;
        var anyEliminated = false;
        var firstElimR = 0;
        var firstElimC = 0;
        for (var r = startR; r < startR + 3; r++)
        {
            for (var c = startC; c < startC + 3; c++)
            {
                if (state.Grid[r, c] != 0) continue;
                if (isRow && r == line) continue;
                if (!isRow && c == line) continue;
                if ((state.CellCandidates[r, c] & bit) == 0) continue;
                if (state.EliminateCandidate(r, c, digit) && !anyEliminated)
                {
                    anyEliminated = true;
                    firstElimR = r;
                    firstElimC = c;
                }
            }
        }
        if (anyEliminated)
        {
            step = new SolveStep(Name, Level,
                $"{Name}: digit {digit} in {(isRow ? "row" : "col")} {line} confined to box {targetBox}; eliminated from ({firstElimR},{firstElimC})");
            return true;
        }
        step = default!;
        return false;
    }

    private bool TryEliminateInLine(ref SolverState state, int digit, int line, bool isRow, int excludeBox, out SolveStep step)
    {
        var bit = 1 << (digit - 1);
        var anyEliminated = false;
        var firstElimR = 0;
        var firstElimC = 0;
        for (var k = 0; k < 9; k++)
        {
            var (r, c) = isRow ? (line, k) : (k, line);
            if (SolverState.BoxIndex(r, c) == excludeBox) continue;
            if (state.Grid[r, c] != 0) continue;
            if ((state.CellCandidates[r, c] & bit) == 0) continue;
            if (state.EliminateCandidate(r, c, digit) && !anyEliminated)
            {
                anyEliminated = true;
                firstElimR = r;
                firstElimC = c;
            }
        }
        if (anyEliminated)
        {
            step = new SolveStep(Name, Level,
                $"{Name}: digit {digit} in box {excludeBox} confined to {(isRow ? "row" : "col")} {line}; eliminated from ({firstElimR},{firstElimC})");
            return true;
        }
        step = default!;
        return false;
    }
}
