# Cherry Blossom Theme — Design Spec

**Date**: 2026-04-21
**Status**: Draft — pending user review
**Scope**: `Sudoku.App` (MAUI) + small test addition in `Sudoku.Tests`

## Overview

Add a subtle cherry-blossom visual accent to the existing zen earth-tone theme. The change is purely cosmetic and additive — no behavior, no save format, no API changes.

Two concrete additions:

1. A soft "watercolor" sakura branch appears in the top-right corner of every screen (MenuPage, GamePage, SettingsPage) and behind the Ensō on the splash screen.
2. The board's row/column/box highlight — shown when a cell is selected — shifts from warm cream to a near-imperceptible pink.

Every other visual element (sage-green accent, warm earth palette, typography, layout, number-pad, win overlay) remains unchanged.

## Goals

- Introduce a cherry-blossom visual signature across the app.
- Preserve the app's zen/serene tone — the pink is a whisper, not a shout.
- Keep the sage-green accent as the primary UI color (Continue button, win-card title, candidate-mode checkbox, player numbers, difficulty label). Pink is additive, not a rebrand.

## Non-goals

- No change to sage-green button/accent colors.
- No change to selected-cell, same-number, or error-cell highlights on the board.
- No change to Android adaptive icon or app icon — splash only.
- No runtime theme customization (user-selectable accents, etc.). The light/dark pair is fixed.
- No animations on the blossoms. Static imagery.

## Design decisions (summary)

| Decision | Choice |
|---|---|
| Pink accent strategy | Additive — only for board row/col/box highlight. Sage unchanged. |
| Watercolor composition | Corner branch (sakura branch sweeping from top-right corner) |
| Screen scope | MenuPage, SettingsPage, GamePage (margins only — board masks its area), Splash |
| Position variation across screens | Same top-right anchor on every screen |
| Dark-theme palette | Dusty rose at ~40% opacity (recessed but still clearly cherry blossom) |
| Light-theme related-cell color | `#f0ddda` (halfway between whisper and blush) |
| Dark-theme related-cell color | `#2e2224` (same "shift" applied to the existing warm dark cell tint) |
| Splash integration | Ensō remains on top; branch painted behind it |
| Rendering approach | Two static SVG files (light/dark variants) referenced via `AppThemeBinding` |
| Source-of-truth enforcement | Parity test asserting geometry is identical across the two files |
| GamePage bottom margin | No bottom branch — only the top-right anchor; bottom area stays clean |

## Palette

### Watercolor asset palette

| Element | Light theme | Dark theme |
|---|---|---|
| Blossom — light petal | `#f2cdd1` | `#a87682` |
| Blossom — deep petal | `#edb9c2` | `#966374` |
| Branch bark | `#a08778` | `#6a4f40` |
| Leaf | `#a8b89a` | `#6d7a5e` |
| Root `<svg opacity>` | `0.55` | `0.4` |

### Board related-cell highlight (light/dark theme)

| Token | Before | After |
|---|---|---|
| `s_lightRelatedCell` | `#ebe4db` | `#f0ddda` |
| `s_darkRelatedCell` | `#2a2520` | `#2e2224` |

## Asset design

Two SVG files live in `Sudoku.App/Resources/Images/`:

- `sakura_branch_light.svg`
- `sakura_branch_dark.svg`

Both files share the same `viewBox` (portrait-ish canvas; exact dimensions tuned during implementation — a 600×1000 starting point is reasonable) and the same shape vocabulary:

- 2 bezier paths: a main branch curving in from the top-right, plus one small offshoot.
- ~10 blossom clusters. Each cluster is 2–3 overlapping `<circle>` elements using the two blossom colors from the palette.
- 2 `<ellipse>` leaves in the sage-leaf color.
- A single `<defs>` block defining two filters: one `feGaussianBlur` with `stdDeviation="2"` (applied to branch), one with `stdDeviation="4"` (applied to blossoms and leaves). Filters are referenced via `filter="url(#…)"` on the shapes.
- Root `<svg opacity="…">` attribute controls the overall theme intensity.

The branch composition occupies roughly the top-right 40% of the canvas, trailing down-left. Everything outside that area is transparent.

Only color and opacity literals differ between `sakura_branch_light.svg` and `sakura_branch_dark.svg`. All geometry (`d`, `cx`, `cy`, `r`, `rx`, `ry`), element ordering, and filter structure are identical.

## Page integration

### MenuPage, SettingsPage

Add a single `<Image>` as the first child of the root `Grid` in `Views/MenuPage.xaml` and `Views/SettingsPage.xaml`:

```xml
<Image Source="{AppThemeBinding Light=sakura_branch_light.png, Dark=sakura_branch_dark.png}"
       Grid.RowSpan="2"
       ZIndex="-1"
       InputTransparent="True"
       Aspect="AspectFit"
       HorizontalOptions="End"
       VerticalOptions="Start" />
```

(Both MenuPage and SettingsPage use `RowDefinitions="*,Auto"` / `"Auto,*"` respectively — `RowSpan="2"` covers the full layout on both.)

Notes:

- MAUI's resizetizer converts `.svg` in `Resources/Images/` to platform-native PNGs at build time. XAML references use the `.png` filename.
- `ZIndex="-1"` keeps the image behind content.
- `InputTransparent="True"` ensures taps pass through to underlying controls.
- Sizing may need small platform-specific tuning (Windows vs Android). The anchor is top-right; exact `WidthRequest` / `HeightRequest` values are an implementation detail to settle during build-and-look iteration.

### GamePage

Same pattern in `Views/GamePage.xaml`, with `Grid.RowSpan="6"` covering the full outer `Grid`.

