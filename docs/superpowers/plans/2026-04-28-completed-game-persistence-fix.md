# Completed Game Persistence Fix Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix the bug where the **Continue** button reappears for a completed game after dismissing and reopening the Android app, by centralizing the "do not persist completed games" rule in `GamePersistenceService.SaveAsync`.

**Architecture:** Move `GamePersistenceService` from `Sudoku.App` to `Sudoku.Core/Game/` so it's reachable from `Sudoku.Tests` without taking a MAUI dependency. Make `SaveAsync` reject completed states (and self-heal any pre-existing save file), then remove now-redundant caller-side guards. Add unit tests covering the in-progress, completed-no-save, and completed-self-heal paths.

**Tech Stack:** C# 12 / .NET 10, MAUI 10, NUnit 4, FluentAssertions 6, CommunityToolkit.Mvvm 8.

**Spec reference:** `docs/superpowers/specs/2026-04-28-completed-game-persistence-fix-design.md`

---

## File Map

| File | Action | Responsibility |
|---|---|---|
| `Sudoku.App/Services/GamePersistenceService.cs` | **Delete** | (moved to Core) |
| `Sudoku.Core/Game/GamePersistenceService.cs` | **Create** | Save/load/delete game state JSON; enforces "no completed-game persistence" invariant |
| `Sudoku.App/MauiProgram.cs` | Modify (line 28) | Register service via factory lambda supplying `FileSystem.AppDataDirectory` |
| `Sudoku.App/App.xaml.cs` | Modify (using directives) | Switch namespace import for `GamePersistenceService` |
| `Sudoku.App/ViewModels/GameViewModel.cs` | Modify (using directives + line 197) | Switch namespace import; drop redundant `!IsGameComplete` guard |
| `Sudoku.App/ViewModels/MenuViewModel.cs` | Modify (using directives) | Switch namespace import |
| `Sudoku.Tests/App/GamePersistenceServiceTests.cs` | **Create** | Three regression tests: in-progress round-trip, completed no-save, completed self-heal |
| `CLAUDE.md` | Modify | Replace **Save-state lifecycle** gotcha with new persistence-invariant one-liner |

---

## Task 1: Move `GamePersistenceService` to `Sudoku.Core/Game/`

This task is a pure refactor — no behavior change. Build must succeed at the end. Any test failure that surfaces here is unrelated to the bug fix.

**Files:**
- Delete: `Sudoku.App/Services/GamePersistenceService.cs`
- Create: `Sudoku.Core/Game/GamePersistenceService.cs`
- Modify: `Sudoku.App/MauiProgram.cs`
- Modify: `Sudoku.App/App.xaml.cs`
- Modify: `Sudoku.App/ViewModels/GameViewModel.cs`
- Modify: `Sudoku.App/ViewModels/MenuViewModel.cs`

- [ ] **Step 1.1: Create the new file at `Sudoku.Core/Game/GamePersistenceService.cs`**

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sudoku.Core.Game;

public class GamePersistenceService
{
    private static readonly JsonSerializerOptions s_options = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new CellStateArrayConverter(), new UndoStackConverter() }
    };

    private readonly string _saveDirectory;

    public GamePersistenceService(string saveDirectory)
    {
        _saveDirectory = saveDirectory;
    }

    private string SavePath => Path.Combine(_saveDirectory, "game_state.json");

    public async Task SaveAsync(GameState state)
    {
        var json = JsonSerializer.Serialize(state, s_options);
        await File.WriteAllTextAsync(SavePath, json);
    }

    public async Task<GameState?> LoadAsync()
    {
        if (!File.Exists(SavePath))
            return null;

        var json = await File.ReadAllTextAsync(SavePath);
        return JsonSerializer.Deserialize<GameState>(json, s_options);
    }

    public void Delete()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }

    public bool HasSavedGame() => File.Exists(SavePath);
}

