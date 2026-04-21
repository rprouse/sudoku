namespace Sudoku.Core.Solvers.Techniques;

internal sealed class Wing : ITechnique
{
    private readonly int _pivotSize;
    public DifficultyTechnique Level { get; }
    public string Name { get; }

    public Wing(int pivotSize, DifficultyTechnique level, string name)
    {
        _pivotSize = pivotSize;
        Level = level;
        Name = name;
    }

    public bool TryApply(ref SolverState state, out SolveStep step)
    {
        // Enumerate pivots by candidate count equal to _pivotSize.
        for (var pr = 0; pr < 9; pr++)
        {
            for (var pc = 0; pc < 9; pc++)
            {
                if (state.Grid[pr, pc] != 0) continue;
                var pivotMask = state.CellCandidates[pr, pc];
                if (SolverState.PopCount(pivotMask) != _pivotSize) continue;

                // For each pair of "pincer" cells seen from pivot that are bivalue.
                var pincers = FindPincers(in state, pr, pc);
                for (var i = 0; i < pincers.Count; i++)
                {
                    for (var j = i + 1; j < pincers.Count; j++)
                    {
                        if (TryXyOrXyzWing(ref state, pr, pc, pivotMask, pincers[i], pincers[j], out step))
                            return true;
                    }
                }
            }
        }
        step = default!;
        return false;
    }

    private static List<(int r, int c, int mask)> FindPincers(in SolverState state, int pr, int pc)
    {
        var list = new List<(int r, int c, int mask)>();
        for (var r = 0; r < 9; r++)
        {
            for (var c = 0; c < 9; c++)
            {
                if (r == pr && c == pc) continue;
                if (state.Grid[r, c] != 0) continue;
                if (!SeesEachOther(pr, pc, r, c)) continue;
                var m = state.CellCandidates[r, c];
                if (SolverState.PopCount(m) == 2) list.Add((r, c, m));
            }
        }
        return list;
    }

    private bool TryXyOrXyzWing(ref SolverState state, int pr, int pc, int pivotMask,
        (int r, int c, int mask) a, (int r, int c, int mask) b, out SolveStep step)
    {
        // Union of candidates across pivot + both pincers must be 3 (i.e., {X, Y, Z}).
        var combined = pivotMask | a.mask | b.mask;
        if (SolverState.PopCount(combined) != 3) { step = default!; return false; }

        // Z is the candidate appearing in BOTH pincers.
        var z = a.mask & b.mask;
        if (SolverState.PopCount(z) != 1) { step = default!; return false; }

        // For XY-Wing: pivot must NOT contain Z. For XYZ-Wing: pivot must contain Z.
        var zInPivot = (pivotMask & z) != 0;
        if (_pivotSize == 2 && zInPivot) { step = default!; return false; }
        if (_pivotSize == 3 && !zInPivot) { step = default!; return false; }

        // Each pincer shares exactly one candidate with the pivot.
        if (SolverState.PopCount(pivotMask & a.mask) != 1) { step = default!; return false; }
        if (SolverState.PopCount(pivotMask & b.mask) != 1) { step = default!; return false; }
        // And the pincers' "shared with pivot" candidates must be different
        // (one is X, the other is Y — together with Z in both pincers, this
        // is the wing configuration).
        if ((pivotMask & a.mask) == (pivotMask & b.mask)) { step = default!; return false; }

        // Eliminate Z from cells seeing both pincers (and pivot, for XYZ).
        var digit = SolverState.LowestBitIndex(z) + 1;
        var anyEliminated = false;
        var firstElimR = 0;
        var firstElimC = 0;
        for (var r = 0; r < 9; r++)
        {
            for (var c = 0; c < 9; c++)
            {
                if (state.Grid[r, c] != 0) continue;
                if (r == pr && c == pc) continue;
                if (r == a.r && c == a.c) continue;
                if (r == b.r && c == b.c) continue;
                if (!SeesEachOther(a.r, a.c, r, c)) continue;
                if (!SeesEachOther(b.r, b.c, r, c)) continue;
                if (_pivotSize == 3 && !SeesEachOther(pr, pc, r, c)) continue;

                if ((state.CellCandidates[r, c] & z) == 0) continue;
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
            step = new SolveStep(Name, Level, $"{Name}: eliminated {digit} from ({firstElimR},{firstElimC})");
            return true;
        }
        step = default!;
        return false;
    }

    private static bool SeesEachOther(int r1, int c1, int r2, int c2) =>
        r1 == r2 || c1 == c2 || SolverState.BoxIndex(r1, c1) == SolverState.BoxIndex(r2, c2);
}
