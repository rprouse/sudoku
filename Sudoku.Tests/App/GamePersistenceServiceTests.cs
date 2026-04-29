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
