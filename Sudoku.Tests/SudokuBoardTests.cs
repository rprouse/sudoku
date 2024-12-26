namespace Sudoku.Tests;

public class SudokuBoardTests
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
    public void IsValidShouldReturnFalseWhenSudokuIsInvalid(SudokuBoard sudoku)
    {
        var result = sudoku.IsValid();
        result.Should().BeFalse();
    }

    public static IEnumerable<SudokuBoard> InvalidSudokus()
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

    [Test]
    public void NewSudokuShouldReturnNewSudokuArray()
    {
        var result = new SudokuBoard(UNSOLVED_VALID.Sudoku);
        result.Sudoku.Should().NotBeSameAs(UNSOLVED_VALID.Sudoku);
    }

    [Test]
    public void NewSudokuShouldReturnEqualSudoku()
    {
        var result = new SudokuBoard(UNSOLVED_VALID.Sudoku);
        result.Sudoku.Should().BeEquivalentTo(UNSOLVED_VALID.Sudoku);
    }

    [Test]
    public void ModifyingClonedSudokuShouldNotModifyOriginalSudoku()
    {
        var result = new SudokuBoard(UNSOLVED_VALID.Sudoku);
        result.Sudoku[0][0] = 1;
        UNSOLVED_VALID.Sudoku[0][0].Should().NotBe(1);
    }


    public static readonly SudokuBoard UNSOLVED_VALID = new(
    [
        [ 5, 3, 4, 9, 2, 0, 7, 0, 0 ],
        [ 0, 6, 0, 0, 0, 7, 3, 0, 9 ],
        [ 9, 0, 0, 0, 0, 0, 0, 1, 0 ],
        [ 0, 0, 8, 7, 0, 0, 0, 0, 0 ],
        [ 4, 9, 6, 8, 0, 3, 0, 0, 2 ],
        [ 7, 2, 1, 5, 9, 4, 8, 0, 6 ],
        [ 0, 0, 0, 2, 0, 0, 9, 4, 0 ],
        [ 8, 0, 0, 0, 4, 6, 1, 0, 0 ],
        [ 0, 0, 3, 0, 0, 0, 0, 0, 0 ]
    ]);

    public static readonly SudokuBoard SOLVED_VALID = new(
    [
        [ 5, 3, 4, 9, 2, 1, 7, 6, 8 ],
        [ 1, 6, 2, 4, 8, 7, 3, 5, 9 ],
        [ 9, 8, 7, 6, 3, 5, 2, 1, 4 ],
        [ 3, 5, 8, 7, 6, 2, 4, 9, 1 ],
        [ 4, 9, 6, 8, 1, 3, 5, 7, 2 ],
        [ 7, 2, 1, 5, 9, 4, 8, 3, 6 ],
        [ 6, 1, 5, 2, 7, 8, 9, 4, 3 ],
        [ 8, 7, 9, 3, 4, 6, 1, 2, 5 ],
        [ 2, 4, 3, 1, 5, 9, 6, 8, 7 ]
    ]);

    public static readonly SudokuBoard UNSOLVED_INVALID_BOX = new(
    [
        [ 5, 3, 4, 9, 2, 0, 7, 0, 0 ],
        [ 0, 6, 0, 0, 0, 7, 3, 0, 9 ],
        [ 9, 0, 0, 0, 0, 0, 0, 1, 0 ],
        [ 0, 0, 8, 7, 0, 0, 0, 0, 0 ],
        [ 4, 9, 6, 8, 0, 3, 0, 0, 2 ],
        [ 7, 2, 1, 5, 9, 4, 8, 0, 6 ],
        [ 0, 0, 8, 2, 0, 0, 9, 4, 0 ],
        [ 8, 0, 0, 0, 4, 6, 1, 0, 0 ],
        [ 0, 0, 3, 0, 0, 0, 0, 0, 0 ]
    ]);

    public static readonly SudokuBoard SOLVED_INVALID_BOX = new(
    [
        [ 5, 3, 4, 9, 2, 1, 7, 6, 8 ],
        [ 1, 6, 2, 4, 8, 7, 3, 5, 9 ],
        [ 9, 8, 7, 6, 3, 5, 2, 1, 4 ],
        [ 3, 5, 8, 7, 6, 2, 4, 9, 1 ],
        [ 4, 9, 6, 8, 1, 3, 5, 7, 2 ],
        [ 7, 2, 1, 5, 9, 4, 8, 3, 6 ],
        [ 6, 1, 8, 2, 7, 8, 9, 4, 3 ],
        [ 8, 7, 9, 3, 4, 6, 1, 2, 5 ],
        [ 2, 4, 3, 1, 5, 9, 6, 8, 7 ]
    ]);

    public static readonly SudokuBoard SOLVED_INVALID_ROW = new(
    [
        [ 5, 3, 4, 9, 2, 1, 7, 6, 5 ],
        [ 1, 6, 2, 4, 8, 7, 3, 5, 9 ],
        [ 9, 8, 7, 6, 3, 5, 2, 1, 4 ],
        [ 3, 5, 8, 7, 6, 2, 4, 9, 1 ],
        [ 4, 9, 6, 8, 1, 3, 5, 7, 2 ],
        [ 7, 2, 1, 5, 9, 4, 8, 3, 6 ],
        [ 6, 1, 5, 2, 7, 8, 9, 4, 3 ],
        [ 8, 7, 9, 3, 4, 6, 1, 2, 5 ],
        [ 2, 4, 3, 1, 5, 9, 6, 8, 7 ]
    ]);

    public static readonly SudokuBoard UNSOLVED_INVALID_ROW = new(
    [
        [ 5, 3, 4, 9, 2, 0, 7, 0, 0 ],
        [ 0, 6, 0, 0, 0, 7, 3, 0, 9 ],
        [ 9, 0, 0, 0, 0, 0, 0, 1, 0 ],
        [ 0, 0, 8, 7, 0, 0, 0, 0, 0 ],
        [ 4, 9, 6, 8, 0, 3, 0, 8, 2 ],
        [ 7, 2, 1, 5, 9, 4, 8, 0, 6 ],
        [ 0, 0, 0, 2, 0, 0, 9, 4, 0 ],
        [ 8, 0, 0, 0, 4, 6, 1, 0, 0 ],
        [ 0, 0, 3, 0, 0, 0, 0, 0, 0 ]
    ]);

    public static readonly SudokuBoard SOLVED_INVALID_COLUMN = new(
    [
        [ 5, 3, 4, 9, 2, 1, 7, 6, 5 ],
        [ 1, 6, 2, 4, 8, 7, 3, 5, 9 ],
        [ 9, 8, 7, 6, 3, 5, 2, 1, 4 ],
        [ 3, 5, 8, 7, 6, 2, 4, 9, 1 ],
        [ 4, 9, 6, 8, 1, 3, 5, 7, 2 ],
        [ 7, 2, 1, 5, 9, 4, 8, 3, 6 ],
        [ 6, 1, 5, 2, 7, 8, 9, 4, 3 ],
        [ 8, 7, 9, 3, 4, 6, 1, 2, 5 ],
        [ 2, 4, 3, 1, 5, 9, 6, 8, 7 ]
    ]);

    public static readonly SudokuBoard UNSOLVED_INVALID_COLUMN = new(
    [
        [ 5, 3, 4, 9, 2, 0, 7, 0, 0 ],
        [ 0, 6, 0, 0, 0, 7, 3, 0, 9 ],
        [ 9, 0, 0, 0, 0, 0, 0, 1, 0 ],
        [ 0, 0, 8, 7, 0, 0, 0, 0, 0 ],
        [ 4, 9, 6, 8, 0, 3, 0, 1, 2 ],
        [ 7, 2, 1, 5, 9, 4, 8, 0, 6 ],
        [ 0, 0, 0, 2, 0, 0, 9, 4, 0 ],
        [ 8, 0, 0, 0, 4, 6, 1, 0, 0 ],
        [ 0, 0, 3, 0, 0, 0, 0, 0, 0 ]
    ]);
}
