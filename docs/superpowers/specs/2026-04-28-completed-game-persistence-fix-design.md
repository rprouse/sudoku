# Completed Game Persistence Fix — Design Spec

**Date**: 2026-04-28
**Status**: Draft — pending user review
**Scope**: `Sudoku.App` (MAUI), `Sudoku.Tests`, `CLAUDE.md`

## Problem

After completing a Sudoku puzzle on Android, the saved-game file is correctly deleted and the **Continue** button on the menu disappears as expected. However, if the user dismisses the app and re-launches it, the **Continue** button reappears, pointing at the already-completed board.

### Root cause

`GameViewModel` is registered as a singleton in `MauiProgram.cs`. When a game completes, `GameViewModel.NumberInput` deletes the on-disk save file but the singleton's `State` reference still points at the completed `GameState`. When the app is later suspended, `App.xaml.cs`'s `window.Stopped` handler runs:

```csharp
window.Stopped += (_, _) =>
{
    _gameViewModel.StopTimer();
    if (_gameViewModel.State != null)
        _ = _persistenceService.SaveAsync(_gameViewModel.State);
};
```

This handler's only guard is `State != null`. It does *not* check `IsGameComplete`, so it serializes the completed state back to disk, recreating the file we just deleted. On next launch, `MenuViewModel.CheckForSavedGameAsync` finds the file and re-shows the Continue button.

The bug is platform-asymmetric in practice: on Windows, `Stopped` fires less often between completion and seeing the menu, so the bug rarely manifests. On Android the `Stopped` handler is the normal path on app dismiss, so the bug is reliable.

`CLAUDE.md` already documents a related pitfall under the **Save-state lifecycle** gotcha:
> *"Any code that deletes the save file during input processing must also guard `SaveGameAsync` from re-creating it (check `IsGameComplete`)."*

That guard exists in `GameViewModel.SaveGameAsync` (line 197) but was never added to the `App.xaml.cs` save site. The fix could mirror the guard, but doing so propagates the same convention-based pattern that allowed this bug to ship in the first place.

## Goals

- The Continue button never reappears for a completed game, regardless of app lifecycle.
- The "we don't persist completed games" rule is enforced *structurally* by the persistence layer, not by convention at each caller.
- A unit test prevents regression of exactly this class of bug.
- `CLAUDE.md`'s Save-state lifecycle gotcha is removed (or simplified to a one-liner) because the rule no longer requires caller discipline.

## Non-goals

- No change to save format or the on-disk JSON schema.
- No change to game-completion UX (win overlay behavior, navigation, etc.).
- No change to `LoadAsync` or `Delete` semantics.
- No broader refactor of MAUI app-lifecycle handling beyond what's needed to make persistence testable.

## Design

### 1. Centralize the invariant in `GamePersistenceService`

`SaveAsync` becomes the single enforcement point for the rule "completed games are not persisted." It also self-heals by deleting any pre-existing save file when handed a completed state — this matters for users upgrading from a buggy build whose disk already contains a poisoned save.

```csharp
public async Task SaveAsync(GameState state)
{
    if (state.IsComplete())
    {
        Delete();          // self-heal: clear any pre-existing stale save
        return;
    }
    var json = JsonSerializer.Serialize(state, s_options);
    await File.WriteAllTextAsync(SavePath, json);
}
```

`Delete()`, `LoadAsync`, and `HasSavedGame` are unchanged.

**Why convergent (`Delete()` inside the guard) over transitional:**
A transitional design — "delete on completion event, never re-save" — only works if every event fires exactly once and every save site remembers the rule. App lifecycles regularly produce duplicate or missing events, and we've already seen one bug from the convention being forgotten at one save site. A convergent design — *"any time you ask me to persist a finished game, I make sure no save exists"* — self-corrects from arbitrary bad disk states, including ones written by older app versions.

### 2. Remove now-redundant guards at call sites

With the invariant in `SaveAsync`, the convention-based guards at the two call sites become misleading: a future reader sees them and infers the rule still requires caller discipline. Remove them so the codebase teaches the correct mental model.

**`Sudoku.App/ViewModels/GameViewModel.cs:197`** (`SaveGameAsync`):

```csharp
// before
if (State != null && !IsGameComplete) await _persistenceService.SaveAsync(State);
// after
if (State != null) await _persistenceService.SaveAsync(State);
```

**`Sudoku.App/App.xaml.cs:25-30`** (`Stopped` handler) — left as-is structurally; no `IsGameComplete` check is added (the service handles it). This is the *only* caller that was actually missing a guard, and we deliberately do not add one — the whole point is that no caller should need to know about completion semantics.

### 3. Make `GamePersistenceService` testable

Two coupled changes are required to reach the service from `Sudoku.Tests`:

**3a. Move the file from `Sudoku.App/Services/GamePersistenceService.cs` to `Sudoku.Core/Game/GamePersistenceService.cs`.**
`Sudoku.Tests` only references `Sudoku.Core`, and `Sudoku.App` is a multi-TFM MAUI project (`net10.0-android`, `net10.0-windows10.0.19041.0`). Adding a project reference from tests to the app would force a MAUI workload dependency on the test project. Moving the service to `Sudoku.Core/Game/` matches the architectural rule already documented in `CLAUDE.md`:
> *"Game logic models (`GameState`, `CellState`, etc.) live in `Sudoku.Core/Game/` rather than the MAUI project so they remain testable from `Sudoku.Tests`."*
The namespace becomes `Sudoku.Core.Game`. The two helper JSON converters (`CellStateArrayConverter`, `UndoStackConverter`) move with it — they reference `CellState` and `UndoAction` from `Sudoku.Core.Game` already.

