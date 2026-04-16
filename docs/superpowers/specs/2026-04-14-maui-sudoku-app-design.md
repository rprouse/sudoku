# MAUI Sudoku App — Design Spec

## Overview

A .NET MAUI Sudoku game targeting Android and Windows. Puzzles are generated on-demand using the existing `SudokuGenerator` from `Sudoku.Core`. The app features a dark/light theme, cell highlighting, candidate mode, auto-candidate computation, undo, error checking, timer, and save/resume.

## Target Platforms

- Android (`net8.0-android`)
- Windows (`net8.0-windows10.0.19041.0`)

## Project Structure

New project **Sudoku.App** referencing **Sudoku.Core**. MVVM architecture.

```
Sudoku.App/
  App.xaml / App.xaml.cs          — Theme management, navigation setup
  Models/
    GameState.cs                  — Puzzle, solution, cell states, undo stack, timer, errors
    CellState.cs                  — Per-cell: value, candidates, excludedCandidates, isGiven, isError
    GameSettings.cs               — Theme, highlight options, show errors, auto-remove candidates
    UndoAction.cs                 — Record: row, col, previousValue, previousCandidates
  ViewModels/
    GameViewModel.cs              — Game logic, input, timer, undo, error checking, redraw triggers
    MenuViewModel.cs              — Difficulty selection, continue detection, puzzle generation
    SettingsViewModel.cs          — Binds to GameSettings, persists on change
  Views/
    MenuPage.xaml                 — Difficulty picker + Continue button
    GamePage.xaml                 — Board (GraphicsView) + number pad + controls
    SettingsPage.xaml              — Theme & gameplay toggles
  Controls/
    SudokuBoardDrawable.cs        — IDrawable: renders grid, numbers, highlights, candidates
  Services/
    GamePersistenceService.cs     — Save/load GameState to app storage (JSON)
    SettingsService.cs            — Save/load settings via MAUI Preferences
```

## Screens

### MenuPage

- App title ("SUDOKU") at top.
- **Continue button** — visible only when a saved game exists. Shows difficulty and elapsed time. Styled in blue (`#2970c2`).
- **New Game section** — five buttons for Easy, Medium, Hard, Expert, Evil. Each color-coded (green → yellow → orange → red → purple). Tapping one generates a puzzle on a background thread with a loading spinner, then navigates to GamePage.
- Settings link at the bottom.

### GamePage

Top-to-bottom layout:

1. **Navigation bar** — Back button (returns to menu), Settings gear icon.
2. **Info bar** — Difficulty label, timer (mm:ss format, no pause icon), error count.
3. **Sudoku grid** — `GraphicsView` with `SudokuBoardDrawable`. Square aspect ratio, centered.
4. **Mode toggle** — Normal/Candidate segmented control + Undo button.
5. **Number pad** — 5-column, 2-row grid of buttons (1–9 + X to clear).
6. **Auto Candidate checkbox** — below the number pad.

### SettingsPage

- Back arrow returns to MenuPage.
- **Appearance section** — Dark/Light theme segmented toggle.
- **Gameplay section** — four toggle switches:
  - Highlight Related Cells (default: on) — shade same row/col/box.
  - Highlight Same Numbers (default: on) — tint cells with matching value.
  - Show Errors (default: on) — highlight incorrect values immediately.
  - Auto-Remove Candidates (default: off) — when placing a number, remove that candidate from related cells.
- Settings persist via MAUI `Preferences` and apply immediately.

## Grid Rendering (SudokuBoardDrawable)

Implements `IDrawable`. Renders onto a `GraphicsView`.

### Lines

- Thin lines (`1px`, `#444`) between cells.
- Thick lines (`2px`, `#888`) at 3×3 box boundaries.
- Outer border (`2px`, `#888`).

### Cell Backgrounds (Dark Theme)

| State | Color | Hex |
|-------|-------|-----|
| Default (empty) | Near-black | `#1a1a1a` |
| Selected cell | Bright blue | `#2970c2` |
| Related (row/col/box) | Dim blue | `#1a2640` |
| Same number | Soft green | `#1e3520` |
| Error | Red | `#cc4444` |

Light theme colors will be derived counterparts (lighter backgrounds, same hue families).

### Priority order when multiple states overlap:
1. Error (highest — always visible)
2. Selected
3. Same number
4. Related
5. Default (lowest)

### Text

