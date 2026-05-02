# Zen Koan Loading Screen — Design Spec

**Date**: 2026-05-01
**Status**: Draft — pending user review
**Scope**: `Sudoku.App` (MAUI), `Sudoku.Core`, `Sudoku.Tests`

## Problem

The Sudoku Zen app currently sends the user directly from the main menu's difficulty buttons to the puzzle. The transition is functional but loses the contemplative tone established by the design system (sage palette, cherry-blossom motif, *gentle*/*steady*/*challenging* difficulty names, "A MINDFUL MOMENT" subtitle).

We want a brief, contemplative interlude between difficulty selection and puzzle: a single zen koan, displayed full-screen on the same cherry-blossom background, with a **Begin** button that opens the puzzle. The koan is randomly selected from a curated pool, expressed with author-controlled soft line breaks, sized typographically by length, and bypassable via a settings toggle.

The koan page is **not** a generation-latency mask. Puzzle generation continues to run on the menu (with the existing `ActivityIndicator`) before navigation. The koan is purely a contemplative interlude — code shape and scope flow from that decision.

## Goals

- A new `KoanPage` displays one randomly-selected koan between menu and puzzle.
- Selecting **Begin** navigates to the puzzle. The koan page must not be present in the back stack while the puzzle is active — back from puzzle returns to menu.
- A `ShowKoans` setting (default `true`) toggles the interlude. When off, the menu navigates directly to the puzzle, exactly as today.
- Soft line breaks in koan text use `|` as a hint character; line breaks are never stored in JSON.
- `"sharp"`-tone koans are excluded from the active pool by default; the JSON file retains them as future-toggleable content.
- Font size is selected per koan by line count, so short koans look weighty and long koans stay readable.
- Pure logic (parsing, filtering) lives in `Sudoku.Core` and is unit-tested. The MAUI-specific file-I/O wrapper is verified by manual smoke test on both target platforms.

## Non-goals

- Not a generation-latency mask. Puzzle generation stays on the menu.
- No animation entry/exit on the koan page beyond Shell's default page transition.
- No "favorite koan," "skip this koan," or "remember last koan" state.
- No tone-weighting algorithm. Filtering is binary: `tone != "sharp"`. The location for future weighting is marked in code with a comment.
- No XAML style extraction. The cherry-blossom `Image` block is duplicated in `KoanPage` to match the existing pattern (`MenuPage`, `SettingsPage`, `GamePage`); CLAUDE.md's "don't add abstractions beyond what the task requires" governs.

## Design

### 1. Asset — `Sudoku.App/Resources/Raw/koans.json`

Curated content provided by the user. 107 entries (one duplicate, the original `id: 107`, dropped). Each entry shape:

```json
{ "id": 1, "text": "Some text|with optional soft breaks", "tone": "gentle" }
```

`tone` values present in the data: `gentle`, `contemplative`, `exchange`, `sharp`. Schema is open-ended — `tone` is parsed as a free-form string, not an enum, so adding new tone categories later requires no code change.

`Resources/Raw` is already configured as `MauiAsset` with `LogicalName="%(RecursiveDir)%(Filename)%(Extension)"` in `Sudoku.App.csproj`, so the file is loadable via `FileSystem.OpenAppPackageFileAsync("koans.json")`.

### 2. Model + parser in `Sudoku.Core`

Both files live in `Sudoku.Core/Game/` so they remain testable from `Sudoku.Tests`. This follows the rule already documented in `CLAUDE.md`:
> *"Game logic models (`GameState`, `CellState`, etc.) live in `Sudoku.Core/Game/` rather than the MAUI project so they remain testable from `Sudoku.Tests`."*

**`Sudoku.Core/Game/Koan.cs`** — POCO with property setters for `System.Text.Json` deserialization (matches `GameSettings`):

```csharp
namespace Sudoku.Core.Game;

public class Koan
{
    public int Id { get; set; }
    public string Text { get; set; } = "";
    public string Tone { get; set; } = "";
}
```

