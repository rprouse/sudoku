# Sudoku Zen

A Sudoku puzzle game built with C# and .NET MAUI, targeting Android and Windows. Warm earth-tone aesthetic with a zen-inspired design.

## Projects

| Project | Framework | Description |
|---------|-----------|-------------|
| **Sudoku.Core** | net8.0 | Core library — board model, solver, puzzle generator, and game logic |
| **Sudoku.App** | net10.0-android/windows | .NET MAUI game app with MVVM architecture |
| **Sudoku.Tests** | net8.0 | NUnit 4 tests with FluentAssertions |
| **Sudoku.Benchmarks** | net9.0 | BenchmarkDotNet solver benchmarks |

## Features

- Five difficulty levels: Gentle, Steady, Challenging, Deep, Profound
- Manual and auto-candidate modes with independent tracking
- Undo support with full state restoration
- Error highlighting and related cell highlighting
- Game persistence (save/resume)
- Dark and light theme support with warm earth-tone palette

## Color Palette

### Light Theme

<table>
  <tr>
    <th>Role</th>
    <th>Color</th>
    <th>Hex</th>
  </tr>
  <tr>
    <td>Page Background</td>
    <td><img src="https://via.placeholder.com/24/f5f0eb/f5f0eb" alt="#f5f0eb" /></td>
    <td><code>#f5f0eb</code></td>
  </tr>
  <tr>
    <td>Primary Text</td>
    <td><img src="https://via.placeholder.com/24/3d3632/3d3632" alt="#3d3632" /></td>
    <td><code>#3d3632</code></td>
  </tr>
  <tr>
    <td>Secondary Text</td>
    <td><img src="https://via.placeholder.com/24/8a7e74/8a7e74" alt="#8a7e74" /></td>
    <td><code>#8a7e74</code></td>
  </tr>
  <tr>
    <td>Accent (Sage Green)</td>
    <td><img src="https://via.placeholder.com/24/7d8c6e/7d8c6e" alt="#7d8c6e" /></td>
    <td><code>#7d8c6e</code></td>
  </tr>
  <tr>
    <td>Surface / Controls</td>
    <td><img src="https://via.placeholder.com/24/e0d6cc/e0d6cc" alt="#e0d6cc" /></td>
    <td><code>#e0d6cc</code></td>
  </tr>
  <tr>
    <td>Dividers</td>
    <td><img src="https://via.placeholder.com/24/c4b5a4/c4b5a4" alt="#c4b5a4" /></td>
    <td><code>#c4b5a4</code></td>
  </tr>
  <tr>
    <td>Board Thin Lines</td>
    <td><img src="https://via.placeholder.com/24/c4b5a4/c4b5a4" alt="#c4b5a4" /></td>
    <td><code>#c4b5a4</code></td>
  </tr>
  <tr>
    <td>Board Thick Lines</td>
    <td><img src="https://via.placeholder.com/24/5a4e44/5a4e44" alt="#5a4e44" /></td>
    <td><code>#5a4e44</code></td>
  </tr>
  <tr>
    <td>Selected Cell</td>
    <td><img src="https://via.placeholder.com/24/ddd2c4/ddd2c4" alt="#ddd2c4" /></td>
    <td><code>#ddd2c4</code></td>
  </tr>
  <tr>
    <td>Related Cells</td>
    <td><img src="https://via.placeholder.com/24/f0ddda/f0ddda" alt="#f0ddda" /></td>
    <td><code>#f0ddda</code></td>
  </tr>
  <tr>
    <td>Same Number</td>
    <td><img src="https://via.placeholder.com/24/d8e0d0/d8e0d0" alt="#d8e0d0" /></td>
    <td><code>#d8e0d0</code></td>
  </tr>
  <tr>
    <td>Error Cell</td>
    <td><img src="https://via.placeholder.com/24/e8c4c0/e8c4c0" alt="#e8c4c0" /></td>
    <td><code>#e8c4c0</code></td>
  </tr>
  <tr>
    <td>Player Text</td>
    <td><img src="https://via.placeholder.com/24/7d8c6e/7d8c6e" alt="#7d8c6e" /></td>
    <td><code>#7d8c6e</code></td>
  </tr>
  <tr>
    <td>Candidate Text</td>
    <td><img src="https://via.placeholder.com/24/8a7e74/8a7e74" alt="#8a7e74" /></td>
    <td><code>#8a7e74</code></td>
  </tr>
</table>

### Dark Theme

