using Sudoku.Core.Solvers;

namespace Sudoku.Core.Generators;

public class DifficultyGrader
{
    private readonly ITechniqueSolver _solver;

    public DifficultyGrader(ITechniqueSolver solver)
    {
        _solver = solver;
    }

    public Difficulty? Grade(SudokuBoard puzzle)
    {
        var trace = _solver.Solve(puzzle);
        if (!trace.Solved) return null;
        if (trace.HardestTechnique is null) return Difficulty.Easy; // already solved (0 steps)
        return TechniqueTier(trace.HardestTechnique.Value);
    }

    private static Difficulty TechniqueTier(DifficultyTechnique t) => t switch
    {
        DifficultyTechnique.NakedSingle or DifficultyTechnique.HiddenSingle
            => Difficulty.Easy,
        DifficultyTechnique.NakedPair or DifficultyTechnique.HiddenPair
            => Difficulty.Medium,
        DifficultyTechnique.NakedTriple or DifficultyTechnique.HiddenTriple
            or DifficultyTechnique.NakedQuad or DifficultyTechnique.HiddenQuad
            or DifficultyTechnique.PointingPair or DifficultyTechnique.BoxLineReduction
            => Difficulty.Hard,
        DifficultyTechnique.XWing or DifficultyTechnique.XYWing or DifficultyTechnique.SimpleColoring
            => Difficulty.Expert,
        DifficultyTechnique.Swordfish or DifficultyTechnique.XYZWing or DifficultyTechnique.XChain
            => Difficulty.Evil,
        _ => throw new ArgumentOutOfRangeException(nameof(t))
    };
}