**`Sudoku.Core/Game/KoanParser.cs`** — pure functions, no MAUI or I/O:

```csharp
namespace Sudoku.Core.Game;

public static class KoanParser
{
    public static List<Koan> Parse(string json) =>
        JsonSerializer.Deserialize<List<Koan>>(json, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize koans.");

    public static List<Koan> FilterPlayablePool(IEnumerable<Koan> koans) =>
        // Sharp koans are excluded for now; can be re-enabled later via tone weighting.
        koans.Where(k => k.Tone != "sharp").ToList();

    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNameCaseInsensitive = true };
}
```

### 3. Service — `Sudoku.App/Services/KoanService.cs`

Thin MAUI-specific wrapper around `KoanParser`. Singleton lifetime — the file is parsed and filtered once per app session.

```csharp
namespace Sudoku.App.Services;

public class KoanService
{
    private List<Koan>? _koans;
    private readonly Random _random = new();

    public async Task<Koan> GetRandomAsync()
    {
        if (_koans == null) await LoadAsync();
        return _koans![_random.Next(_koans.Count)];
    }

    private async Task LoadAsync()
    {
        await using var stream = await FileSystem.OpenAppPackageFileAsync("koans.json");
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();
        _koans = KoanParser.FilterPlayablePool(KoanParser.Parse(json));
    }
}
```

`await using` on the stream is intentional — `OpenAppPackageFileAsync` returns a platform-backed stream (Android asset manager, Windows MRT) whose handle should be released asynchronously after the deserialization finishes.

### 4. ViewModel — `Sudoku.App/ViewModels/KoanViewModel.cs`

Transient lifetime (DI registration: `AddTransient`). A fresh instance per page push guarantees a fresh koan on each new puzzle, while the load-once guard inside `LoadAsync` keeps the same koan stable across `OnAppearing` calls (e.g., app resume).

```csharp
namespace Sudoku.App.ViewModels;

public partial class KoanViewModel : ObservableObject
{
    private readonly KoanService _koanService;

    [ObservableProperty]
    public partial string KoanText { get; set; } = "";

    [ObservableProperty]
    public partial double KoanFontSize { get; set; } = 26;

    public KoanViewModel(KoanService koanService) => _koanService = koanService;

    public async Task LoadAsync()
    {
        if (!string.IsNullOrEmpty(KoanText)) return;  // load once per instance

        try
        {
            var koan = await _koanService.GetRandomAsync();
            var lineCount = koan.Text.Split('|').Length;
            KoanText = koan.Text.Replace("|", Environment.NewLine);
            KoanFontSize = lineCount switch
            {
                1 => 32,
                2 => 26,
                3 => 22,
                _ => 20
            };
        }
        catch (Exception)
        {
            // Asset missing or corrupt — bail straight to the puzzle.
            // The koan is decoration; failing to render one must not block play.
            await Shell.Current.GoToAsync($"../{nameof(Views.GamePage)}");
        }
    }

    [RelayCommand]
    private static async Task Begin() =>
        await Shell.Current.GoToAsync($"../{nameof(Views.GamePage)}");
}
```

**Why per-koan font sizing:** A single fixed font size means short koans (e.g., `"Just this."`) look lost in whitespace while long ones (4-line exchanges) crowd the Begin button. A length-curated pool would lose the most evocative content (the multi-line exchange koans) on a maintainability axis the team would forget. Line-count is the right proxy because the soft-break decision already encodes the author's intent about visual rhythm.

**Why `static` on `BeginCommand`:** the method captures nothing from the VM. The MVVM Toolkit source generator handles `static` `[RelayCommand]` methods cleanly and it suppresses an analyzer warning.

**Why catch-all on `LoadAsync` failure:** `OpenAppPackageFileAsync` is a system-boundary call (file system / platform asset manager) that can genuinely fail — corrupt asset, malformed package, platform-specific edge case. The fallback path navigates to the already-loaded puzzle (`GameViewModel.State` was set on the menu before push). The koan is decoration; a render failure must not block play.

