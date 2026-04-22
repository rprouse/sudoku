namespace Sudoku.Core.Solvers.Techniques;

internal sealed class NakedSingle : ITechnique
{
    public DifficultyTechnique Level => DifficultyTechnique.NakedSingle;
    public string Name => "Naked Single";

    public bool TryApply(ref SolverState state, out SolveStep step)
    {
        for (var r = 0; r < 9; r++)
        {
            for (var c = 0; c < 9; c++)
            {
                if (state.Grid[r, c] != 0) continue;
                var mask = state.CellCandidates[r, c];
                if (SolverState.PopCount(mask) == 1)
                {
                    var digit = SolverState.LowestBitIndex(mask) + 1;
                    state.PlacePropagating(r, c, digit);
                    step = new SolveStep(Name, Level, $"Naked single ({r},{c})={digit}");
                    return true;
                }
            }
        }
        step = default!;
        return false;
    }
}
