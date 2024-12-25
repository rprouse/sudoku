using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku.Tests.Extensions;

public class SudokuBoardExtensionsTests : SolverTestsBase
{
    [Test]
    public void IsValidRowShouldReturnTrueWhenSolvedAndRowIsValid()
    {
        var result = SOLVED_VALID.IsValidRows();
        result.Should().BeTrue();
    }

    [Test]
    public void IsValidRowShouldReturnTrueWhenUnsolvedAndRowIsValid()
    {
        var result = UNSOLVED_VALID.IsValidRows();
        result.Should().BeTrue();
    }

    [Test]
    public void IsValidRowShouldReturnFalseWhenSolvedAndRowIsInvalid()
    {
        var result = SOLVED_INVALID_ROW.IsValidRows();
        result.Should().BeFalse();
    }

    [Test]
    public void IsValidRowShouldReturnFalseWhenUnsolvedAndRowIsInvalid()
    {
        var result = UNSOLVED_INVALID_ROW.IsValidRows();
        result.Should().BeFalse();
    }

    [Test]
    public void IsValidColumnShouldReturnTrueWhenSolvedAndColumnIsValid()
    {
        var result = SOLVED_VALID.IsValidColumns();
        result.Should().BeTrue();
    }

    [Test]
    public void IsValidColumnShouldReturnTrueWhenUnsolvedAndColumnIsValid()
    {
        var result = UNSOLVED_VALID.IsValidColumns();
        result.Should().BeTrue();
    }

    [Test]
    public void IsValidColumnShouldReturnFalseWhenSolvedAndColumnIsInvalid()
    {
        var result = SOLVED_INVALID_COLUMN.IsValidColumns();
        result.Should().BeFalse();
    }

    [Test]
    public void IsValidColumnShouldReturnFalseWhenUnsolvedAndColumnIsInvalid()
    {
        var result = UNSOLVED_INVALID_COLUMN.IsValidColumns();
        result.Should().BeFalse();
    }

    [Test]
    public void IsValidBoxShouldReturnTrueWhenSolvedAndBoxIsValid()
    {
        var result = SOLVED_VALID.IsValidBoxes();
        result.Should().BeTrue();
    }

    [Test]
    public void IsValidBoxShouldReturnTrueWhenUnsolvedAndBoxIsValid()
    {
        var result = UNSOLVED_VALID.IsValidBoxes();
        result.Should().BeTrue();
    }

    [Test]
    public void IsValidBoxShouldReturnFalseWhenSolvedAndBoxIsInvalid()
    {
        var result = SOLVED_INVALID_BOX.IsValidBoxes();
        result.Should().BeFalse();
    }

    [Test]
    public void IsValidBoxShouldReturnFalseWhenUnsolvedAndBoxIsInvalid()
    {
        var result = UNSOLVED_INVALID_BOX.IsValidBoxes();
        result.Should().BeFalse();
    }

    [Test]
    public void IsFilledShouldReturnFalseWhenSudokuIsNotFilled()
    {
        var result = UNSOLVED_VALID.IsFilled();
        result.Should().BeFalse();
    }

    [Test]
    public void IsFilledShouldReturnTrueWhenSudokuIsFilled()
    {
        var result = SOLVED_VALID.IsFilled();
        result.Should().BeTrue();
    }

    [Test]
    public void IsValidShouldReturnTrueWhenSudokuIsUnsolvedAndValid()
    {
        var result = UNSOLVED_VALID.IsValid();
        result.Should().BeTrue();
    }

    [Test]
    public void IsValidShouldReturnTrueWhenSudokuIsSolvedAndValid()
    {
        var result = SOLVED_VALID.IsValid();
        result.Should().BeTrue();
    }

    [TestCaseSource(nameof(InvalidSudokus))]
    public void IsValidShouldReturnFalseWhenSudokuIsInvalid(int[][] sudoku)
    {
        var result = sudoku.IsValid();
        result.Should().BeFalse();
    }

    public static IEnumerable<object[]> InvalidSudokus()
    {
        yield return UNSOLVED_INVALID_BOX;
        yield return SOLVED_INVALID_BOX;
        yield return UNSOLVED_INVALID_ROW;
        yield return UNSOLVED_INVALID_COLUMN;
    }

    [TestCaseSource(nameof(GetBoxData))]
    public void GetBoxReturnsBox(int x, int y, int[] expected)
    {
        IEnumerable<int> result = SOLVED_VALID.GetBox(x, y);
        result.Should().BeEquivalentTo(expected);
    }

    public static IEnumerable<object[]> GetBoxData()
    {
        yield return new object[] { 0, 0, new int[] { 5, 3, 4, 1, 6, 2, 9, 8, 7 } };
        yield return new object[] { 1, 1, new int[] { 7, 6, 2, 8, 1, 3, 5, 9, 4 } };
        yield return new object[] { 2, 2, new int[] { 9, 4, 3, 1, 2, 5, 6, 8, 7 } };
    }
}
