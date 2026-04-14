using FluentAssertions.Execution;

namespace Sudoku.Tests.Extensions;

public static class TestExtensions
{
    public static void ShouldBeEqualTo(this SudokuBoard board, SudokuBoard actual)
    {
        using (new AssertionScope())
        {
            for (var row = 0; row < 9; row++)
            {
                for (var col = 0; col < 9; col++)
                {
                    board[row, col].Should().Be(actual[row, col]);
                }
            }
        }
    }

    public static void ShouldBeEqualTo(this SudokuBoard board, int[][] actual)
    {
        using (new AssertionScope())
        {
            for (var row = 0; row < 9; row++)
            {
                for (var col = 0; col < 9; col++)
                {
                    board[row, col].Should().Be(actual[row][col]);
                }
            }
        }
    }
}