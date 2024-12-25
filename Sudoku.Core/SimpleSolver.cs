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
        int[][] result = new int[9][];
        for (int i = 0; i < 9; i++)
        {
            result[i] = new int[9];
            for (int j = 0; j < 9; j++)
            {
                result[i][j] = sudoku[i][j];
            }
        }
        return result;
    }
}