**3b. Make the save directory a required constructor parameter** — no default. With the file in `Sudoku.Core`, the service no longer has access to `FileSystem.AppDataDirectory` (that lives in `Microsoft.Maui.Storage`), and we don't want `Sudoku.Core` to take a MAUI dependency. The MAUI app supplies the path at registration time.

```csharp
namespace Sudoku.Core.Game;

public class GamePersistenceService
{
    private readonly string _saveDirectory;

    public GamePersistenceService(string saveDirectory)
    {
        _saveDirectory = saveDirectory;
    }

    private string SavePath => Path.Combine(_saveDirectory, "game_state.json");
    // ... rest unchanged (with the IsComplete guard from §1 added)
}
```

**3c. Update `MauiProgram.cs` registration** to supply the directory via factory lambda:

```csharp
// before:
builder.Services.AddSingleton<GamePersistenceService>();
// after:
builder.Services.AddSingleton(_ => new GamePersistenceService(FileSystem.AppDataDirectory));
```

Tests construct with a per-test temp directory.

### 4. Add regression test

New file: `Sudoku.Tests/App/GamePersistenceServiceTests.cs`. Three test cases sized to the change:

| Test | Setup | Asserts |
|---|---|---|
| `SaveAsync_PersistsState_WhenGameInProgress` | Fresh in-progress `GameState` | After save, file exists; `LoadAsync` returns equivalent state |
| `SaveAsync_DoesNotPersist_WhenGameIsComplete` | `GameState` where every cell is filled correctly (i.e. `IsComplete()` returns true) | After save, file does not exist |
| `SaveAsync_DeletesExistingFile_WhenGameIsComplete` | Pre-write a save file, then call `SaveAsync` with a completed state | After save, file does not exist (self-healing path) |

Each test uses a unique temp directory (`Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())`) and cleans it up in `[TearDown]`.

### 5. Update `CLAUDE.md`

Delete the **Save-state lifecycle** gotcha bullet entirely. The condition it warned about ("any code that deletes the save file during input processing must also guard `SaveGameAsync` from re-creating it") is no longer true — the persistence service self-enforces. Leaving the gotcha would mislead future contributors into thinking caller-side guards are still required.

If we want to retain a breadcrumb, replace it with a one-liner documenting the new invariant:

> **Persistence invariant**: `GamePersistenceService.SaveAsync` is a no-op (and clears any pre-existing save file) when called with a completed `GameState`. Callers do not need to check completion before saving.

## Risks and mitigations

| Risk | Mitigation |
|---|---|
| The constructor change + file move breaks DI registration. | `MauiProgram.cs:28` is updated to register via factory lambda (`AddSingleton(_ => new GamePersistenceService(FileSystem.AppDataDirectory))`). The factory supplies the path; the service no longer depends on MAUI. |
| Moving the service to `Sudoku.Core` causes namespace breakage in `Sudoku.App` consumers. | Three call sites (`MenuViewModel`, `GameViewModel`, `App.xaml.cs`) gain a `using Sudoku.Core.Game;` (or remove `using Sudoku.App.Services;`). Mechanical change covered by compiler. |
| `state.IsComplete()` is mildly expensive (iterates all cells) and `SaveAsync` runs after every input. | `GameState.IsComplete()` is an `O(81)` row/col/box check. Saves are already async file writes — the cost is negligible compared to disk I/O. |
| Users who already have a poisoned save file from a buggy build still see Continue on first launch after the fix. | The self-healing `Delete()` inside the guard cleans this up the next time the lifecycle calls `SaveAsync` with a completed state. In the meantime, tapping Continue and finishing (or abandoning) the puzzle once will heal it on the next save. Acceptable. |

## Out-of-scope follow-ups

- A broader test suite for `GamePersistenceService` covering load/round-trip/corruption paths. The single regression test added here intentionally targets the bug at hand and the self-heal path; broader coverage is a separate effort.
- Auditing other singletons whose state may outlive their natural lifecycle (`SettingsService`, `Drawable`) for similar "stale state persisted on suspend" hazards. Out of scope for this fix.

## Implementation order

1. Move `GamePersistenceService` from `Sudoku.App/Services/` to `Sudoku.Core/Game/`, change namespace to `Sudoku.Core.Game`, change constructor to take a required `string saveDirectory`. Update three callers and `MauiProgram.cs` registration. (No behavior change yet; build should succeed.)
2. Add the first regression test (in-progress save round-trip) — proves the new constructor wiring works. Should pass immediately.
3. Add the failing test for "completed game does not persist," then add the `IsComplete()` early-return to `SaveAsync` to make it pass.
4. Add the failing test for the self-heal path ("completed save with existing file deletes it"), then add the `Delete()` call inside the guard to make it pass.
5. Remove the redundant `!IsGameComplete` guard in `GameViewModel.SaveGameAsync` (line 197).
6. Update `CLAUDE.md`: replace the **Save-state lifecycle** gotcha with the new persistence-invariant one-liner.
7. Manual smoke test on Android: complete a game, return to menu, dismiss app, reopen — verify Continue is gone.