- **Given clues** — white, bold.
- **Player entries** — slightly lighter blue-white (`#b0c4de`), distinguishable from givens.
- **Candidates** — small digits arranged in a 3×3 mini-grid within the cell. Digit positions: 1 top-left, 2 top-center, ..., 9 bottom-right.

### Touch Handling

- `GraphicsView.StartInteraction` provides the touch point.
- Divide coordinates by cell size to determine tapped row/col.
- Update `GameViewModel.SelectedRow`/`SelectedCol`, call `GraphicsView.Invalidate()`.

## Game Logic

### Input Modes

- **Normal mode** — number buttons set the cell's value (replacing any existing value). Only works on non-given cells.
- **Candidate mode** — number buttons toggle individual candidates in the cell's 3×3 mini-grid. Only works on empty non-given cells.
- **X button** — in Normal mode, clears the cell value. In Candidate mode, clears all candidates for the cell.

### Error Checking

On every number placement in Normal mode, compare the entered value against the solution board stored in `GameState`. If it doesn't match:
- Mark the cell as error (`CellState.IsError = true`).
- Increment the error counter.
- The error highlight persists until the user changes or clears the value.

This only applies when "Show Errors" is enabled in settings. When disabled, no visual error feedback and no error counting.

### Undo

A stack of `UndoAction` records. Each action captures `(row, col, previousValue, previousCandidates[])`.

- Every value placement or candidate toggle pushes an action.
- Undo pops the stack and restores the cell to its previous state.
- No redo. Stack is saved with the game state.

### Auto Candidate Mode

When the Auto Candidate checkbox is toggled **on**:
- For every empty cell, compute valid candidates: values 1–9 not present in the same row, column, or 3×3 box.
- Intersect with the cell's `ExcludedCandidates` set — any candidate the user previously manually removed stays removed.
- Populate `CellState.Candidates`.

When toggled **off**:
- Clear all candidate displays.
- `ExcludedCandidates` is preserved so re-enabling auto-candidate respects prior manual removals.

### Auto-Remove Candidates (Settings Option)

When enabled and a number is placed in Normal mode:
- Remove that number from the candidate arrays of all cells in the same row, column, and 3×3 box.
- These removals are also recorded in `ExcludedCandidates` so they persist across auto-candidate toggles.

### Timer

- `System.Timers.Timer` ticking every second, updating `GameState.ElapsedSeconds`.
- Displayed as `m:ss` format in the info bar.
- Auto-pauses when the app is backgrounded (MAUI `OnSleep` lifecycle event).
- Auto-resumes when the app is foregrounded (`OnResume`).
- No manual pause button.

### Win Detection

After each valid placement in Normal mode:
- Check if all 81 cells are filled (no zeros).
- Check if all values match the solution.
- If both true, stop the timer and show a congratulations dialog with final time and error count.

### Save / Resume

`GamePersistenceService` serializes the full `GameState` to JSON in `FileSystem.AppDataDirectory`.

**Auto-saves on:**
- Every move (value placement, candidate toggle, undo).
- App backgrounding (`OnSleep`).

**Resume:**
- On app launch, `MenuViewModel` checks for a saved game file.
- If found, the Continue button appears showing the difficulty and elapsed time.
- Tapping Continue deserializes the state and navigates to GamePage.
- Starting a new game deletes the existing save.

## Puzzle Generation

Uses `SudokuGenerator` from `Sudoku.Core` with `SimpleSolver`.

- Generation runs on a background thread (`Task.Run`).
- MenuPage shows a loading spinner while generating.
- The generator returns a `SudokuBoard` (the puzzle). To get the solution, solve it with `SimpleSolver`.
- Both puzzle and solution are stored in `GameState`.

## Theme Support

- Dark and Light themes, user-selectable in Settings.
- Theme choice persisted in `Preferences`.
- `App.xaml` defines both `ResourceDictionary` sets (colors, styles).
- `App.xaml.cs` applies the selected theme on startup and on change.
- Default theme: Dark.

## Dependencies

- **Sudoku.Core** — `SudokuBoard`, `SudokuGenerator`, `SimpleSolver`, `Difficulty` enum.
- **CommunityToolkit.Mvvm** — `ObservableObject`, `RelayCommand`, `ObservableProperty` for clean MVVM without boilerplate.
- **No other third-party packages** — MAUI provides `GraphicsView`, `Preferences`, navigation, and lifecycle management.
