using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sudoku.Core.Extensions;

namespace Sudoku.Core;

public static class SimpleSolver
{
    public static int[][] Solve(int[][] sudoku)
    {
        return BruteForceSolve(sudoku).FirstOrDefault() ??
            throw new InvalidOperationException("No solution found.");
    }

    private static IEnumerable<int[][]> BruteForceSolve(int[][] sudoku)
    {
        if (sudoku.IsFilled())
        {
            yield return sudoku;
            yield break;
        }
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (sudoku[i][j] == 0)
                {
                    for (int k = 1; k <= 9; k++)
                    {
                        sudoku[i][j] = k;
                        if (sudoku.IsValid())
                        {
                            foreach (int[][] solution in BruteForceSolve(sudoku))
                            {
                                yield return solution;
                            }
                        }
                        sudoku[i][j] = 0;
                    }
                    yield break;
                }
            }
        }
    }
}