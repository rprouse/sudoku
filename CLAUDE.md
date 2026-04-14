# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Test Commands

```bash
dotnet build              # Build the entire solution
dotnet test               # Run all tests
dotnet test --filter "FullyQualifiedName~CanLoadFromJson"  # Run a single test by name
dotnet test Sudoku.Tests  # Run tests in the test project only
```

## Architecture

This is a C# (.NET 8.0) Sudoku library with nullable reference types enabled. The solution has two projects:

- **Sudoku.Core** — Core library. Currently contains `Persistence` (JSON deserialization of sudoku puzzle files) and the data models `SudokuBoard` and `SudokuJsonFile`.
- **Sudoku.Tests** — NUnit 4 test project using FluentAssertions. Test data files (`sudokus.json`, `sudokus.txt`) are copied to output on build.

The puzzle data model represents boards as `int[][]` (9 arrays of 9 ints), with `0` representing empty cells. Puzzles are organized by difficulty tier: Easy, Medium, Hard, Expert, Evil (100 each).

## Code Style

Enforced via `.editorconfig`. Key conventions:
- Private fields: `_camelCase`; private static fields: `s_camelCase`
- Explicit types preferred over `var`
- Allman brace style (braces on new lines)
- File-scoped namespaces