### 5. Page — `Sudoku.App/Views/KoanPage.xaml` + `KoanPage.xaml.cs`

Full-screen, no Shell chrome, same cherry-blossom overlay pattern as every other page.

```xml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:vm="clr-namespace:Sudoku.App.ViewModels"
             x:Class="Sudoku.App.Views.KoanPage"
             x:DataType="vm:KoanViewModel"
             Shell.NavBarIsVisible="False"
             BackgroundColor="{AppThemeBinding Light={StaticResource PageBackgroundLight},
                                               Dark={StaticResource PageBackgroundDark}}">

    <Grid RowDefinitions="*,Auto" Padding="32,40">

        <Image Source="{AppThemeBinding Light=sakura_branch_light.png, Dark=sakura_branch_dark.png}"
               Grid.RowSpan="2"
               InputTransparent="True"
               Aspect="AspectFit"
               HorizontalOptions="End"
               VerticalOptions="Start"
               WidthRequest="320"
               HeightRequest="480" />

        <Label Grid.Row="0"
               Text="{Binding KoanText}"
               FontSize="{Binding KoanFontSize}"
               HorizontalTextAlignment="Center"
               VerticalTextAlignment="Center"
               VerticalOptions="Center"
               LineHeight="1.4"
               TextColor="{AppThemeBinding Light=#3d3632, Dark=#e0d8cf}"
               Margin="16" />

        <Button Grid.Row="1"
                Text="Begin"
                FontSize="16"
                HeightRequest="56"
                CornerRadius="16"
                BackgroundColor="{AppThemeBinding Light=#7d8c6e, Dark=#9aab88}"
                TextColor="White"
                Margin="32,16,32,32"
                Command="{Binding BeginCommand}" />

    </Grid>
</ContentPage>
```

```csharp
namespace Sudoku.App.Views;

public partial class KoanPage : ContentPage
{
    private readonly KoanViewModel _viewModel;

    public KoanPage(KoanViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
```

The Begin button color (`#7d8c6e` light / `#9aab88` dark, white text, `CornerRadius="16"`, `HeightRequest="56"`, `FontSize="16"`) replicates the existing primary-action pattern used on the **Continue** button in `MenuPage` and the win-overlay continue in `GamePage`. There is no shared XAML style for primary buttons in the codebase today — extracting one is out of scope for this work.

### 6. Navigation flow

Stack transitions:

```
[MenuPage]                        ← Shell root
   │
   │ tap difficulty button
   │ MenuViewModel.NewGame:
   │   1. generate puzzle (existing Task.Run)
   │   2. set GameViewModel.State (existing pattern)
   │   3. branch on Settings.ShowKoans:
   │      ├── true  → GoToAsync(nameof(KoanPage))
   │      └── false → GoToAsync(nameof(GamePage))   ← unchanged from today
   ▼
[MenuPage, KoanPage]              ← koan visible; back here = back to menu
   │
   │ tap Begin
   │ KoanViewModel.Begin:
   │   GoToAsync($"../{nameof(GamePage)}")
   │   ↑ compound route: pops KoanPage, pushes GamePage in one transaction
   ▼
[MenuPage, GamePage]              ← KoanPage gone; back returns to MenuPage
```

**Why the compound route `"../GamePage"`:**
- `"//GamePage"` (absolute route) replaces the Shell stack root. After the call, `[GamePage]` is the entire stack and back-from-puzzle exits the app instead of returning to the menu — this breaks an explicit requirement of the prompt ("back from puzzle returns to menu").
- `Navigation.RemovePage(this)` operates on `INavigation`, not Shell's routing layer; mixing it with `Shell.Current.GoToAsync` causes flicker on Android.
- `"../GamePage"` is a Shell-idiomatic compound relative route. Shell parses it as two segments (`..` pop one, `GamePage` push) and coalesces them into a **single** navigation transaction. Stack ends as `[MenuPage, GamePage]`, back goes to menu, single animation, no flicker.

### 7. `MenuViewModel` — the navigation branch