<table>
  <tr>
    <th>Role</th>
    <th>Color</th>
    <th>Hex</th>
  </tr>
  <tr>
    <td>Page Background</td>
    <td><img src="https://via.placeholder.com/24/1e1b18/1e1b18" alt="#1e1b18" /></td>
    <td><code>#1e1b18</code></td>
  </tr>
  <tr>
    <td>Primary Text</td>
    <td><img src="https://via.placeholder.com/24/e0d8cf/e0d8cf" alt="#e0d8cf" /></td>
    <td><code>#e0d8cf</code></td>
  </tr>
  <tr>
    <td>Secondary Text</td>
    <td><img src="https://via.placeholder.com/24/7a7068/7a7068" alt="#7a7068" /></td>
    <td><code>#7a7068</code></td>
  </tr>
  <tr>
    <td>Accent (Sage Green)</td>
    <td><img src="https://via.placeholder.com/24/9aab88/9aab88" alt="#9aab88" /></td>
    <td><code>#9aab88</code></td>
  </tr>
  <tr>
    <td>Surface / Controls</td>
    <td><img src="https://via.placeholder.com/24/2d2925/2d2925" alt="#2d2925" /></td>
    <td><code>#2d2925</code></td>
  </tr>
  <tr>
    <td>Dividers</td>
    <td><img src="https://via.placeholder.com/24/4a4038/4a4038" alt="#4a4038" /></td>
    <td><code>#4a4038</code></td>
  </tr>
  <tr>
    <td>Board Thin Lines</td>
    <td><img src="https://via.placeholder.com/24/4a4038/4a4038" alt="#4a4038" /></td>
    <td><code>#4a4038</code></td>
  </tr>
  <tr>
    <td>Board Thick Lines</td>
    <td><img src="https://via.placeholder.com/24/7a7068/7a7068" alt="#7a7068" /></td>
    <td><code>#7a7068</code></td>
  </tr>
  <tr>
    <td>Selected Cell</td>
    <td><img src="https://via.placeholder.com/24/3a3228/3a3228" alt="#3a3228" /></td>
    <td><code>#3a3228</code></td>
  </tr>
  <tr>
    <td>Related Cells</td>
    <td><img src="https://via.placeholder.com/24/2c2422/2c2422" alt="#2c2422" /></td>
    <td><code>#2c2422</code></td>
  </tr>
  <tr>
    <td>Same Number</td>
    <td><img src="https://via.placeholder.com/24/2d3328/2d3328" alt="#2d3328" /></td>
    <td><code>#2d3328</code></td>
  </tr>
  <tr>
    <td>Error Cell</td>
    <td><img src="https://via.placeholder.com/24/8b4040/8b4040" alt="#8b4040" /></td>
    <td><code>#8b4040</code></td>
  </tr>
  <tr>
    <td>Player Text</td>
    <td><img src="https://via.placeholder.com/24/9aab88/9aab88" alt="#9aab88" /></td>
    <td><code>#9aab88</code></td>
  </tr>
  <tr>
    <td>Candidate Text</td>
    <td><img src="https://via.placeholder.com/24/7a7068/7a7068" alt="#7a7068" /></td>
    <td><code>#7a7068</code></td>
  </tr>
</table>

### Difficulty Level Colors

<table>
  <tr>
    <th>Level</th>
    <th>Color</th>
    <th>Hex</th>
  </tr>
  <tr>
    <td>Gentle</td>
    <td><img src="https://via.placeholder.com/24/8a9a7b/8a9a7b" alt="#8a9a7b" /></td>
    <td><code>#8a9a7b</code></td>
  </tr>
  <tr>
    <td>Steady</td>
    <td><img src="https://via.placeholder.com/24/b5a67d/b5a67d" alt="#b5a67d" /></td>
    <td><code>#b5a67d</code></td>
  </tr>
  <tr>
    <td>Challenging</td>
    <td><img src="https://via.placeholder.com/24/c49a6c/c49a6c" alt="#c49a6c" /></td>
    <td><code>#c49a6c</code></td>
  </tr>
  <tr>
    <td>Deep</td>
    <td><img src="https://via.placeholder.com/24/a67563/a67563" alt="#a67563" /></td>
    <td><code>#a67563</code></td>
  </tr>
  <tr>
    <td>Profound</td>
    <td><img src="https://via.placeholder.com/24/8b6d7b/8b6d7b" alt="#8b6d7b" /></td>
    <td><code>#8b6d7b</code></td>
  </tr>
</table>

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

## Performance targets

Puzzle generation targets on a typical development machine:

| Tier     | Target generation time |
| -------- | ---------------------- |
| Easy     | < 1 s                   |
| Medium   | < 1 s                   |
| Hard     | < 1 s                   |
| Expert   | < 3 s                   |
| Evil     | < 5 s                   |

`SudokuGenerator` uses `BitmaskSolver` for fast uniqueness checking and `DifficultyGrader` (wrapping `TechniqueSolver`) for technique-based difficulty grading. See `Sudoku.Benchmarks` for measured performance.

## Difficulty grading

Difficulty is determined by the hardest *human-solving technique* actually required to solve the puzzle, not by clue count. Every generated puzzle is solvable using pure logic — if the technique solver can't complete the puzzle, it's rejected.

