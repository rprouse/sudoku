namespace Sudoku.Core.Solvers.Techniques;

internal sealed class Coloring : ITechnique
{
    public DifficultyTechnique Level { get; }
    public string Name { get; }

    public Coloring(DifficultyTechnique level, string name)
    {
        Level = level;
        Name = name;
    }

    public bool TryApply(ref SolverState state, out SolveStep step)
    {
        for (var d = 1; d <= 9; d++)
        {
            var bit = 1 << (d - 1);
            var edges = BuildStrongLinks(in state, bit);
            if (edges.Count == 0) continue;
            if (TryColorFromEdges(ref state, edges, d, bit, out step)) return true;
        }
        step = default!;
        return false;
    }

    private static List<((int r, int c) a, (int r, int c) b)> BuildStrongLinks(in SolverState state, int bit)
    {
        var edges = new List<((int, int), (int, int))>();
        for (var u = 0; u < 9; u++)
        {
            AddIfTwo(in state, bit, SolverState.RowCells[u], edges);
            AddIfTwo(in state, bit, SolverState.ColCells[u], edges);
            AddIfTwo(in state, bit, SolverState.BoxCells[u], edges);
        }
        return edges;
    }

    private static void AddIfTwo(in SolverState state, int bit, (int r, int c)[] cells, List<((int, int), (int, int))> edges)
    {
        (int, int) a = default;
        (int, int) b = default;
        var count = 0;
        foreach (var (r, c) in cells)
        {
            if (state.Grid[r, c] != 0) continue;
            if ((state.CellCandidates[r, c] & bit) == 0) continue;
            count++;
            if (count == 1) a = (r, c);
            else if (count == 2) b = (r, c);
            else return;
        }
        if (count == 2) edges.Add((a, b));
    }

    private bool TryColorFromEdges(
        ref SolverState state,
        List<((int r, int c) a, (int r, int c) b)> edges,
        int digit,
        int bit,
        out SolveStep step)
    {
        // Build adjacency.
        var adj = new Dictionary<(int, int), List<(int, int)>>();
        foreach (var (a, b) in edges)
        {
            if (!adj.ContainsKey(a)) adj[a] = new List<(int, int)>();
            if (!adj.ContainsKey(b)) adj[b] = new List<(int, int)>();
            adj[a].Add(b);
            adj[b].Add(a);
        }

        var visited = new Dictionary<(int, int), int>();
        foreach (var start in adj.Keys)
        {
            if (visited.ContainsKey(start)) continue;
            var queue = new Queue<((int, int) cell, int color)>();
            queue.Enqueue((start, 0));
            visited[start] = 0;
            var componentA = new List<(int, int)>();
            var componentB = new List<(int, int)>();
            while (queue.Count > 0)
            {
                var (cell, color) = queue.Dequeue();
                (color == 0 ? componentA : componentB).Add(cell);
                foreach (var next in adj[cell])
                {
                    if (visited.ContainsKey(next)) continue;
                    visited[next] = 1 - color;
                    queue.Enqueue((next, 1 - color));
                }
            }

            // Same-color conflict: two cells of color A see each other → all color A is false, so all color B must hold the digit.
            if (HasSameColorConflict(componentA))
            {
                if (TryPlaceAny(ref state, componentB, digit, bit, $"{Name}: color A conflict, placed ", out step))
                    return true;
            }
            if (HasSameColorConflict(componentB))
            {
                if (TryPlaceAny(ref state, componentA, digit, bit, $"{Name}: color B conflict, placed ", out step))
                    return true;
            }

            // Bridged elimination: cell seeing both colors can't hold digit.
            if (TryEliminationFromColoring(ref state, componentA, componentB, bit, digit, out step)) return true;
        }
        step = default!;
        return false;
    }

    private static bool HasSameColorConflict(List<(int r, int c)> cells)
    {
        for (var i = 0; i < cells.Count; i++)
            for (var j = i + 1; j < cells.Count; j++)
                if (Sees(cells[i], cells[j])) return true;
        return false;
    }

    private bool TryPlaceAny(
        ref SolverState state,
        List<(int r, int c)> cells,
        int digit,
        int bit,
        string reasonPrefix,
        out SolveStep step)
    {
        // Place the digit in the first cell of the component that can still hold it.
        foreach (var (r, c) in cells)
        {
            if (state.Grid[r, c] != 0) continue;
            if ((state.CellCandidates[r, c] & bit) == 0) continue;
            // Only place if the digit is truly forced — other candidates for the cell
            // must be eliminated by conflict. Since the coloring tells us the digit
            // MUST go here (color-conflict case), we place unconditionally.
            state.PlacePropagating(r, c, digit);
            step = new SolveStep(Name, Level, $"{reasonPrefix}({r},{c})={digit}");
            return true;
        }
        step = default!;
        return false;
    }

    private bool TryEliminationFromColoring(
        ref SolverState state,
        List<(int r, int c)> colorA,
        List<(int r, int c)> colorB,
        int bit,
        int digit,
        out SolveStep step)
    {
        var anyEliminated = false;
        var firstElimR = 0;
        var firstElimC = 0;
        for (var r = 0; r < 9; r++)
        {
            for (var c = 0; c < 9; c++)
            {
                if (state.Grid[r, c] != 0) continue;
                var cell = (r, c);
                if (colorA.Contains(cell) || colorB.Contains(cell)) continue;
                if ((state.CellCandidates[r, c] & bit) == 0) continue;
                if (!colorA.Any(a => Sees(a, cell))) continue;
                if (!colorB.Any(b => Sees(b, cell))) continue;

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

    private static bool Sees((int r, int c) a, (int r, int c) b)
    {
        if (a == b) return false;
        return a.r == b.r || a.c == b.c || SolverState.BoxIndex(a.r, a.c) == SolverState.BoxIndex(b.r, b.c);
    }
}