public class CellStateArrayConverter : JsonConverter<CellState[,]>
{
    public override CellState[,] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var flat = JsonSerializer.Deserialize<CellState[]>(ref reader, options) ?? Array.Empty<CellState>();
        var cells = new CellState[9, 9];
        for (var i = 0; i < flat.Length && i < 81; i++)
            cells[i / 9, i % 9] = flat[i];
        return cells;
    }

    public override void Write(Utf8JsonWriter writer, CellState[,] value, JsonSerializerOptions options)
    {
        var flat = new CellState[81];
        for (var r = 0; r < 9; r++)
            for (var c = 0; c < 9; c++)
                flat[r * 9 + c] = value[r, c];
        JsonSerializer.Serialize(writer, flat, options);
    }
}

public class UndoStackConverter : JsonConverter<Stack<UndoAction>>
{
    public override Stack<UndoAction> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var list = JsonSerializer.Deserialize<List<UndoAction>>(ref reader, options) ?? new List<UndoAction>();
        list.Reverse();
        return new Stack<UndoAction>(list);
    }

    public override void Write(Utf8JsonWriter writer, Stack<UndoAction> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.ToArray(), options);
    }
}
```

Note the differences from the original: namespace is `Sudoku.Core.Game`, the static `s_options` and `SavePath` are gone (`SavePath` is now an instance member), the constructor takes a required `string saveDirectory`, and the previous `using Sudoku.Core.Game;` directive is unnecessary because we're inside that namespace now. Behavior of `SaveAsync`/`LoadAsync`/`Delete`/`HasSavedGame` is unchanged — the `IsComplete()` guard is added in Task 3.

- [ ] **Step 1.2: Delete the old file**

```bash
rm Sudoku.App/Services/GamePersistenceService.cs
```

- [ ] **Step 1.3: Update `Sudoku.App/MauiProgram.cs:28`**

Replace this line:

```csharp
        builder.Services.AddSingleton<GamePersistenceService>();
```

With:

```csharp
        builder.Services.AddSingleton(_ => new GamePersistenceService(FileSystem.AppDataDirectory));
```

Then update the `using` block at the top of the file. Remove `using Sudoku.App.Services;` (no longer needed) and add `using Sudoku.Core.Game;` if not already present. The final using block in `MauiProgram.cs` should be:

```csharp
using Sudoku.App.Services;     // still needed for SettingsService
using Sudoku.App.ViewModels;
using Sudoku.App.Views;
using Sudoku.Core.Game;        // NEW — for GamePersistenceService
using Sudoku.Core.Generators;
using Sudoku.Core.Solvers;
```

(Keep `Sudoku.App.Services` because `SettingsService` is still in that namespace.)

- [ ] **Step 1.4: Update `Sudoku.App/App.xaml.cs` using directives**

Add `using Sudoku.Core.Game;` to the top of the file (before `using Sudoku.App.Services;`). The first three lines should become:

```csharp
using Sudoku.App.Services;
using Sudoku.App.ViewModels;
using Sudoku.Core.Game;
```

(Order alphabetical — keep `Sudoku.App.*` together, then `Sudoku.Core.*`.)

- [ ] **Step 1.5: Update `Sudoku.App/ViewModels/GameViewModel.cs` using directives**

The file already imports `Sudoku.Core.Game` (line 7). The reference to `GamePersistenceService` will now resolve to `Sudoku.Core.Game.GamePersistenceService` automatically. No change needed unless the compiler reports an ambiguity; if so, remove `using Sudoku.App.Services;` from line 5.

Verify by inspecting the using block:

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Sudoku.App.Controls;
using Sudoku.App.Services;     // still needed for SettingsService
using Sudoku.Core;
using Sudoku.Core.Game;        // GamePersistenceService now resolves here
```

- [ ] **Step 1.6: Update `Sudoku.App/ViewModels/MenuViewModel.cs` using directives**