Three changes to `MenuViewModel`: add a `SettingsService` constructor parameter (already a registered singleton in DI), add the matching `_settingsService` field, and replace the unconditional `GoToAsync` at the end of `NewGame` with a branch on `Settings.ShowKoans`.

**Constructor:**

```csharp
public MenuViewModel(SudokuGenerator generator, ISolver solver,
    GamePersistenceService persistenceService, GameViewModel gameViewModel,
    SettingsService settingsService)                          // NEW parameter
{
    _generator = generator;
    _solver = solver;
    _persistenceService = persistenceService;
    _gameViewModel = gameViewModel;
    _settingsService = settingsService;                       // NEW assignment
}
```

**`NewGame` body (only the trailing nav line changes):**

```csharp
[RelayCommand]
private async Task NewGame(string difficultyName)
{
    if (IsGenerating) return;
    IsGenerating = true;

    var difficulty = Enum.Parse<Difficulty>(difficultyName);
    var (puzzle, solution) = await Task.Run(() =>
    {
        var p = _generator.Generate(difficulty);
        var s = _solver.Solve(new SudokuBoard(p));
        return (p, s);
    });

    _persistenceService.Delete();
    var state = new GameState(puzzle, solution, difficulty);
    _gameViewModel.LoadState(state);

    IsGenerating = false;

    var route = _settingsService.Load().ShowKoans                 // NEW branch
        ? nameof(Views.KoanPage)
        : nameof(Views.GamePage);
    await Shell.Current.GoToAsync(route);
}
```

The `ContinueGame` command is unchanged — resuming a saved game still goes directly to `GamePage`, never via koan. (A koan only marks the start of a *new* practice; resuming an in-progress game is not a fresh sit.)

### 8. Settings

**`Sudoku.Core/Game/GameSettings.cs`** — add one property at the bottom:

```csharp
public bool ShowKoans { get; set; } = true;
```

**`Sudoku.App/Services/SettingsService.cs`** — three lines added, mirroring the existing pattern:

```csharp
private const string ShowKoansKey = "ShowKoans";
// in Load():   ShowKoans = Preferences.Default.Get(ShowKoansKey, true)
// in Save():   Preferences.Default.Set(ShowKoansKey, settings.ShowKoans);
```

**`Sudoku.App/ViewModels/SettingsViewModel.cs`** — observable property + change handler + load assignment, mirroring the existing pattern:

```csharp
[ObservableProperty]
public partial bool ShowKoans { get; set; } = true;

partial void OnShowKoansChanged(bool value) { _settings.ShowKoans = value; Save(); }

// in LoadSettings():   ShowKoans = _settings.ShowKoans;
```

**`Sudoku.App/Views/SettingsPage.xaml`** — new toggle row inserted after the **Show Errors** row, under the existing **GAMEPLAY** section header. Title pattern matches the other rows in that section ("Highlight Related Cells" / "Shade row, column, and box"):

```xml
<Grid ColumnDefinitions="*,Auto" Padding="0,14">
    <VerticalStackLayout>
        <Label Text="Zen Koans" FontSize="15"
               TextColor="{AppThemeBinding Light=#3d3632, Dark=#e0d8cf}" />
        <Label Text="Show a Zen koan before each puzzle" FontSize="12"
               TextColor="{AppThemeBinding Light=#8a7e74, Dark=#7a7068}" />
    </VerticalStackLayout>
    <Switch IsToggled="{Binding ShowKoans}" OnColor="#7d8c6e"
            Grid.Column="1" VerticalOptions="Center" />
</Grid>
```

No new section header — adding a single toggle under its own section ("MINDFULNESS" or similar) is overkill; "Zen Koans" reads naturally as a gameplay-flow toggle.

### 9. DI and routing registration

**`Sudoku.App/MauiProgram.cs`** — three additions:

```csharp
builder.Services.AddSingleton<KoanService>();
builder.Services.AddTransient<KoanViewModel>();
builder.Services.AddTransient<KoanPage>();
```

**`Sudoku.App/AppShell.xaml.cs`** — one route registration:

