namespace Sudoku.Core.Solvers.Techniques;

internal sealed class HiddenSubset : ITechnique
{
    private readonly int _size;
    public DifficultyTechnique Level { get; }
    public string Name { get; }

    public HiddenSubset(int size, DifficultyTechnique level, string name)
    {
        _size = size;
        Level = level;
        Name = name;
    }

    public bool TryApply(ref SolverState state, out SolveStep step)
    {
        for (var u = 0; u < 9; u++)
        {
            if (TryUnit(ref state, SolverState.RowCells[u], state.RowMask[u], "row", u, out step)) return true;
            if (TryUnit(ref state, SolverState.ColCells[u], state.ColMask[u], "col", u, out step)) return true;
            if (TryUnit(ref state, SolverState.BoxCells[u], state.BoxMask[u], "box", u, out step)) return true;
        }
        step = default!;
        return false;
    }

    private bool TryUnit(ref SolverState state, (int r, int c)[] cells, int unitPlaced, string unitKind, int unitIndex, out SolveStep step)
    {
        // Digits not yet placed in the unit.
        var missing = (~unitPlaced) & SolverState.All;
        var missingDigits = new List<int>();
        for (var d = 1; d <= 9; d++)
        {
            if ((missing & (1 << (d - 1))) != 0) missingDigits.Add(d);
        }
        if (missingDigits.Count <= _size)
        {
            step = default!;
            return false;
        }

        // Try every N-subset of missing digits. If exactly N empty cells
        // host any of those digits, those cells form a hidden subset —
        // they can only hold those N digits. When a host cell's live
        // candidates intersected with the subset mask equal just one bit
        // AND the subset mask equals the cell's entire live candidates
        // (i.e., no other digit can go there), place the forced digit.
        var indices = new int[_size];
        return TryCombination(ref state, cells, missingDigits, indices, 0, 0, unitKind, unitIndex, out step);
    }

    private bool TryCombination(
        ref SolverState state,
        (int r, int c)[] cells,
        List<int> missingDigits,
        int[] indices,
        int depth,
        int start,
        string unitKind,
        int unitIndex,
        out SolveStep step)
    {
        if (depth == _size)
        {
            var digitMask = 0;
            for (var i = 0; i < _size; i++) digitMask |= 1 << (missingDigits[indices[i]] - 1);

            // Host cells: empty cells in the unit that can hold any of these digits.
            var hostCells = new List<(int r, int c)>();
            foreach (var (r, c) in cells)
            {
                if (state.Grid[r, c] != 0) continue;
                if ((state.UnitCandidates(r, c) & digitMask) != 0) hostCells.Add((r, c));
            }
            if (hostCells.Count != _size)
            {
                step = default!;
                return false;
            }

            // Hidden subset identified. Because exactly _size cells can hold
            // any of the _size subset digits, each host cell can only be assigned
            // one of those digits. If a host cell's intersection with digitMask has
            // popcount 1, that digit is forced into that cell regardless of any
            // other (outside) candidates — the hidden-subset constraint confines the
            // subset digits to these cells, so if only one subset digit fits in a
            // host cell, it must go there.
            foreach (var (r, c) in hostCells)
            {
                var inside = state.UnitCandidates(r, c) & digitMask;
                if (SolverState.PopCount(inside) == 1)
                {
                    var digit = SolverState.LowestBitIndex(inside) + 1;
                    state.Place(r, c, digit);
                    step = new SolveStep(Name, Level, $"{Name} in {unitKind} {unitIndex}: placed ({r},{c})={digit}");
                    return true;
                }
            }

            // Pattern identified but no cell reduces to a single candidate here.
            // Return false so other techniques can make progress.
            step = default!;
            return false;
        }

        for (var i = start; i < missingDigits.Count; i++)
        {
            indices[depth] = i;
            if (TryCombination(ref state, cells, missingDigits, indices, depth + 1, i + 1, unitKind, unitIndex, out step)) return true;
        }
        step = default!;
        return false;
    }
}
