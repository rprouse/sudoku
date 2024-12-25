using FluentAssertions.Execution;

namespace Sudoku.Tests.Extensions;

public static class TestExtensions
{
    public static void ShouldBeEqualTo(this int[][] board, int[][] actual)
    {
        using (new AssertionScope())
        {
            board[0].Should().BeEquivalentTo(actual[0]);
            board[1].Should().BeEquivalentTo(actual[1]);
            board[2].Should().BeEquivalentTo(actual[2]);
            board[3].Should().BeEquivalentTo(actual[3]);
            board[4].Should().BeEquivalentTo(actual[4]);
            board[5].Should().BeEquivalentTo(actual[5]);
            board[6].Should().BeEquivalentTo(actual[6]);
            board[7].Should().BeEquivalentTo(actual[7]);
            board[8].Should().BeEquivalentTo(actual[8]);
        }
    }
}