using FluentAssertions;

using Sudoku.Core;
using Sudoku.Core.Game;
using Sudoku.Core.Generators;
using Sudoku.Core.Solvers;

namespace Sudoku.Tests.App;

[TestFixture]
public class GamePersistenceServiceTests
{
    private string _tempDir = null!;
    private GamePersistenceService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "sudoku-tests-" + Guid.NewGuid());
        Directory.CreateDirectory(_tempDir);
        _service = new GamePersistenceService(_tempDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private static GameState MakeInProgressState()
    {
        var solver = new BitmaskSolver();
        var generator = new SudokuGenerator(solver, new DifficultyGrader(new TechniqueSolver()), new Random(42));
        var puzzle = generator.Generate(Difficulty.Easy);
        var solution = solver.Solve(new SudokuBoard(puzzle));
        return new GameState(puzzle, solution, Difficulty.Easy);
    }

    private static GameState MakeCompletedState()
    {
        var solver = new BitmaskSolver();
        var generator = new SudokuGenerator(solver, new DifficultyGrader(new TechniqueSolver()), new Random(42));
        var puzzle = generator.Generate(Difficulty.Easy);
        var solution = solver.Solve(new SudokuBoard(puzzle));
        var state = new GameState(puzzle, solution, Difficulty.Easy);

        // Fill every empty cell with the solution value.
        for (var r = 0; r < 9; r++)
            for (var c = 0; c < 9; c++)
                if (!state.Cells[r, c].IsGiven)
                    state.PlaceNumber(r, c, solution[r, c]);

        state.IsComplete().Should().BeTrue("test setup must produce a fully-solved state");
        return state;
    }

    [Test]
    public async Task SaveAsync_DoesNotPersist_WhenGameIsComplete()
    {
        var state = MakeCompletedState();

        await _service.SaveAsync(state);

        _service.HasSavedGame().Should().BeFalse();
    }

    [Test]
    public async Task SaveAsync_PersistsState_WhenGameInProgress()
    {
        var state = MakeInProgressState();

        await _service.SaveAsync(state);

        _service.HasSavedGame().Should().BeTrue();
        var loaded = await _service.LoadAsync();
        loaded.Should().NotBeNull();
        loaded!.Difficulty.Should().Be(Difficulty.Easy);
    }
}
