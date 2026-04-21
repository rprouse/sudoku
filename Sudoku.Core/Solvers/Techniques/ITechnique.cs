namespace Sudoku.Core.Solvers.Techniques;

internal interface ITechnique
{
    DifficultyTechnique Level { get; }
    string Name { get; }

    // Returns true if the technique made progress. On true, the technique
    // has mutated state (placed a digit and/or eliminated candidates) and
    // populated step. On false, step is default and state is unchanged.
    bool TryApply(ref SolverState state, out SolveStep step);
}
