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
            if (TryUnit(ref state, SolverState.RowCells[u], "row", u, out step)) return true;
            if (TryUnit(ref state, SolverState.ColCells[u], "col", u, out step)) return true;
            if (TryUnit(ref state, SolverState.BoxCells[u], "box", u, out step)) return true;
        }
        step = default!;
        return false;
    }

    private bool TryUnit(ref SolverState state, (int r, int c)[] cells, string unitKind, int unitIndex, out SolveStep step)
    {
        var empties = new List<(int r, int c, int mask)>();
        foreach (var (r, c) in cells)
        {
            if (state.Grid[r, c] == 0)
            {
                empties.Add((r, c, state.CellCandidates[r, c]));
            }
        }
        if (empties.Count <= _size)
        {
            step = default!;
            return false;
        }

        var indices = new int[_size];
        return TryCombination(ref state, empties, indices, 0, 0, unitKind, unitIndex, out step);
    }

    private bool TryCombination(
        ref SolverState state,
        List<(int r, int c, int mask)> empties,
        int[] indices,
        int depth,
        int start,
        string unitKind,
        int unitIndex,
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

            // Eliminate subset digits from other empty cells in the unit.
            var anyEliminated = false;
            var firstElimR = 0;
            var firstElimC = 0;
            var firstElimDigit = 0;
            for (var j = 0; j < empties.Count; j++)
            {
                if (Contains(indices, j)) continue;
                var (er, ec, _) = empties[j];
                var tmp = union;
                while (tmp != 0)
                {
                    var lowestBit = tmp & -tmp;
                    var digit = SolverState.LowestBitIndex(lowestBit) + 1;
                    if (state.EliminateCandidate(er, ec, digit) && !anyEliminated)
                    {
                        anyEliminated = true;
                        firstElimR = er;
                        firstElimC = ec;
                        firstElimDigit = digit;
                    }
                    tmp &= tmp - 1;
                }
            }

            if (anyEliminated)
            {
                step = new SolveStep(Name, Level, $"{Name} in {unitKind} {unitIndex}: eliminated {firstElimDigit} from ({firstElimR},{firstElimC})");
                return true;
            }

            step = default!;
            return false;
        }

        for (var i = start; i < empties.Count; i++)
        {
            indices[depth] = i;
            if (TryCombination(ref state, empties, indices, depth + 1, i + 1, unitKind, unitIndex, out step)) return true;
        }
        step = default!;
        return false;
    }

    private static bool Contains(int[] arr, int v)
    {
        for (var i = 0; i < arr.Length; i++) if (arr[i] == v) return true;
        return false;
    }
}
