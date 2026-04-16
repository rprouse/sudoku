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

- **Sudoku.Core** (net8.0) — Core library containing `SudokuBoard` data model, `Solvers` (`ISolver`, `SimpleSolver`), `Generators` (`SudokuGenerator`), `Game` namespace (`GameState`, `CellState`, `GameSettings`, `UndoAction`, `WinMessages`), `Difficulty` enum (root namespace), and `Persistence` (JSON deserialization).
- **Sudoku.App** (net10.0-android/windows) — .NET MAUI game app. MVVM with CommunityToolkit.Mvvm. GraphicsView-based board rendering. References Sudoku.Core. Structure: `Views/` (XAML pages), `ViewModels/` (observable ViewModels), `Services/` (`GamePersistenceService`, `SettingsService`), `Controls/` (`SudokuBoardDrawable`), `Converters/`.
- **Sudoku.Tests** (net8.0) — NUnit 4 test project using FluentAssertions. Test data files (`sudokus.json`, `sudokus.txt`) are copied to output on build.
- **Sudoku.Benchmarks** (net9.0) — BenchmarkDotNet performance benchmarks for solver implementations.

`SudokuBoard` stores the grid internally as `int[,]` (2D array). It accepts `int[][]` (jagged array) via constructor for JSON deserialization but converts immediately. `0` represents empty cells. Puzzles are organized by difficulty tier: Easy, Medium, Hard, Expert, Evil (100 each).

Game logic models (`GameState`, `CellState`, etc.) live in `Sudoku.Core/Game/` rather than the MAUI project so they remain testable from `Sudoku.Tests`.

## Code Style

Enforced via `.editorconfig`. Key conventions:
- Private fields: `_camelCase`; private static fields: `s_camelCase`
- Prefer `var` over explicit types
- Allman brace style (braces on new lines)
- File-scoped namespaces

## Gotchas

- **Candidate arrays**: Each `CellState` has three bool[9] arrays: `Candidates` (currently displayed), `ManualCandidates` (user-entered, preserved across mode switches), and `ExcludedCandidates` (auto-mode exclusions). `ToggleCandidate` and `ClearCandidates` require an `isAutoMode` parameter to write to the correct array.
- **SVG text centering**: SkiaSharp's SVG renderer (used by MAUI resizetizer) does not support `dominant-baseline`. Use manual y-offset (`+font_size * 0.35`) for vertical centering.
- **Resizetizer caching**: Icon/splash changes may not regenerate. Delete `obj/**/resizetizer/` and rebuild.
- **Android adaptive icons**: Keep foreground SVG content within ~72% of the canvas center to survive circular/shaped icon masks.
- **MAUI .NET 10 animation API**: `FadeTo`, `ScaleTo`, etc. are deprecated. Use `FadeToAsync`, `ScaleToAsync` instead.
- **Save-state lifecycle**: `SaveGameAsync` runs after every `NumberInput`. Any code that deletes the save file during input processing must also guard `SaveGameAsync` from re-creating it (check `IsGameComplete`).
