using Sudoku.Core.Solvers.Techniques;

namespace Sudoku.Core.Solvers;

public class TechniqueSolver : ITechniqueSolver
{
    // Ordered easiest-first. Later tasks append techniques to this list.
    private readonly ITechnique[] _techniques =
    {
        new NakedSingle(),
        new HiddenSingle()
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