Same situation as `GameViewModel.cs`. The file already imports `Sudoku.Core.Game`. No change unless the compiler reports an ambiguity. Verify the using block looks like:

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Sudoku.App.Services;     // not strictly needed here anymore — keep only if other types from it are referenced
using Sudoku.Core;
using Sudoku.Core.Game;
using Sudoku.Core.Generators;
using Sudoku.Core.Solvers;
```

If `Sudoku.App.Services` has no remaining references in `MenuViewModel.cs` (it doesn't — only `GamePersistenceService` came from there), remove that using line.

- [ ] **Step 1.7: Build the solution**

Run: `dotnet build`
Expected: Build succeeds with 0 errors, 0 new warnings.

If errors appear, common causes:
- Forgot to update one of the `using` directives.
- Forgot to update `MauiProgram.cs` registration.
- Whitespace difference in the moved file.

- [ ] **Step 1.8: Run all tests to confirm no regression**

Run: `dotnet test Sudoku.Tests`
Expected: All existing tests pass.

- [ ] **Step 1.9: Commit**

```bash
git add Sudoku.Core/Game/GamePersistenceService.cs \
        Sudoku.App/Services/GamePersistenceService.cs \
        Sudoku.App/MauiProgram.cs \
        Sudoku.App/App.xaml.cs \
        Sudoku.App/ViewModels/GameViewModel.cs \
        Sudoku.App/ViewModels/MenuViewModel.cs
git commit -m "refactor: move GamePersistenceService to Sudoku.Core for testability"
```

---

## Task 2: Add the in-progress save round-trip test

This test exercises the new constructor wiring and proves the moved service still behaves the same for in-progress games. It should pass on first run because we haven't changed save behavior yet.

**Files:**
- Create: `Sudoku.Tests/App/GamePersistenceServiceTests.cs`

- [ ] **Step 2.1: Create the test file with the first test**

```csharp
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
```

- [ ] **Step 2.2: Run the new test**

Run: `dotnet test Sudoku.Tests --filter "FullyQualifiedName~GamePersistenceServiceTests"`
Expected: 1 test passes (`SaveAsync_PersistsState_WhenGameInProgress`).

- [ ] **Step 2.3: Commit**

```bash
git add Sudoku.Tests/App/GamePersistenceServiceTests.cs
git commit -m "test: add round-trip save test for GamePersistenceService"
```

---

## Task 3: Test-drive the completed-game guard

Write a failing test for "completed game does not persist," then add the `IsComplete()` early-return to make it pass. Classic red→green TDD.

**Files:**
- Modify: `Sudoku.Tests/App/GamePersistenceServiceTests.cs`
- Modify: `Sudoku.Core/Game/GamePersistenceService.cs`

- [ ] **Step 3.1: Add the failing test**

Append a new helper method and test to `GamePersistenceServiceTests.cs`. Add the helper alongside `MakeInProgressState`:

```csharp
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
```

Then add the test method:

```csharp
    [Test]
    public async Task SaveAsync_DoesNotPersist_WhenGameIsComplete()
    {
        var state = MakeCompletedState();

        await _service.SaveAsync(state);

        _service.HasSavedGame().Should().BeFalse();
    }
```

- [ ] **Step 3.2: Run the new test and confirm it fails**

Run: `dotnet test Sudoku.Tests --filter "FullyQualifiedName~GamePersistenceServiceTests.SaveAsync_DoesNotPersist_WhenGameIsComplete"`
Expected: FAIL — `HasSavedGame` returns `true` because `SaveAsync` currently writes regardless of completion.

- [ ] **Step 3.3: Add the `IsComplete()` early-return to `SaveAsync`**

In `Sudoku.Core/Game/GamePersistenceService.cs`, replace the body of `SaveAsync`:

```csharp
    public async Task SaveAsync(GameState state)
    {
        if (state.IsComplete())
            return;

        var json = JsonSerializer.Serialize(state, s_options);
        await File.WriteAllTextAsync(SavePath, json);
    }