```csharp
Routing.RegisterRoute(nameof(Views.KoanPage), typeof(Views.KoanPage));
```

## Testing

### Unit tests in `Sudoku.Tests`

Two new files. The tests rely on `Koan` and `KoanParser` living in `Sudoku.Core` per §2.

**`Sudoku.Tests/KoanParserTests.cs`** — pure logic, no I/O:

| Test | Setup | Asserts |
|---|---|---|
| `Parse_ValidJson_ReturnsAllEntries` | small canned JSON string with 3 entries | returns 3 koans, fields populated correctly |
| `Parse_MalformedJson_Throws` | garbage string | throws `JsonException` |
| `Parse_NullJson_Throws` | the literal string `"null"` | throws `InvalidOperationException` |
| `FilterPlayablePool_ExcludesSharp` | mixed tones including `"sharp"` | result contains no `sharp` entries |
| `FilterPlayablePool_KeepsAllOtherTones` | mix of `gentle`, `contemplative`, `exchange`, and an unknown tone | all four pass through |
| `FilterPlayablePool_PreservesOrder` | ordered input, no sharp entries | output order matches input order |

**`Sudoku.Tests/KoansAssetTests.cs`** — validates the actual production data file:

| Test | Asserts |
|---|---|
| `KoansFile_AllEntriesHaveText` | no entry has empty `Text` |
| `KoansFile_AllTonesAreKnown` | every `Tone` is in `{ gentle, contemplative, exchange, sharp }` |
| `KoansFile_AllIdsAreUnique` | distinct IDs across all entries |
| `KoansFile_PlayablePoolIsNotEmpty` | filtering sharp leaves at least 50 entries |
| `KoansFile_HasSoftBreaks` | at least one entry's `Text` contains `\|` |

The asset is referenced by **`<Link>`** in `Sudoku.Tests.csproj` so tests read from the same `koans.json` as the app — single source of truth, no drift:

