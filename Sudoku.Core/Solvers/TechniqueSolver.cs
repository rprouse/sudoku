using Sudoku.Core.Solvers.Techniques;

namespace Sudoku.Core.Solvers;

public sealed class TechniqueSolver : ITechniqueSolver
{
    // Ordered easiest-first. Later tasks append techniques to this list.
    private readonly ITechnique[] _techniques =
    {
        new NakedSingle(),
        new HiddenSingle(),
        new NakedSubset(2, DifficultyTechnique.NakedPair, "Naked Pair"),
        new HiddenSubset(2, DifficultyTechnique.HiddenPair, "Hidden Pair"),
        new NakedSubset(3, DifficultyTechnique.NakedTriple, "Naked Triple"),
        new HiddenSubset(3, DifficultyTechnique.HiddenTriple, "Hidden Triple"),
        new NakedSubset(4, DifficultyTechnique.NakedQuad, "Naked Quad"),
        new HiddenSubset(4, DifficultyTechnique.HiddenQuad, "Hidden Quad"),
        new Intersection(pointing: true),
        new Intersection(pointing: false),
        new Fish(2, DifficultyTechnique.XWing, "X-Wing"),
        new Wing(2, DifficultyTechnique.XYWing, "XY-Wing"),
        new Coloring(DifficultyTechnique.SimpleColoring, "Simple Coloring"),
        new Fish(3, DifficultyTechnique.Swordfish, "Swordfish"),
        new Wing(3, DifficultyTechnique.XYZWing, "XYZ-Wing"),
        new Coloring(DifficultyTechnique.XChain, "X-Chain")
    };

    public SolveTrace Solve(SudokuBoard board)
    {
        var state = SolverState.FromBoard(board);
        var steps = new List<SolveStep>();

        while (state.EmptyCount > 0)
        {
            var madeProgress = false;
            foreach (var technique in _techniques)
            {
                if (technique.TryApply(ref state, out var step))
                {
                    steps.Add(step);
                    madeProgress = true;
                    break; // restart from the top (easiest technique first)
                }
            }
            if (!madeProgress) break;
        }

        return new SolveTrace(steps, state.EmptyCount == 0);
    }
}