```

(The `Delete()` self-heal call is intentionally omitted — Task 4 adds it.)

- [ ] **Step 3.4: Run the test and confirm it passes**

Run: `dotnet test Sudoku.Tests --filter "FullyQualifiedName~GamePersistenceServiceTests.SaveAsync_DoesNotPersist_WhenGameIsComplete"`
Expected: PASS.

- [ ] **Step 3.5: Run all tests in the fixture to confirm no regression**

Run: `dotnet test Sudoku.Tests --filter "FullyQualifiedName~GamePersistenceServiceTests"`
Expected: Both tests pass.

- [ ] **Step 3.6: Commit**

```bash
git add Sudoku.Tests/App/GamePersistenceServiceTests.cs Sudoku.Core/Game/GamePersistenceService.cs
git commit -m "fix: refuse to persist completed games in SaveAsync"
```

---

## Task 4: Test-drive the self-heal path

Write a failing test for "completed save with existing file deletes it," then add the `Delete()` call inside the guard.

**Files:**
- Modify: `Sudoku.Tests/App/GamePersistenceServiceTests.cs`
- Modify: `Sudoku.Core/Game/GamePersistenceService.cs`

- [ ] **Step 4.1: Add the failing self-heal test**

Append to `GamePersistenceServiceTests.cs`:

```csharp
    [Test]
    public async Task SaveAsync_DeletesExistingFile_WhenGameIsComplete()
    {
        // Pre-write a stale save (simulates a save file written by a buggy older build).
        await _service.SaveAsync(MakeInProgressState());
        _service.HasSavedGame().Should().BeTrue("precondition: a save file must exist before the completed save call");

        await _service.SaveAsync(MakeCompletedState());

        _service.HasSavedGame().Should().BeFalse("completed save must remove any existing save file");
    }
```

- [ ] **Step 4.2: Run the test and confirm it fails**

Run: `dotnet test Sudoku.Tests --filter "FullyQualifiedName~GamePersistenceServiceTests.SaveAsync_DeletesExistingFile_WhenGameIsComplete"`
Expected: FAIL — `HasSavedGame` returns `true` because the early-return doesn't delete the existing file.

- [ ] **Step 4.3: Add the `Delete()` call to `SaveAsync`**

In `Sudoku.Core/Game/GamePersistenceService.cs`, update `SaveAsync`:

```csharp
    public async Task SaveAsync(GameState state)
    {
        if (state.IsComplete())
        {
            Delete();
            return;
        }

        var json = JsonSerializer.Serialize(state, s_options);
        await File.WriteAllTextAsync(SavePath, json);
    }
```

- [ ] **Step 4.4: Run the test and confirm it passes**

Run: `dotnet test Sudoku.Tests --filter "FullyQualifiedName~GamePersistenceServiceTests.SaveAsync_DeletesExistingFile_WhenGameIsComplete"`
Expected: PASS.

- [ ] **Step 4.5: Run all fixture tests**

Run: `dotnet test Sudoku.Tests --filter "FullyQualifiedName~GamePersistenceServiceTests"`
Expected: All three tests pass.

- [ ] **Step 4.6: Commit**

```bash
git add Sudoku.Tests/App/GamePersistenceServiceTests.cs Sudoku.Core/Game/GamePersistenceService.cs
git commit -m "fix: self-heal stale save files when persisting completed game"
```

---

## Task 5: Remove redundant `!IsGameComplete` guard in `GameViewModel.SaveGameAsync`

The persistence service now enforces the rule. Leaving the caller-side guard would mislead future readers into thinking the rule still requires caller discipline.

**Files:**
- Modify: `Sudoku.App/ViewModels/GameViewModel.cs:197`

- [ ] **Step 5.1: Edit `SaveGameAsync`**

Locate this line in `Sudoku.App/ViewModels/GameViewModel.cs` (around line 197, inside `private async Task SaveGameAsync()`):

```csharp
        if (State != null && !IsGameComplete) await _persistenceService.SaveAsync(State);
```

Replace with:

```csharp
        if (State != null) await _persistenceService.SaveAsync(State);
