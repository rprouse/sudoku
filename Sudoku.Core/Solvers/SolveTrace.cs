namespace Sudoku.Core.Solvers;

public record SolveTrace(IReadOnlyList<SolveStep> Steps, bool Solved)
{
    public DifficultyTechnique? HardestTechnique =>
        Steps.Count == 0 ? null : Steps.Max(s => s.Level);
}