| Tier (display name)     | Hardest technique required                             |
| ----------------------- | ------------------------------------------------------ |
| Easy (Gentle)           | Naked Single / Hidden Single                           |
| Medium (Steady)         | Naked Pair / Hidden Pair                               |
| Hard (Challenging)      | Pointing Pair, Box/Line Reduction, Naked/Hidden Triple |
| Expert (Deep)           | X-Wing, XY-Wing, Simple Coloring                       |
| Evil (Profound)         | Swordfish, XYZ-Wing, X-Chain                           |

Clue counts per tier are used as a secondary sanity filter (see `SudokuGenerator.GetClueRange`). The technique grade is the primary acceptance criterion — ranges may overlap slightly at tier boundaries.

| Technique | Tier | Description |
|---|---|---|
| Naked Single | Easy | A cell has only one candidate remaining — place it directly. |
| Hidden Single | Easy | A digit has only one possible cell within a row, column, or box. |
| Naked Pair | Medium | Two cells in a unit share the same two candidates, eliminating those digits from the rest of the unit. |
| Hidden Pair | Medium | Two digits appear as candidates in only two cells within a unit, allowing all other candidates in those cells to be removed. |
| Pointing Pair | Hard | A candidate within a box is confined to one row or column, eliminating it from the rest of that row or column outside the box. |
| Box/Line Reduction | Hard | A candidate in a row or column exists only within one box, eliminating it from the rest of that box. |
| Naked Triple | Hard | Three cells in a unit share a total of three candidates between them, eliminating those digits elsewhere in the unit. |
| Hidden Triple | Hard | Three digits appear only within the same three cells in a unit, allowing other candidates in those cells to be removed. |
| Naked Quad ⁺ | Hard–Expert | Like a Naked Triple but with four cells and four candidates. Rarer but mechanically identical in logic. |
| Hidden Quad ⁺ | Hard–Expert | Four digits confined to four cells in a unit; all other candidates in those cells can be eliminated. |
| X-Wing | Expert | A digit appears in only two cells across two rows, and those cells share the same two columns — eliminating that digit from those columns elsewhere. |
| Simple Coloring | Expert | Chains of conjugate pairs (cells where a digit has only two options) are colored to detect contradictions or forced placements. |
| Skyscraper ⁺ | Expert | A two-row or two-column X-Wing variant where one end is offset, creating a chain that eliminates candidates from cells seeing both endpoints. |
| XY-Wing | Expert | Three cells form a chain of bivalue candidates; the pivot sees two wings, and any cell seeing both wings can have their shared candidate eliminated. |
| XYZ-Wing ⁺ | Evil | Like XY-Wing but the pivot cell contains three candidates; cells seen by all three cells can have the shared candidate removed. |
| X-Chain | Evil | A chain of cells linked by a single candidate in conjugate pairs; the endpoints create forced eliminations. |
| Swordfish | Evil | A three-row (or column) extension of X-Wing — a digit confined to the same three columns across three rows eliminates that digit elsewhere in those columns. |
| BUG ⁺ | Evil | Bivalue Universal Grave — if all unsolved cells have exactly two candidates except one, that odd cell must contain the digit that prevents a non-unique solution. |
| Forcing Chains ⁺ | Evil | Hypothetical chains testing both values of a bivalue cell; if both paths lead to the same elimination, that elimination is valid. |

⁺ *Techniques not currently in the app's tier list.*

## Future optimizations

- **Relax symmetry at the hardest tiers.** The generator enforces strict 180° rotational symmetry at every difficulty. This slightly caps the space of reachable puzzles at Expert and Evil, because some genuinely hard configurations can only be reached by asymmetric clue removal. If Deep/Profound puzzles feel too easy after the technique-grading change, the next lever is a two-phase removal in `SudokuGenerator.TryGenerate`: first pass removes symmetric pairs (unchanged), a second pass tries individual-cell removals gated by a per-difficulty `SymmetryPolicy` enum (Strict / PreferredWithFallback). Estimated effort: ~15 lines.

- **More advanced techniques.** The technique solver stops at X-Chain / Swordfish / XYZ-Wing for Evil. Puzzles requiring harder patterns (Jellyfish, ALS, Forcing Chains, Uniqueness Rectangles, Death Blossom) are rejected. Adding more techniques is mechanical: a new file in `Sudoku.Core/Solvers/Techniques/` implementing `ITechnique`, an entry in `DifficultyTechnique`, registration in `TechniqueSolver._techniques` (easiest-first), and a tier mapping in `DifficultyGrader`.

- **Digit-order randomization inside BitmaskSolver.** `SudokuGenerator.BuildFullSolution` seeds a single random cell to perturb the solver's branch choices. If this ever produces insufficient variety across seeds, `BitmaskSolver` can accept an optional `Random` and shuffle candidate order inside its MRV loop.

## Acknowledgements

Puzzle generation initially based on [How to generate sudokus](https://tn1ck.com/blog/how-to-generate-sudokus) by TN1ck and the accompanying [source code](https://github.com/TN1ck/super-sudoku).

## License

[MIT](LICENSE)