```

- [ ] **Step 5.2: Build and run all tests**

Run: `dotnet build && dotnet test Sudoku.Tests`
Expected: Build succeeds, all tests pass.

- [ ] **Step 5.3: Commit**

```bash
git add Sudoku.App/ViewModels/GameViewModel.cs
git commit -m "refactor: drop redundant !IsGameComplete guard in GameViewModel.SaveGameAsync"
```

---

## Task 6: Update `CLAUDE.md`

Replace the **Save-state lifecycle** gotcha. The condition it warned about no longer applies because the rule is structurally enforced.

**Files:**
- Modify: `CLAUDE.md`

- [ ] **Step 6.1: Edit the gotcha bullet**

In `CLAUDE.md`, find this bullet under the **Gotchas** heading:

```markdown
- **Save-state lifecycle**: `SaveGameAsync` runs after every `NumberInput`. Any code that deletes the save file during input processing must also guard `SaveGameAsync` from re-creating it (check `IsGameComplete`).
```

Replace with:

```markdown
- **Persistence invariant**: `GamePersistenceService.SaveAsync` is a no-op (and clears any pre-existing save file) when called with a completed `GameState`. Callers do not need to check completion before saving — the rule lives in the service so it cannot be forgotten at any save site (game-input, app-suspend, etc.).
```

- [ ] **Step 6.2: Commit**

```bash
git add CLAUDE.md
git commit -m "docs: update CLAUDE.md gotcha for centralized persistence invariant"
```

---

## Task 7: Manual smoke test on Android

Unit tests cover the service-level invariant. The app-suspend lifecycle path that triggered the original bug report cannot be exercised in unit tests, so a manual Android run is the final check.

**Files:** none.

- [ ] **Step 7.1: Build and deploy to an Android device or emulator**

Run: `dotnet build Sudoku.App -f net10.0-android -t:Run`
Expected: App builds and launches.

- [ ] **Step 7.2: Reproduce the original bug scenario**

1. From the menu, start a new Easy puzzle.
2. Tap a cell, then tap any number — confirm the in-progress state is saving (no need to verify on disk; just play).
3. Solve the puzzle (use the auto-candidate mode if helpful). The win overlay should appear.
4. Tap **Continue** on the win overlay — you should land on the menu.
5. Confirm the **Continue Game** button is hidden on the menu.
6. **Dismiss the app** (swipe it away from Recents on Android — this is what fires `window.Stopped`).
7. Re-launch the app from the launcher.
8. Verify the menu shows **no Continue Game button**.

Expected: Continue button stays hidden after re-launch. (Pre-fix behavior: Continue button reappeared pointing at the completed board.)

- [ ] **Step 7.3: Sanity-check the in-progress path is unchanged**

1. From the menu, start another Easy puzzle.
2. Tap a cell, place one number.
3. Dismiss the app.
4. Re-launch.
5. Verify the **Continue Game** button is present and resumes the in-progress puzzle.

Expected: In-progress games still resume correctly.

- [ ] **Step 7.4: No commit needed**

This task is verification only.

---

## Self-Review Notes

Spec coverage:
- ✓ Section 1 (`SaveAsync` guard + self-heal): Tasks 3 and 4
- ✓ Section 2 (remove redundant guards): Task 5 (and Task 1 deliberately preserves `App.xaml.cs` unchanged per spec)
- ✓ Section 3 (move + constructor refactor + DI): Task 1
- ✓ Section 4 (regression tests): Tasks 2, 3, 4
- ✓ Section 5 (CLAUDE.md update): Task 6
- ✓ Implementation order matches spec §"Implementation order"
- ✓ Manual Android smoke test: Task 7

No placeholders. All code is shown in full at the point it is written.

Type/method consistency: `GameState.IsComplete()`, `GameState.PlaceNumber(int, int, int)`, `state.Cells[r, c].IsGiven`, `Difficulty.Easy`, `BitmaskSolver`, `SudokuGenerator(ISolver, DifficultyGrader, Random)`, `DifficultyGrader(ITechniqueSolver)`, `TechniqueSolver()` are all referenced consistently between Task 2 (`MakeInProgressState`) and Task 3 (`MakeCompletedState`).
