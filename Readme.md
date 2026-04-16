# Sudoku

A Sudoku puzzle game built with C# and .NET MAUI, targeting Android and Windows.

## Projects

| Project | Framework | Description |
|---------|-----------|-------------|
| **Sudoku.Core** | net8.0 | Core library — board model, solver, puzzle generator, and game logic |
| **Sudoku.App** | net10.0-android/windows | .NET MAUI game app with MVVM architecture |
| **Sudoku.Tests** | net8.0 | NUnit 4 tests with FluentAssertions |
| **Sudoku.Benchmarks** | net9.0 | BenchmarkDotNet solver benchmarks |

## Features

- Five difficulty levels: Easy, Medium, Hard, Expert, Evil
- Manual and auto-candidate modes with independent tracking
- Undo support with full state restoration
- Error highlighting and related cell highlighting
- Game persistence (save/resume)
- Dark and light theme support

## Building

```bash
dotnet build                                                  # Build entire solution
dotnet build Sudoku.App -f net10.0-windows10.0.19041.0        # Windows
dotnet build Sudoku.App -f net10.0-android                    # Android
```

## Testing

```bash
dotnet test                                                   # Run all tests
dotnet test Sudoku.Tests                                      # Tests only
dotnet run --project Sudoku.Benchmarks -c Release             # Benchmarks
```

## Acknowledgements

Puzzle generation initially based on [How to generate sudokus](https://tn1ck.com/blog/how-to-generate-sudokus) by TN1ck and the accompanying [source code](https://github.com/TN1ck/super-sudoku).

## License

[MIT](LICENSE)