```xml
<ItemGroup>
  <None Include="..\Sudoku.App\Resources\Raw\koans.json" Link="koans.json">
    <CopyToOutputDirectory>PreserveNewerOutput</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

### Manual smoke checklist

Run on Windows **and** Android, light **and** dark theme:

1. Fresh install → tap any difficulty → koan page appears with cherry-blossom overlay in correct theme tones, koan rendered centered, **Begin** anchored bottom.
2. Tap **Begin** → puzzle loads. Hardware/Shell back from puzzle returns to menu (not koan).
3. Open Settings → toggle **Zen Koans** off → return to menu → tap difficulty → puzzle loads directly, no koan.
4. Toggle **Zen Koans** back on, kill app, relaunch — toggle is still on (Preferences persistence).
5. Start ten new puzzles in succession — confirm koans visibly vary, sharp-tone koans never appear (id 103, 104, 105, 106).
6. Force a koan-load failure on a debug build (rename `koans.json`) → puzzle still loads (fallback path in `KoanViewModel.LoadAsync`).
7. Verify a 1-line koan (e.g. `"Just this."`) and a 4-line koan (e.g. `id 102`) both display well — short feels weighty, long stays readable.

### Not unit-tested (and why)

| Code | Why not | How verified |
|---|---|---|
| `KoanService` (file load + RNG) | `FileSystem.OpenAppPackageFileAsync` is MAUI-platform | Smoke test items 1, 5, 6 |
| `KoanViewModel.LoadAsync` (font-size switch + Replace) | Lives in App project, not testable from `Sudoku.Tests`; logic is a 4-arm switch + one string op | Smoke test item 7 |
| `KoanPage.xaml` (layout, theme bindings) | XAML only renders on a platform | Smoke test item 1 on both targets, both themes |
| Navigation (`"../GamePage"` compound route) | Shell needs a host | Smoke test item 2 |
| Settings round-trip on `Preferences.Default` | MAUI Essentials API | Smoke test items 3, 4 |

## Risks and mitigations

| Risk | Mitigation |
|---|---|
| Cherry-blossom `Image` block is duplicated across four pages now (Menu, Settings, Game, Koan). | Acknowledged. Following established pattern; CLAUDE.md prohibits speculative abstraction. Extraction to a `ContentView` is a follow-up if the pattern reaches a fifth page or if a cross-cutting change to the overlay (e.g., adjusting size or opacity) is wanted. |
| Lazy file load means the first koan render has a brief moment of empty `Text`. | Default `KoanFontSize = 26` keeps the Label sized neutrally so the swap-in doesn't reflow. JSON is small and the parse runs in well under 100ms in practice; the koan page's own animation absorbs it. |
| `OpenAppPackageFileAsync` fails (corrupt asset, platform-specific edge case). | `KoanViewModel.LoadAsync` catches all exceptions and navigates straight to `GamePage` via the same compound route Begin uses. The puzzle is already loaded into `GameViewModel.State`. User sees no error; play continues. |
| User toggles `ShowKoans` mid-game (i.e., from the Settings page reached *during* a puzzle). | The setting is read in `MenuViewModel.NewGame`, so it only affects the *next* new puzzle. In-flight games are unaffected. Resume of saved games never goes through the koan path regardless. |
| `KoanService` singleton holds the koan list for the app's lifetime. | The list is ~107 small POCOs (~10 KB). Negligible. The singleton is the right lifetime — re-parsing per page push wastes I/O. |
| `Random` is instance-per-service, not seeded for reproducibility. | Acceptable. The sole consumer is decorative koan selection; deterministic behavior is not a goal. If we ever want "show fewer recent koans," that's a separate state-tracking change that does not depend on the RNG choice here. |

## Out-of-scope follow-ups

- **Tone weighting.** The `tone` field exists in the data but the only filter is `tone != "sharp"`. A future pass could weight selection by tone, throttle sharp koans to "every Nth session," or implement a recency window. The marker is the comment on the `FilterPlayablePool` line.
- **Recency suppression.** "Don't repeat the last N koans" requires a small piece of persistent state (`Preferences.Default.Get("RecentKoanIds", "[]")`). Not worth adding until the user reports observed repetition.
- **Animation entry/exit.** A subtle fade-in on the koan label and fade-out on **Begin** would feel polished. Out of scope; Shell's default page transition is adequate.
- **`SakuraBranchOverlay` ContentView.** Once a fifth consumer of the overlay exists, or once cross-cutting changes to it are wanted, extract it. Mechanical and isolated.
- **Primary-action button style.** The `#7d8c6e` / `#9aab88` / white / `CornerRadius=16` / `HeightRequest=56` pattern is now repeated four places. A `PrimaryButton` style in `Styles.xaml` would be a small, scoped follow-up.

## Implementation order

1. **Data + Core logic.** Add `Sudoku.Core/Game/Koan.cs` and `Sudoku.Core/Game/KoanParser.cs`. Add `Sudoku.Tests/KoanParserTests.cs` with all six parser/filter tests; verify they pass.
2. **Asset.** Add `Sudoku.App/Resources/Raw/koans.json` with the 107 curated entries. Add the `<Link>` reference and `Sudoku.Tests/KoansAssetTests.cs`; verify all five asset tests pass.
3. **Settings plumbing.** Add `ShowKoans` to `GameSettings`, `SettingsService`, `SettingsViewModel`, and the new toggle row to `SettingsPage.xaml`. Verify build succeeds; smoke-test the toggle round-trips through `Preferences.Default`.
4. **Service.** Add `Sudoku.App/Services/KoanService.cs`. Register as singleton in `MauiProgram.cs`. Verify build.
5. **ViewModel and page.** Add `KoanViewModel`, `KoanPage.xaml`, `KoanPage.xaml.cs`. Register VM/page transient in `MauiProgram.cs`, register route in `AppShell.xaml.cs`.
6. **Wire navigation.** Modify `MenuViewModel.NewGame` to branch on `Settings.ShowKoans` and add the `SettingsService` constructor parameter.
7. **Smoke test** the full checklist above on both target platforms in both themes.
