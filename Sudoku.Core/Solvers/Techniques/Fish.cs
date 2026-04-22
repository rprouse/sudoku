namespace Sudoku.Core.Solvers.Techniques;

internal sealed class Fish : ITechnique
{
    private readonly int _size;
    public DifficultyTechnique Level { get; }
    public string Name { get; }

    public Fish(int size, DifficultyTechnique level, string name)
    {
        _size = size;
        Level = level;
        Name = name;
    }

    public bool TryApply(ref SolverState state, out SolveStep step)
    {
        // For each digit, try row-fish then column-fish.
        for (var d = 1; d <= 9; d++)
        {
            var bit = 1 << (d - 1);
            if (TryFish(ref state, d, bit, rows: true, out step)) return true;
            if (TryFish(ref state, d, bit, rows: false, out step)) return true;
        }
        step = default!;
        return false;
    }

    private bool TryFish(ref SolverState state, int digit, int bit, bool rows, out SolveStep step)
    {
        // For each "base" unit (row when rows==true, column otherwise), find the
        // set of columns (or rows) where the digit is still a candidate.
        // A position set has popcount in [2, _size] to be a fish component.
        var baseSets = new List<(int unit, int posMask)>();
        for (var u = 0; u < 9; u++)
        {
            if (rows && (state.RowMask[u] & bit) != 0) continue;
            if (!rows && (state.ColMask[u] & bit) != 0) continue;
            var posMask = 0;
            for (var k = 0; k < 9; k++)
            {
                var (r, c) = rows ? (u, k) : (k, u);
                if (state.Grid[r, c] != 0) continue;
                if ((state.CellCandidates[r, c] & bit) != 0) posMask |= 1 << k;
            }
            var count = SolverState.PopCount(posMask);
            if (count >= 2 && count <= _size)
            {
                baseSets.Add((u, posMask));
            }
        }

        if (baseSets.Count < _size)
        {
            step = default!;
            return false;
        }

        var indices = new int[_size];
        return TryCombination(ref state, baseSets, indices, 0, 0, digit, bit, rows, out step);
    }

    private bool TryCombination(
        ref SolverState state,
        List<(int unit, int posMask)> baseSets,
        int[] indices,
        int depth,
        int start,
        int digit,
        int bit,
        bool rows,
        out SolveStep step)
    {
        if (depth == _size)
        {
            var union = 0;
            for (var i = 0; i < _size; i++) union |= baseSets[indices[i]].posMask;
            if (SolverState.PopCount(union) != _size)
            {
                step = default!;
                return false;
            }

            // Build the set of base units we picked.
            var baseUnitBits = 0;
            for (var i = 0; i < _size; i++) baseUnitBits |= 1 << baseSets[indices[i]].unit;

            // For each cross coordinate k in union, look at cells in cross units
            // not in baseUnitBits — eliminate digit from them.
            var anyEliminated = false;
            var firstElimR = 0;
            var firstElimC = 0;
            for (var k = 0; k < 9; k++)
            {
                if ((union & (1 << k)) == 0) continue;
                for (var u = 0; u < 9; u++)
                {
                    if ((baseUnitBits & (1 << u)) != 0) continue;
                    var (r, c) = rows ? (u, k) : (k, u);
                    if (state.Grid[r, c] != 0) continue;
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
                    $"{Name} on digit {digit}: eliminated from ({firstElimR},{firstElimC})");
                return true;
            }
            step = default!;
            return false;
        }

        for (var i = start; i < baseSets.Count; i++)
        {
            indices[depth] = i;
            if (TryCombination(ref state, baseSets, indices, depth + 1, i + 1, digit, bit, rows, out step)) return true;
        }
        step = default!;
        return false;
    }
}
