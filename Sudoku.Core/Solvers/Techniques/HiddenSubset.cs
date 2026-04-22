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
        // host any of those digits, those cells form a hidden subset.
        // The hidden-subset property forces each host cell to hold one
        // of the N subset digits, so when a host's live candidates &
        // subset mask has popcount 1, that digit is placed — regardless
        // of any outside candidates the cell may also have.
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
                if ((state.CellCandidates[r, c] & digitMask) != 0) hostCells.Add((r, c));
            }
            if (hostCells.Count != _size)
            {
                step = default!;
                return false;
            }

            // Hidden subset identified. Host cells can only hold subset digits.
            // Eliminate all non-subset candidates from each host cell.
            var anyEliminated = false;
            var firstElimR = 0;
            var firstElimC = 0;
            var firstElimDigit = 0;
            foreach (var (r, c) in hostCells)
            {
                var nonSubset = state.CellCandidates[r, c] & ~digitMask & SolverState.All;
                var tmp = nonSubset;
                while (tmp != 0)
                {
                    var lowestBit = tmp & -tmp;
                    var digit = SolverState.LowestBitIndex(lowestBit) + 1;
                    if (state.EliminateCandidate(r, c, digit) && !anyEliminated)
                    {
                        anyEliminated = true;
                        firstElimR = r;
                        firstElimC = c;
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

        for (var i = start; i < missingDigits.Count; i++)
        {
            indices[depth] = i;
            if (TryCombination(ref state, cells, missingDigits, indices, depth + 1, i + 1, unitKind, unitIndex, out step)) return true;
        }
        step = default!;
        return false;
    }
}
