namespace Sudoku.Core.Solvers.Techniques;

// Implementation contract for techniques:
// - Always read candidates via state.UnitCandidates(r, c) — NEVER read
//   state.CellCandidates directly. CellCandidates is left stale by Place
//   and Remove (peer cells are not updated), so reading it can yield wrong
//   answers for any cell other than the one whose candidates were just
//   zeroed.
// - When making progress, mutate state via Place (placement) or by
//   recording eliminations in a way appropriate to the technique.
// - Emit a SolveStep describing what was done — step.Description is used
//   by tests and diagnostics.
internal interface ITechnique
{
    DifficultyTechnique Level { get; }
    string Name { get; }

    // Returns true if the technique made progress. On true, the technique
    // has mutated state (placed a digit and/or eliminated candidates) and
    // populated step. On false, step is default and state is unchanged.
    bool TryApply(ref SolverState state, out SolveStep step);
}
