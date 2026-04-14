# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Test Commands

```bash
dotnet build              # Build the entire solution
dotnet test               # Run all tests
dotnet test --filter "FullyQualifiedName~CanLoadFromJson"  # Run a single test by name
dotnet test Sudoku.Tests  # Run tests in the test project only
dotnet run --project Sudoku.Benchmarks -c Release  # Run benchmarks (must use Release)
```

## Architecture

This is a C# Sudoku library with nullable reference types enabled. The solution has three projects:

- **Sudoku.Core** (net8.0) — Core library containing `Persistence` (JSON deserialization), data model `SudokuBoard`, and `Solvers` namespace (`ISolver` interface, `SimpleSolver`).
- **Sudoku.Tests** (net8.0) — NUnit 4 test project using FluentAssertions. Test data files (`sudokus.json`, `sudokus.txt`) are copied to output on build.
- **Sudoku.Benchmarks** (net9.0) — BenchmarkDotNet performance benchmarks for solver implementations.

`SudokuBoard` stores the grid internally as `int[,]` (2D array). It accepts `int[][]` (jagged array) via constructor for JSON deserialization but converts immediately. `0` represents empty cells. Puzzles are organized by difficulty tier: Easy, Medium, Hard, Expert, Evil (100 each).

## Code Style

Enforced via `.editorconfig`. Key conventions:
- Private fields: `_camelCase`; private static fields: `s_camelCase`
- Prefer `var` over explicit types
- Allman brace style (braces on new lines)
- File-scoped namespaces
