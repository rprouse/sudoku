using FluentAssertions;

using Sudoku.Core;
using Sudoku.Core.Game;
using Sudoku.Core.Generators;
using Sudoku.Core.Solvers;

namespace Sudoku.Tests.App;

[TestFixture]
public class GameStateTests
{
    private static (SudokuBoard puzzle, SudokuBoard solution) GenerateTestPuzzle()
    {
        var solver = new SimpleSolver();
        var generator = new SudokuGenerator(solver, new Random(42));
        var puzzle = generator.Generate(Difficulty.Easy);
        var solution = solver.Solve(new SudokuBoard(puzzle));
        return (puzzle, solution);
    }

    [Test]
    public void Initialize_SetsCellsFromPuzzle()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);

        for (var r = 0; r < 9; r++)
        {
            for (var c = 0; c < 9; c++)
            {
                state.Cells[r, c].Value.Should().Be(puzzle[r, c]);
                if (puzzle[r, c] != 0)
                    state.Cells[r, c].IsGiven.Should().BeTrue();
                else
                    state.Cells[r, c].IsGiven.Should().BeFalse();
            }
        }
    }

    [Test]
    public void Initialize_SetsMetadata()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);

        state.Difficulty.Should().Be(Difficulty.Easy);
        state.ElapsedSeconds.Should().Be(0);
        state.ErrorCount.Should().Be(0);
    }

    [Test]
    public void PlaceNumber_SetsValueOnEmptyCell()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);
        var (r, c) = FindEmptyCell(state);
        var correctValue = solution[r, c];

        state.PlaceNumber(r, c, correctValue);

        state.Cells[r, c].Value.Should().Be(correctValue);
    }

    [Test]
    public void PlaceNumber_DoesNotChangeGivenCell()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);
        var (r, c) = FindGivenCell(state);
        var original = state.Cells[r, c].Value;

        state.PlaceNumber(r, c, 1);

        state.Cells[r, c].Value.Should().Be(original);
    }

    [Test]
    public void PlaceNumber_WrongValue_MarksError()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);
        var settings = new GameSettings { ShowErrors = true };
        var (r, c) = FindEmptyCell(state);
        var wrongValue = solution[r, c] == 9 ? 1 : solution[r, c] + 1;

        state.PlaceNumber(r, c, wrongValue, settings);

        state.Cells[r, c].IsError.Should().BeTrue();
        state.ErrorCount.Should().Be(1);
    }

    [Test]
    public void PlaceNumber_WrongValue_ShowErrorsOff_DoesNotMarkError()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);
        var settings = new GameSettings { ShowErrors = false };
        var (r, c) = FindEmptyCell(state);
        var wrongValue = solution[r, c] == 9 ? 1 : solution[r, c] + 1;

        state.PlaceNumber(r, c, wrongValue, settings);

        state.Cells[r, c].IsError.Should().BeFalse();
        state.ErrorCount.Should().Be(0);
    }

    [Test]
    public void PlaceNumber_CorrectValue_NoError()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);
        var settings = new GameSettings { ShowErrors = true };
        var (r, c) = FindEmptyCell(state);

        state.PlaceNumber(r, c, solution[r, c], settings);

        state.Cells[r, c].IsError.Should().BeFalse();
    }

    [Test]
    public void PlaceNumber_PushesUndoAction()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);
        var (r, c) = FindEmptyCell(state);

        state.PlaceNumber(r, c, solution[r, c]);

        state.UndoStack.Should().HaveCount(1);
        state.UndoStack.Peek().Row.Should().Be(r);
        state.UndoStack.Peek().Col.Should().Be(c);
        state.UndoStack.Peek().PreviousValue.Should().Be(0);
    }

    [Test]
    public void Undo_RestoresPreviousValue()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);
        var (r, c) = FindEmptyCell(state);

        state.PlaceNumber(r, c, solution[r, c]);
        state.Undo();

        state.Cells[r, c].Value.Should().Be(0);
        state.UndoStack.Should().BeEmpty();
    }

    [Test]
    public void ClearCell_RemovesValueAndPushesUndo()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);
        var (r, c) = FindEmptyCell(state);

        state.PlaceNumber(r, c, solution[r, c]);
        state.ClearCell(r, c);

        state.Cells[r, c].Value.Should().Be(0);
        state.UndoStack.Should().HaveCount(2);
    }

    [Test]
    public void ClearCell_DoesNotClearGivenCell()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);
        var (r, c) = FindGivenCell(state);
        var original = state.Cells[r, c].Value;

        state.ClearCell(r, c);

        state.Cells[r, c].Value.Should().Be(original);
    }

    [Test]
    public void ToggleCandidate_OnEmptyCell_TogglesCandidateAndPushesUndo()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);
        var (r, c) = FindEmptyCell(state);

        state.ToggleCandidate(r, c, 5);

        state.Cells[r, c].HasCandidate(5).Should().BeTrue();
        state.UndoStack.Should().HaveCount(1);
    }

    [Test]
    public void ToggleCandidate_OnCellWithValue_DoesNothing()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);
        var (r, c) = FindGivenCell(state);

        state.ToggleCandidate(r, c, 5);

        state.UndoStack.Should().BeEmpty();
    }

    [Test]
    public void ClearCandidates_RemovesAllAndPushesUndo()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);
        var (r, c) = FindEmptyCell(state);
        state.ToggleCandidate(r, c, 3);
        state.ToggleCandidate(r, c, 7);

        state.ClearCandidates(r, c);

        state.Cells[r, c].HasCandidate(3).Should().BeFalse();
        state.Cells[r, c].HasCandidate(7).Should().BeFalse();
    }

    [Test]
    public void ComputeAutoCandidates_FillsValidCandidates()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);
        var (r, c) = FindEmptyCell(state);

        state.ComputeAutoCandidates();

        state.Cells[r, c].HasCandidate(solution[r, c]).Should().BeTrue();
    }

    [Test]
    public void ComputeAutoCandidates_RespectsExcludedCandidates()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);
        var (r, c) = FindEmptyCell(state);
        var correctValue = solution[r, c];

        state.Cells[r, c].ExcludedCandidates[correctValue - 1] = true;
        state.ComputeAutoCandidates();

        state.Cells[r, c].HasCandidate(correctValue).Should().BeFalse();
    }

    [Test]
    public void ClearAutoCandidates_RemovesCandidatesButKeepsExcluded()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);
        var (r, c) = FindEmptyCell(state);
        state.Cells[r, c].ExcludedCandidates[2] = true;

        state.ComputeAutoCandidates();
        state.ClearAutoCandidates();

        state.Cells[r, c].Candidates.Should().AllBeEquivalentTo(false);
        state.Cells[r, c].ExcludedCandidates[2].Should().BeTrue();
    }

    [Test]
    public void IsComplete_ReturnsFalse_WhenCellsEmpty()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);

        state.IsComplete().Should().BeFalse();
    }

    [Test]
    public void IsComplete_ReturnsTrue_WhenAllCorrect()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);

        for (var r = 0; r < 9; r++)
        {
            for (var c = 0; c < 9; c++)
            {
                if (!state.Cells[r, c].IsGiven)
                    state.PlaceNumber(r, c, solution[r, c]);
            }
        }

        state.IsComplete().Should().BeTrue();
    }

    [Test]
    public void AutoRemoveCandidates_RemovesFromRelatedCells()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);
        var settings = new GameSettings { AutoRemoveCandidates = true };

        state.ComputeAutoCandidates();

        var (r, c) = FindEmptyCell(state);
        var value = solution[r, c];

        state.PlaceNumber(r, c, value, settings);

        for (var col = 0; col < 9; col++)
        {
            if (col != c && !state.Cells[r, col].HasValue)
            {
                state.Cells[r, col].HasCandidate(value).Should().BeFalse();
            }
        }
    }

    private static (int r, int c) FindEmptyCell(GameState state)
    {
        for (var r = 0; r < 9; r++)
            for (var c = 0; c < 9; c++)
                if (!state.Cells[r, c].IsGiven && state.Cells[r, c].Value == 0)
                    return (r, c);
        throw new InvalidOperationException("No empty cell found");
    }

    private static (int r, int c) FindGivenCell(GameState state)
    {
        for (var r = 0; r < 9; r++)
            for (var c = 0; c < 9; c++)
                if (state.Cells[r, c].IsGiven)
                    return (r, c);
        throw new InvalidOperationException("No given cell found");
    }
}
