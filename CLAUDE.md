# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Test Commands

```bash
dotnet build              # Build the entire solution
dotnet test               # Run all tests
dotnet test --filter "FullyQualifiedName~CanLoadFromJson"  # Run a single test by name
dotnet test Sudoku.Tests  # Run tests in the test project only
dotnet run --project Sudoku.Benchmarks -c Release  # Run benchmarks (must use Release)
dotnet build Sudoku.App -f net10.0-windows10.0.19041.0  # Build MAUI app (Windows)
dotnet build Sudoku.App -f net10.0-android              # Build MAUI app (Android)
dotnet build Sudoku.App -f net10.0-windows10.0.19041.0 -t:Run  # Run MAUI app (Windows)
```

## Architecture

This is a C# Sudoku library and mobile game with nullable reference types enabled. The solution has four projects:

- **Sudoku.Core** (net8.0) — Core library containing `SudokuBoard` data model, `Solvers` (`ISolver`, `SimpleSolver`), `Generators` (`SudokuGenerator`), `Game` namespace (`GameState`, `CellState`, `GameSettings`, `UndoAction`, `RelatedCellChange`, `WinMessages`), `Difficulty` enum (root namespace), `DifficultyExtensions` (display name mapping), and `Persistence` (JSON deserialization).
- **Sudoku.App** (net10.0-android/windows) — .NET MAUI game app. MVVM with CommunityToolkit.Mvvm. GraphicsView-based board rendering. References Sudoku.Core. Structure: `Views/` (XAML pages), `ViewModels/` (observable ViewModels), `Services/` (`GamePersistenceService`, `SettingsService`), `Controls/` (`SudokuBoardDrawable`), `Converters/`.
- **Sudoku.Tests** (net8.0) — NUnit 4 test project using FluentAssertions. Test data files (`sudokus.json`, `sudokus.txt`) are copied to output on build.
- **Sudoku.Benchmarks** (net9.0) — BenchmarkDotNet performance benchmarks for solver implementations.

`SudokuBoard` stores the grid internally as `int[,]` (2D array). It accepts `int[][]` (jagged array) via constructor for JSON deserialization but converts immediately. `0` represents empty cells. Puzzles are organized by difficulty tier: Easy, Medium, Hard, Expert, Evil (100 each).

Game logic models (`GameState`, `CellState`, etc.) live in `Sudoku.Core/Game/` rather than the MAUI project so they remain testable from `Sudoku.Tests`.

## Design System

The app uses a zen-inspired warm earth-tone palette. Key design principles:

- **Warm tones throughout**: No pure greys. Backgrounds, text, and UI elements all carry warm amber/brown undertones.
- **Sage green accent**: Primary accent is `#7d8c6e` (light) / `#9aab88` (dark), replacing the original blue.
- **Soft UI**: Buttons use `CornerRadius="16"`, difficulty buttons have no borders, generous spacing between elements.
- **Difficulty display names**: The `Difficulty` enum retains its original values (`Easy`, `Medium`, `Hard`, `Expert`, `Evil`) for serialization compatibility. UI display names (`Gentle`, `Steady`, `Challenging`, `Deep`, `Profound`) are provided by `DifficultyExtensions.DisplayName()`. Always use `.DisplayName()` when showing difficulty to the user.
- **Difficulty colors**: Earth-tone gradient from sage (`#8a9a7b`) through sand, clay, terracotta to dusty mauve (`#8b6d7b`).

Color definitions live in:
- `Colors.xaml` — resource dictionary colors (backgrounds, surfaces)
- `SudokuBoardDrawable.cs` — board-specific colors as `static readonly Color` fields
- `BoolToModeConverters.cs` — mode toggle button colors
- XAML views — inline `AppThemeBinding` values for text and UI elements

## Code Style

Enforced via `.editorconfig`. Key conventions:
- Private fields: `_camelCase`; private static fields: `s_camelCase`
- Prefer `var` over explicit types
- Allman brace style (braces on new lines)
- File-scoped namespaces

## Gotchas

- **Difficulty naming**: The enum values (`Easy`, `Medium`, etc.) must not be renamed — they are used in JSON serialization for saved games and the puzzle data file. Use `DifficultyExtensions.DisplayName()` for UI text. XAML `CommandParameter` values still use the enum names (e.g., `CommandParameter="Easy"`).
- **Candidate arrays**: Each `CellState` has three bool[9] arrays: `Candidates` (currently displayed), `ManualCandidates` (user-entered, preserved across mode switches), and `ExcludedCandidates` (tracks removed candidates to prevent re-adding in auto mode). `ToggleCandidate` and `ClearCandidates` require an `isAutoMode` parameter to write to the correct array. Placing a number always removes that number as a candidate from related cells (row, column, box).
- **SVG text centering**: SkiaSharp's SVG renderer (used by MAUI resizetizer) does not support `dominant-baseline`. Use manual y-offset (`+font_size * 0.35`) for vertical centering.
- **Resizetizer caching**: Icon/splash changes may not regenerate. Delete `obj/**/resizetizer/` and rebuild.
- **Android adaptive icons**: Keep foreground SVG content within ~72% of the canvas center to survive circular/shaped icon masks.
- **MAUI .NET 10 animation API**: `FadeTo`, `ScaleTo`, etc. are deprecated. Use `FadeToAsync`, `ScaleToAsync` instead.
- **Undo restores related candidates**: `PlaceNumber` snapshots candidate state of affected row/col/box cells into `RelatedCellChange` entries on the `UndoAction` before removing candidates. `Undo()` restores all of them. Any new code that modifies candidates during placement must snapshot before mutating.
- **Save-state lifecycle**: `SaveGameAsync` runs after every `NumberInput`. Any code that deletes the save file during input processing must also guard `SaveGameAsync` from re-creating it (check `IsGameComplete`).
