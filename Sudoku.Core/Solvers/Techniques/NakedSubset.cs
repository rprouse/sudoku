namespace Sudoku.Core.Solvers.Techniques;

internal sealed class NakedSubset : ITechnique
{
    private readonly int _size;
    public DifficultyTechnique Level { get; }
    public string Name { get; }

    public NakedSubset(int size, DifficultyTechnique level, string name)
    {
        _size = size;
        Level = level;
        Name = name;
    }

    public bool TryApply(ref SolverState state, out SolveStep step)
    {
        for (var u = 0; u < 9; u++)
        {
            if (TryUnit(ref state, RowCells(u), "row " + u, out step)) return true;
            if (TryUnit(ref state, ColCells(u), "col " + u, out step)) return true;
            if (TryUnit(ref state, BoxCells(u), "box " + u, out step)) return true;
        }
        step = default!;
        return false;
    }

    private bool TryUnit(ref SolverState state, (int r, int c)[] cells, string label, out SolveStep step)
    {
        // Collect candidate masks of empty cells.
        var empties = new List<(int r, int c, int mask)>();
        foreach (var (r, c) in cells)
        {
            if (state.Grid[r, c] == 0)
            {
                empties.Add((r, c, state.UnitCandidates(r, c)));
            }
        }
        if (empties.Count <= _size)
        {
            step = default!;
            return false;
        }

        // Try every combination of _size cells whose union of candidates is _size bits.
        var indices = new int[_size];
        return TryCombination(ref state, empties, indices, 0, 0, label, out step);
    }

    private bool TryCombination(
        ref SolverState state,
        List<(int r, int c, int mask)> empties,
        int[] indices,
        int depth,
        int start,
        string label,
        out SolveStep step)
    {
        if (depth == _size)
        {
            var union = 0;
            for (var i = 0; i < _size; i++) union |= empties[indices[i]].mask;
            if (SolverState.PopCount(union) != _size)
            {
                step = default!;
                return false;
            }

            // Eliminate these candidates from other empty cells in the unit.
            // We can only track this via placements: if removing union from
            // another cell leaves exactly one candidate, place it.
            for (var j = 0; j < empties.Count; j++)
            {
                if (Contains(indices, j)) continue;
                var (er, ec, em) = empties[j];
                var reduced = em & ~union;
                if (SolverState.PopCount(reduced) == 1)
                {
                    var digit = SolverState.LowestBitIndex(reduced) + 1;
                    state.Place(er, ec, digit);
                    step = new SolveStep(Name, Level, $"{Name} in {label}: placed ({er},{ec})={digit}");
                    return true;
                }
            }

            step = default!;
            return false;
        }

        for (var i = start; i < empties.Count; i++)
        {
            indices[depth] = i;
            if (TryCombination(ref state, empties, indices, depth + 1, i + 1, label, out step)) return true;
        }
        step = default!;
        return false;
    }

    private static bool Contains(int[] arr, int v)
    {
        for (var i = 0; i < arr.Length; i++) if (arr[i] == v) return true;
        return false;
    }

    internal static (int r, int c)[] RowCells(int r)
    {
        var cells = new (int, int)[9];
        for (var c = 0; c < 9; c++) cells[c] = (r, c);
        return cells;
    }

    internal static (int r, int c)[] ColCells(int c)
    {
        var cells = new (int, int)[9];
        for (var r = 0; r < 9; r++) cells[r] = (r, c);
        return cells;
    }

    internal static (int r, int c)[] BoxCells(int box)
    {
        var cells = new (int, int)[9];
        var startR = (box / 3) * 3;
        var startC = (box % 3) * 3;
        var i = 0;
        for (var r = startR; r < startR + 3; r++)
            for (var c = startC; c < startC + 3; c++)
                cells[i++] = (r, c);
        return cells;
    }
}