The board's `GraphicsView` (at `Grid.Row="2"`) paints opaque cell backgrounds in `SudokuBoardDrawable.DrawCellBackgrounds` — the board naturally masks the branch in the board area. No explicit clipping is required.

The branch is therefore visible:

- Above the board (nav bar and info bar area).

And hidden:

- Behind the board (the opaque 9×9 grid covers any overlap).

There is deliberately no branch below the board — the top-right anchor and single asset keeps bottom margins clean. (The bottom already carries the mode toggle, undo button, number pad, and auto-candidate row — visually full.)

### Splash

Edit `Sudoku.App/Resources/Splash/splash.svg` in place. Inline the branch paths from `sakura_branch_light.svg` (splash uses the light palette because MAUI splash renders before user theme preference is applied reliably).

Draw order inside `splash.svg`:

1. Branch paths and blossoms first (bottom layer).
2. Existing Ensō paths last (top layer — Ensō sits visually in front).

After editing `splash.svg`, the resizetizer output must be cleared (see Gotchas).

## Board highlight change

File: `Sudoku.App/Controls/SudokuBoardDrawable.cs`.

Two `static readonly Color` fields change:

- `s_lightRelatedCell`: `#ebe4db` → `#f0ddda`
- `s_darkRelatedCell`: `#2a2520` → `#2e2224`

No other fields change. The existing `DrawCellBackgrounds` and `GetCellColor` logic applies the new colors automatically. Selected-cell (`s_*SelectedCell`), same-number (`s_*SameNumber`), and error (`s_*ErrorCell`) colors are untouched — only the row/column/box "related" tint shifts.

`Colors.xaml` needs no additions — the project's existing convention is that board colors live in `SudokuBoardDrawable.cs` as code constants, while resource-dictionary colors cover page/number-pad/difficulty surfaces.

## Testing

### SVG parity test

New test file: `Sudoku.Tests/SakuraBranchParityTests.cs`.

Intent: the two SVG files must share identical geometry. Only color and opacity attributes may differ. A test catches geometry drift when one file is edited and the other forgotten.

Approach:

1. Add `<None Include="..\Sudoku.App\Resources\Images\sakura_branch_*.svg" CopyToOutputDirectory="PreserveNewest" />` (or equivalent) to `Sudoku.Tests.csproj` so both files are available next to the test assembly at runtime.
2. Load both files and parse them with `System.Xml.Linq.XDocument`.
3. Walk the element trees in document order, pairing elements by index.
4. For each paired element:
   - Assert the tag name matches.
   - Assert the **geometry attributes** match: `d`, `cx`, `cy`, `r`, `rx`, `ry`, `x`, `y`, `width`, `height`, `stroke-width`, `filter`, `id`.
   - **Ignore** color/opacity attributes: `fill`, `stroke`, `opacity`, `stop-color`, `stop-opacity`, plus `stdDeviation` on `feGaussianBlur`.
5. Assert both files have the same total element count.

Any unexpected divergence — a different number of circles, a path with a different `d`, a missing filter reference — fails the test.

### Board color smoke test (optional)

Low-value but cheap: verify the two new hex values are present on `SudokuBoardDrawable`'s related-cell fields. This pins the spec values against accidental edits. Can be skipped if it feels excessive.

### No visual regression tests

Watercolor aesthetics are a human judgement. The developer building this feature must eyeball both themes on Windows and Android after build.

## Gotchas

- **Resizetizer caching** (from `CLAUDE.md`): after first adding the SVGs or editing `splash.svg`, delete `obj/**/resizetizer/` and rebuild. Otherwise changes may not appear.
- **`feGaussianBlur` render region**: SkiaSharp's SVG renderer respects the filter element's `x/y/width/height` region. If blossom edges appear clipped, expand the filter region (e.g. `x="-20%" y="-20%" width="140%" height="140%"`).
- **`dominant-baseline` unsupported** (from `CLAUDE.md`): not used here, but noted for future reference.
- **Image anchoring across platforms**: `HorizontalOptions="End" VerticalOptions="Start"` with `AspectFit` may render slightly differently on Windows vs Android. Expect minor tuning of size properties on first build.
- **Splash reads light palette**: on systems where dark-mode preference is available at splash time, the light-palette branch will still render (slight tonal mismatch for a split second). This is acceptable — splash is brief and the Ensō is white on warm background regardless of theme. If a dark splash is later desired, it's an MSBuild-config job and belongs in a separate change.

## Rollout

- Purely additive visual change.
- No save-file, no enum, no API change. Existing save games load without migration.
- Feature appears on next build / install.
- No feature flag needed.

## Out of scope / future work

- Animated petals drifting across the screen (zen-breaking, not worth it).
- Seasonal theme rotation (e.g. autumn leaves).
- User-selectable accent colors.
- A dark-specific splash screen (requires platform-specific splash configuration).

## Files touched

Created:

- `Sudoku.App/Resources/Images/sakura_branch_light.svg`
- `Sudoku.App/Resources/Images/sakura_branch_dark.svg`
- `Sudoku.Tests/SakuraBranchParityTests.cs`

Modified:

- `Sudoku.App/Views/MenuPage.xaml` — adds one `<Image>`
- `Sudoku.App/Views/SettingsPage.xaml` — adds one `<Image>`
- `Sudoku.App/Views/GamePage.xaml` — adds one `<Image>`
- `Sudoku.App/Controls/SudokuBoardDrawable.cs` — two color constant edits
- `Sudoku.App/Resources/Splash/splash.svg` — branch paths added behind Ensō
- `Sudoku.Tests/Sudoku.Tests.csproj` — copies the two SVGs into test output
