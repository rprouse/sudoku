# Zen Koan Loading Screen Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a contemplative koan interlude page between the main menu and a new puzzle in the Sudoku Zen MAUI app, bypassable via a settings toggle.

**Architecture:** Pure parsing/filtering logic lives in `Sudoku.Core/Game/` (testable from `Sudoku.Tests`). MAUI-specific glue (file I/O, RNG, ViewModel, page) lives in `Sudoku.App/`. Navigation uses Shell's compound relative route `"../GamePage"` so the koan page never appears in the back stack while the puzzle is active. A new `ShowKoans` boolean is added to the existing `GameSettings` POCO and persisted via the existing `SettingsService`. Failure to load the koan asset is caught in `KoanViewModel.LoadAsync` and falls through to the puzzle, because the koan is decoration and must not block play.

**Tech Stack:** .NET 10, MAUI (Shell), CommunityToolkit.Mvvm 8.4.2 (`ObservableObject` + partial properties + `[RelayCommand]`), `System.Text.Json`, NUnit 4 + FluentAssertions for tests.

**Spec:** `docs/superpowers/specs/2026-05-01-zen-koan-loading-screen-design.md`

---

## Before you start

The spec was authored on `main`. Decide whether to branch:

- [ ] **Step 0a: Create a feature branch (recommended).**

```powershell
git checkout -b feat/zen-koan-loading-screen
```

- [ ] **Step 0b: Sanity-check baseline build and tests.**

```powershell
dotnet build
dotnet test Sudoku.Tests
```

Expected: build succeeds across all TFMs that are installed on this machine; all existing tests pass. **If they don't pass on `main`, stop and fix the baseline before starting.** A failing baseline will mask regressions you introduce.

---

## Task 1: Koan model and KoanParser in `Sudoku.Core` (TDD)

**Goal:** Pure-logic deserialization and tone-filter, fully unit-tested with no MAUI dependency.

**Files:**
- Create: `Sudoku.Core/Game/Koan.cs`
- Create: `Sudoku.Core/Game/KoanParser.cs`
- Create: `Sudoku.Tests/KoanParserTests.cs`

### Steps

- [ ] **Step 1.1: Write the failing tests (`Sudoku.Tests/KoanParserTests.cs`).**

```csharp
using FluentAssertions;
using Sudoku.Core.Game;

namespace Sudoku.Tests;

public class KoanParserTests
{
    [Test]
    public void Parse_ValidJson_ReturnsAllEntries()
    {
        const string json = """
            [
              { "id": 1, "text": "alpha", "tone": "gentle" },
              { "id": 2, "text": "beta|two lines", "tone": "contemplative" },
              { "id": 3, "text": "gamma", "tone": "exchange" }
            ]
            """;

        var koans = KoanParser.Parse(json);

        koans.Should().HaveCount(3);
        koans[0].Id.Should().Be(1);
        koans[0].Text.Should().Be("alpha");
        koans[0].Tone.Should().Be("gentle");
        koans[1].Text.Should().Be("beta|two lines");
        koans[2].Tone.Should().Be("exchange");
    }

    [Test]
    public void Parse_MalformedJson_Throws()
    {
        const string json = "{ this is not valid json";

        var act = () => KoanParser.Parse(json);

        act.Should().Throw<System.Text.Json.JsonException>();
    }

    [Test]
    public void Parse_NullJson_Throws()
    {
        var act = () => KoanParser.Parse("null");

        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void FilterPlayablePool_ExcludesSharp()
    {
        var input = new List<Koan>
        {
            new() { Id = 1, Text = "a", Tone = "gentle" },
            new() { Id = 2, Text = "b", Tone = "sharp" },
            new() { Id = 3, Text = "c", Tone = "contemplative" },
            new() { Id = 4, Text = "d", Tone = "sharp" },
        };

        var pool = KoanParser.FilterPlayablePool(input);

        pool.Should().HaveCount(2);
        pool.Should().NotContain(k => k.Tone == "sharp");
    }

    [Test]
    public void FilterPlayablePool_KeepsAllOtherTones()
    {
        var input = new List<Koan>
        {
            new() { Id = 1, Text = "a", Tone = "gentle" },
            new() { Id = 2, Text = "b", Tone = "contemplative" },
            new() { Id = 3, Text = "c", Tone = "exchange" },
            new() { Id = 4, Text = "d", Tone = "playful" },  // unknown future tone
        };

        var pool = KoanParser.FilterPlayablePool(input);

        pool.Should().HaveCount(4);
    }

    [Test]
    public void FilterPlayablePool_PreservesOrder()
    {
        var input = new List<Koan>
        {
            new() { Id = 10, Text = "a", Tone = "gentle" },
            new() { Id = 20, Text = "b", Tone = "contemplative" },
            new() { Id = 30, Text = "c", Tone = "exchange" },
        };

        var pool = KoanParser.FilterPlayablePool(input);

        pool.Select(k => k.Id).Should().Equal(10, 20, 30);
    }
}
```

- [ ] **Step 1.2: Run the tests to verify they fail.**

```powershell
dotnet test Sudoku.Tests --filter "FullyQualifiedName~KoanParserTests"
```

Expected: **build error** (`The type or namespace name 'Koan' could not be found`, `'KoanParser' could not be found`). Compilation failure is the equivalent of a failing test in statically typed C#. Confirm the error mentions the missing types — if it mentions something else (typo in test, etc.), fix that first.

- [ ] **Step 1.3: Create `Sudoku.Core/Game/Koan.cs`.**

```csharp
namespace Sudoku.Core.Game;

public class Koan
{
    public int Id { get; set; }
    public string Text { get; set; } = "";
    public string Tone { get; set; } = "";
}
```

- [ ] **Step 1.4: Create `Sudoku.Core/Game/KoanParser.cs`.**

```csharp
using System.Text.Json;

namespace Sudoku.Core.Game;

public static class KoanParser
{
    public static List<Koan> Parse(string json) =>
        JsonSerializer.Deserialize<List<Koan>>(json, s_jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize koans.");

    public static List<Koan> FilterPlayablePool(IEnumerable<Koan> koans) =>
        // Sharp koans are excluded for now; can be re-enabled later via tone weighting.
        koans.Where(k => k.Tone != "sharp").ToList();

    private static readonly JsonSerializerOptions s_jsonOptions =
        new() { PropertyNameCaseInsensitive = true };
}
```

- [ ] **Step 1.5: Run the tests again to verify they pass.**

```powershell
dotnet test Sudoku.Tests --filter "FullyQualifiedName~KoanParserTests"
```

Expected: **6 tests pass, 0 failed.**

- [ ] **Step 1.6: Run the full test suite to verify nothing else broke.**

```powershell
dotnet test
```

Expected: all tests pass (existing tests + the 6 new ones).

- [ ] **Step 1.7: Commit.**

```powershell
git add Sudoku.Core/Game/Koan.cs Sudoku.Core/Game/KoanParser.cs Sudoku.Tests/KoanParserTests.cs
git commit -m "feat(core): add Koan model and KoanParser with sharp-tone filter"
```

---

## Task 2: Asset file and asset validation tests

**Goal:** Ship `koans.json` in `Sudoku.App/Resources/Raw/` (auto-deployed as a `MauiAsset`), and validate its schema/content from `Sudoku.Tests` via a `<Link>` reference.

**Files:**
- Create: `Sudoku.App/Resources/Raw/koans.json`
- Modify: `Sudoku.Tests/Sudoku.Tests.csproj` (add `<Link>` reference to the JSON)
- Create: `Sudoku.Tests/KoansAssetTests.cs`

### Steps

- [ ] **Step 2.1: Create `Sudoku.App/Resources/Raw/koans.json` with the curated content (107 entries, `id 107` dropped as the duplicate of `id 57`).**

```json
[
  { "id": 1, "text": "Before enlightenment, chop wood, carry water.|After enlightenment, chop wood, carry water.", "tone": "gentle" },
  { "id": 2, "text": "What was your face before your parents were born?", "tone": "contemplative" },
  { "id": 3, "text": "The moon does not consider whether|she is reflected in the water.", "tone": "gentle" },
  { "id": 4, "text": "If you understand, things are just as they are.|If you do not understand, things are just as they are.", "tone": "gentle" },
  { "id": 5, "text": "Not knowing is most intimate.", "tone": "contemplative" },
  { "id": 6, "text": "The way is not difficult|for those who have no preferences.", "tone": "gentle" },
  { "id": 7, "text": "When you get there, there is nowhere to arrive.", "tone": "contemplative" },
  { "id": 8, "text": "The snow falls, each flake|in its appropriate place.", "tone": "gentle" },
  { "id": 9, "text": "Sitting quietly, doing nothing,|spring comes and the grass grows by itself.", "tone": "gentle" },
  { "id": 10, "text": "The wild geese do not intend to cast their reflection.|The water has no mind to receive their image.", "tone": "gentle" },
  { "id": 11, "text": "The flower invites the butterfly with no-mind.|The butterfly visits the flower with no-mind.", "tone": "gentle" },
  { "id": 12, "text": "Mountains are mountains.|Water is water.", "tone": "gentle" },
  { "id": 13, "text": "The pine tree does not know|it is teaching you patience.", "tone": "gentle" },
  { "id": 14, "text": "The candle flame does not mourn what it has burned.", "tone": "gentle" },
  { "id": 15, "text": "This very place is the lotus land.|This very body, the Buddha.", "tone": "gentle" },
  { "id": 16, "text": "Spring flowers, autumn moon, summer breeze, winter snow.|With a clear, uncluttered mind,|every season is a good season.", "tone": "gentle" },
  { "id": 17, "text": "Carrying water, I am the water.|Carrying wood, I am the wood.", "tone": "gentle" },
  { "id": 18, "text": "The finger pointing at the moon|is not the moon.", "tone": "gentle" },
  { "id": 19, "text": "The oak tree does not wait for permission|to become an oak tree.", "tone": "gentle" },
  { "id": 20, "text": "The river does not refuse the fallen leaf.", "tone": "gentle" },
  { "id": 21, "text": "Nothing is hidden.|Everything has always been here.", "tone": "contemplative" },
  { "id": 22, "text": "Still water reflects the mountain|exactly as it is.", "tone": "gentle" },
  { "id": 23, "text": "The flower does not compare itself to other flowers.", "tone": "gentle" },
  { "id": 24, "text": "The turtle carries its home|without calling it a burden.", "tone": "gentle" },
  { "id": 25, "text": "The stone does not try to be still.|It simply is.", "tone": "gentle" },
  { "id": 26, "text": "The cloud does not decide|which direction to travel.", "tone": "gentle" },
  { "id": 27, "text": "One moon shows in every pool.|In every pool, one moon.", "tone": "gentle" },
  { "id": 28, "text": "All rivers lead to the sea.|The sea is never full.", "tone": "gentle" },
  { "id": 29, "text": "The old pond.|A frog jumps in.|Sound of water.", "tone": "gentle" },
  { "id": 30, "text": "The wind does not move.|The flag does not move.|Your mind moves.", "tone": "contemplative" },
  { "id": 31, "text": "The candle does not know that it gives light.|That is why it gives light.", "tone": "gentle" },
  { "id": 32, "text": "The bird leaves no trace in the air.|The fish leaves no trace in the water.", "tone": "gentle" },
  { "id": 33, "text": "Muddy water,|let stand,|becomes clear.", "tone": "gentle" },
  { "id": 34, "text": "The dewdrop slides off the lotus leaf|without leaving a trace.", "tone": "gentle" },
  { "id": 35, "text": "The deep river does not boast of its depth.", "tone": "gentle" },
  { "id": 36, "text": "Born from emptiness,|returning to emptiness,|playing in the in-between.", "tone": "gentle" },
  { "id": 37, "text": "Nothing remains but a single moon|reflected in the cold water.", "tone": "gentle" },
  { "id": 38, "text": "The seed does not struggle to become the tree.", "tone": "gentle" },
  { "id": 39, "text": "The forest is silent.|The tree falls.|The forest is silent.", "tone": "gentle" },
  { "id": 40, "text": "The mind is like the moon on water.|Handle it too roughly and the image is gone.", "tone": "gentle" },
  { "id": 41, "text": "The silence between two notes|is also music.", "tone": "gentle" },
  { "id": 42, "text": "Consider the lotus:|rooted in mud,|blooming in light.", "tone": "gentle" },
  { "id": 43, "text": "The flame does not ask why it is warm.", "tone": "gentle" },
  { "id": 44, "text": "The mountain does not move.|Therefore it arrives everywhere.", "tone": "contemplative" },
  { "id": 45, "text": "Soft water|wears away hard stone.|Be soft water.", "tone": "gentle" },
  { "id": 46, "text": "Before thinking, what are you?", "tone": "contemplative" },
  { "id": 47, "text": "You cannot see your own eyes without a mirror.|You cannot know your own mind without stillness.", "tone": "contemplative" },
  { "id": 48, "text": "Where were you before the universe began?", "tone": "contemplative" },
  { "id": 49, "text": "What is the colour of wind?", "tone": "contemplative" },
  { "id": 50, "text": "Where does the flame go|when the candle is blown out?", "tone": "contemplative" },
  { "id": 51, "text": "The eye through which I see the universe|is the same eye through which|the universe sees me.", "tone": "contemplative" },
  { "id": 52, "text": "Before the first thought, what are you?", "tone": "contemplative" },
  { "id": 53, "text": "If not now, when?|If not here, where?", "tone": "contemplative" },
  { "id": 54, "text": "The gate is open.|Why do you stand at the threshold?", "tone": "contemplative" },
  { "id": 55, "text": "Between yes and no,|how much distance is there?", "tone": "contemplative" },
  { "id": 56, "text": "To study the self is to forget the self.", "tone": "contemplative" },
  { "id": 57, "text": "The present moment always will have been.", "tone": "contemplative" },
  { "id": 58, "text": "The ten thousand things|arise and return|to the one.", "tone": "contemplative" },
  { "id": 59, "text": "To know that you do not know|is knowing.", "tone": "contemplative" },
  { "id": 60, "text": "All is impermanent.|That too is impermanent.", "tone": "contemplative" },
  { "id": 61, "text": "Why are you looking outside?|You are the source.", "tone": "contemplative" },
  { "id": 62, "text": "Realise the silence that is your true nature.", "tone": "contemplative" },
  { "id": 63, "text": "What is there before the first thought?", "tone": "contemplative" },
  { "id": 64, "text": "The drum has never beaten itself.|Who is the one who hears?", "tone": "contemplative" },
  { "id": 65, "text": "Not two. Not one.", "tone": "contemplative" },
  { "id": 66, "text": "Just this.", "tone": "contemplative" },
  { "id": 67, "text": "You are the sky.|Everything else is just the weather.", "tone": "contemplative" },
  { "id": 68, "text": "This moment will not return.|That is what makes it precious.", "tone": "contemplative" },
  { "id": 69, "text": "Between the tick and the tock,|where do you live?", "tone": "contemplative" },
  { "id": 70, "text": "What you are looking for|is what is looking.", "tone": "contemplative" },
  { "id": 71, "text": "Each moment contains|a thousand years of silence.", "tone": "contemplative" },
  { "id": 72, "text": "Nothing is missing.", "tone": "contemplative" },
  { "id": 73, "text": "If you cannot find the truth right where you are,|where else do you expect to find it?", "tone": "contemplative" },
  { "id": 74, "text": "Let the moment be what the moment is.", "tone": "contemplative" },
  { "id": 75, "text": "Expect nothing.|Be ready for everything.", "tone": "contemplative" },
  { "id": 76, "text": "Not speaking, not silent.", "tone": "contemplative" },
  { "id": 77, "text": "You are not a drop in the ocean.|You are the ocean in a drop.", "tone": "contemplative" },
  { "id": 78, "text": "The present moment is the only home|there has ever been.", "tone": "contemplative" },
  { "id": 79, "text": "Does a dog have Buddha-nature?|Mu.", "tone": "exchange" },
  { "id": 80, "text": "Three pounds of flax.", "tone": "exchange" },
  { "id": 81, "text": "Walk on.", "tone": "exchange" },
  { "id": 82, "text": "Empty your cup.", "tone": "exchange" },
  { "id": 83, "text": "Wash your bowl.", "tone": "exchange" },
  { "id": 84, "text": "When the iron cow moos, listen.", "tone": "exchange" },
  { "id": 85, "text": "Knock on the sky and listen to the sound.", "tone": "exchange" },
  { "id": 86, "text": "Form is emptiness.|Emptiness is form.", "tone": "exchange" },
  { "id": 87, "text": "Rest and be taken.", "tone": "exchange" },
  { "id": 88, "text": "Be a lamp unto yourself.", "tone": "exchange" },
  { "id": 89, "text": "Do not seek the truth.|Only cease to cherish your opinions.", "tone": "exchange" },
  { "id": 90, "text": "The gate of heaven is open|but the gateless gate is closer.", "tone": "exchange" },
  { "id": 91, "text": "A gate that cannot be passed through|is not a gate at all.", "tone": "exchange" },
  { "id": 92, "text": "Even the longest journey|begins with a single step|already taken.", "tone": "gentle" },
  { "id": 93, "text": "If you are facing in the right direction,|all you need to do is keep walking.", "tone": "gentle" },
  { "id": 94, "text": "The journey of a thousand miles|begins beneath your feet.", "tone": "gentle" },
  { "id": 95, "text": "To a beginner there are many possibilities.|To an expert there are few.", "tone": "exchange" },
  { "id": 96, "text": "What is the sound of one hand?", "tone": "contemplative" },
  { "id": 97, "text": "Even in the depths of winter|I know that within me|there is an invincible summer.", "tone": "gentle" },
  { "id": 98, "text": "A student asked: What is the greatest miracle?|The master poured tea and said:|That you are here to ask.", "tone": "exchange" },
  { "id": 99, "text": "A student asked: What is emptiness?|The master opened his hand.", "tone": "exchange" },
  { "id": 100, "text": "A student asked: What is Zen?|The master poured tea until the cup overflowed.", "tone": "exchange" },
  { "id": 101, "text": "Have you eaten your breakfast?|Yes.|Then go wash your bowl.", "tone": "exchange" },
  { "id": 102, "text": "A monk asked: What is Buddha?|The master replied: Three pounds of flax.|The monk stood in silence.|The master drank his tea.", "tone": "exchange" },
  { "id": 103, "text": "A student asked: What happens after death?|The master said: I don't know.|But you are a master!|Yes. But not a dead one.", "tone": "sharp" },
  { "id": 104, "text": "The master said: Before enlightenment I was depressed.|After enlightenment I was still depressed.|But the depression was two inches shorter.", "tone": "sharp" },
  { "id": 105, "text": "A student asked: What is Zen?|The master slapped him.|The student was grateful.", "tone": "sharp" },
  { "id": 106, "text": "If you meet the Buddha on the road, kill him.", "tone": "sharp" },
  { "id": 108, "text": "You cannot step into the same river twice.|The river agrees.", "tone": "gentle" }
]
```

Note: id `107` is intentionally absent (was a duplicate of id `57`). The id sequence has a one-element gap — this is fine because IDs are author metadata only; nothing in the runtime references them.

- [ ] **Step 2.2: Add the `<Link>` reference to `Sudoku.Tests/Sudoku.Tests.csproj`.**

The file already has a similar pattern for the sakura SVGs. Add a third `<None Include>` block within the existing `<ItemGroup>` that links cross-project assets:

```xml
  <ItemGroup>
    <None Include="..\Sudoku.App\Resources\Images\sakura_branch_light.svg"
          Link="sakura_branch_light.svg"
          Condition="Exists('..\Sudoku.App\Resources\Images\sakura_branch_light.svg')">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Include="..\Sudoku.App\Resources\Images\sakura_branch_dark.svg"
          Link="sakura_branch_dark.svg"
          Condition="Exists('..\Sudoku.App\Resources\Images\sakura_branch_dark.svg')">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Include="..\Sudoku.App\Resources\Raw\koans.json"
          Link="koans.json"
          Condition="Exists('..\Sudoku.App\Resources\Raw\koans.json')">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>
```

The `Condition="Exists(...)"` mirrors the existing SVG entries so a partial checkout doesn't fail to load the project.

- [ ] **Step 2.3: Write `Sudoku.Tests/KoansAssetTests.cs`.**

```csharp
using FluentAssertions;
using Sudoku.Core.Game;

namespace Sudoku.Tests;

public class KoansAssetTests
{
    private const string AssetFile = "koans.json";
    private static readonly HashSet<string> s_knownTones =
        new() { "gentle", "contemplative", "exchange", "sharp" };

    private static List<Koan> LoadKoans()
    {
        File.Exists(AssetFile).Should().BeTrue(
            "the <Link> in Sudoku.Tests.csproj should copy koans.json to the test output folder");
        var json = File.ReadAllText(AssetFile);
        return KoanParser.Parse(json);
    }

    [Test]
    public void KoansFile_AllEntriesHaveText()
    {
        var koans = LoadKoans();

        koans.Should().OnlyContain(k => !string.IsNullOrWhiteSpace(k.Text));
    }

    [Test]
    public void KoansFile_AllTonesAreKnown()
    {
        var koans = LoadKoans();

        var unknownTones = koans
            .Where(k => !s_knownTones.Contains(k.Tone))
            .Select(k => $"id={k.Id} tone='{k.Tone}'")
            .ToList();

        unknownTones.Should().BeEmpty(
            "every tone must be in {gentle, contemplative, exchange, sharp}; " +
            "if you're adding a new tone, update s_knownTones in this test deliberately");
    }

    [Test]
    public void KoansFile_AllIdsAreUnique()
    {
        var koans = LoadKoans();

        var duplicates = koans
            .GroupBy(k => k.Id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        duplicates.Should().BeEmpty();
    }

    [Test]
    public void KoansFile_PlayablePoolIsNotEmpty()
    {
        var koans = LoadKoans();
        var pool = KoanParser.FilterPlayablePool(koans);

        pool.Should().HaveCountGreaterThan(50,
            "filtering out sharp-tone koans must still leave a substantial pool");
    }

    [Test]
    public void KoansFile_HasSoftBreaks()
    {
        var koans = LoadKoans();

        koans.Should().Contain(k => k.Text.Contains('|'),
            "at least one koan must use a soft break — guards against bulk find-replace removing them");
    }
}
```

- [ ] **Step 2.4: Run the asset tests.**

```powershell
dotnet test Sudoku.Tests --filter "FullyQualifiedName~KoansAssetTests"
```

Expected: **5 tests pass.** If `KoansFile_AllTonesAreKnown` fails listing an unknown tone, you have a typo in the JSON — fix the JSON. If `KoansFile_AllIdsAreUnique` fails, you have a duplicate id — fix the JSON.

- [ ] **Step 2.5: Run the full test suite.**

```powershell
dotnet test
```

Expected: all tests pass (existing + 6 from Task 1 + 5 from Task 2).

- [ ] **Step 2.6: Commit.**

```powershell
git add Sudoku.App/Resources/Raw/koans.json Sudoku.Tests/Sudoku.Tests.csproj Sudoku.Tests/KoansAssetTests.cs
git commit -m "feat(app): ship koans.json asset and validate schema/content from tests"
```

---

## Task 3: `ShowKoans` setting plumbing

**Goal:** A persisted boolean setting (default `true`) plus a Settings-page toggle. The setting is read but not yet acted on (that's Task 6).

**Files:**
- Modify: `Sudoku.Core/Game/GameSettings.cs`
- Modify: `Sudoku.App/Services/SettingsService.cs`
- Modify: `Sudoku.App/ViewModels/SettingsViewModel.cs`
- Modify: `Sudoku.App/Views/SettingsPage.xaml`

### Steps

- [ ] **Step 3.1: Add `ShowKoans` to `Sudoku.Core/Game/GameSettings.cs`.**

The whole file becomes:

```csharp
namespace Sudoku.Core.Game;

public class GameSettings
{
    public bool IsDarkTheme { get; set; } = true;
    public bool HighlightRelatedCells { get; set; } = true;
    public bool HighlightSameNumbers { get; set; } = true;
    public bool ShowErrors { get; set; } = true;
    public bool ShowKoans { get; set; } = true;
}
```

- [ ] **Step 3.2: Add the load/save plumbing to `Sudoku.App/Services/SettingsService.cs`.**

The whole file becomes:

```csharp
using Sudoku.Core.Game;

namespace Sudoku.App.Services;

public class SettingsService
{
    private const string IsDarkThemeKey = "IsDarkTheme";
    private const string HighlightRelatedCellsKey = "HighlightRelatedCells";
    private const string HighlightSameNumbersKey = "HighlightSameNumbers";
    private const string ShowErrorsKey = "ShowErrors";
    private const string ShowKoansKey = "ShowKoans";

    public GameSettings Load()
    {
        return new GameSettings
        {
            IsDarkTheme = Preferences.Default.Get(IsDarkThemeKey, true),
            HighlightRelatedCells = Preferences.Default.Get(HighlightRelatedCellsKey, true),
            HighlightSameNumbers = Preferences.Default.Get(HighlightSameNumbersKey, true),
            ShowErrors = Preferences.Default.Get(ShowErrorsKey, true),
            ShowKoans = Preferences.Default.Get(ShowKoansKey, true)
        };
    }

    public void Save(GameSettings settings)
    {
        Preferences.Default.Set(IsDarkThemeKey, settings.IsDarkTheme);
        Preferences.Default.Set(HighlightRelatedCellsKey, settings.HighlightRelatedCells);
        Preferences.Default.Set(HighlightSameNumbersKey, settings.HighlightSameNumbers);
        Preferences.Default.Set(ShowErrorsKey, settings.ShowErrors);
        Preferences.Default.Set(ShowKoansKey, settings.ShowKoans);
    }
}
```

- [ ] **Step 3.3: Add the observable property and change-handler to `Sudoku.App/ViewModels/SettingsViewModel.cs`.**

In the existing `SettingsViewModel`, add a new observable property after `ShowErrors` and a matching `partial void` handler. Also add the assignment in `LoadSettings`. The relevant additions:

```csharp
    [ObservableProperty]
    public partial bool ShowKoans { get; set; } = true;
```

Add this property declaration after the existing `ShowErrors` property block.

In `LoadSettings()`, add:

```csharp
        ShowKoans = _settings.ShowKoans;
```

after the existing `ShowErrors = _settings.ShowErrors;` line.

Add the change handler with the other `partial void` handlers:

```csharp
    partial void OnShowKoansChanged(bool value) { _settings.ShowKoans = value; Save(); }
```

After the change, the file should look like this in full:

```csharp
using CommunityToolkit.Mvvm.ComponentModel;

using Sudoku.App.Services;
using Sudoku.Core.Game;

namespace Sudoku.App.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settingsService;
    private GameSettings _settings = new();

    [ObservableProperty]
    public partial bool IsDarkTheme { get; set; } = true;

    [ObservableProperty]
    public partial bool HighlightRelatedCells { get; set; } = true;

    [ObservableProperty]
    public partial bool HighlightSameNumbers { get; set; } = true;

    [ObservableProperty]
    public partial bool ShowErrors { get; set; } = true;

    [ObservableProperty]
    public partial bool ShowKoans { get; set; } = true;

    public SettingsViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;
        LoadSettings();
    }

    private void LoadSettings()
    {
        _settings = _settingsService.Load();
        IsDarkTheme = _settings.IsDarkTheme;
        HighlightRelatedCells = _settings.HighlightRelatedCells;
        HighlightSameNumbers = _settings.HighlightSameNumbers;
        ShowErrors = _settings.ShowErrors;
        ShowKoans = _settings.ShowKoans;
    }

    partial void OnIsDarkThemeChanged(bool value)
    {
        _settings.IsDarkTheme = value;
        SaveAndApplyTheme();
    }

    partial void OnHighlightRelatedCellsChanged(bool value) { _settings.HighlightRelatedCells = value; Save(); }
    partial void OnHighlightSameNumbersChanged(bool value) { _settings.HighlightSameNumbers = value; Save(); }
    partial void OnShowErrorsChanged(bool value) { _settings.ShowErrors = value; Save(); }
    partial void OnShowKoansChanged(bool value) { _settings.ShowKoans = value; Save(); }

    private void Save() => _settingsService.Save(_settings);

    private void SaveAndApplyTheme()
    {
        Save();
        if (Application.Current != null)
            Application.Current.UserAppTheme = _settings.IsDarkTheme ? AppTheme.Dark : AppTheme.Light;
    }
}
```

- [ ] **Step 3.4: Add the new toggle row to `Sudoku.App/Views/SettingsPage.xaml`.**

Insert the following block **immediately after** the existing "Show Errors" `<Grid>` row, still inside the `VerticalStackLayout` under the **GAMEPLAY** section header:

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

- [ ] **Step 3.5: Build the solution to verify no compile errors.**

```powershell
dotnet build
```

Expected: build succeeds for all installed TFMs. If you only have the Windows MAUI workload installed, the Android target may be skipped — that's fine for this task; we'll catch Android-specific issues during smoke test.

- [ ] **Step 3.6: Run the full test suite.**

```powershell
dotnet test
```

Expected: all tests pass (no behavior in the test project depends on `ShowKoans`, but verifying that the `GameSettings` change didn't break anything is cheap).

- [ ] **Step 3.7: Commit.**

```powershell
git add Sudoku.Core/Game/GameSettings.cs Sudoku.App/Services/SettingsService.cs Sudoku.App/ViewModels/SettingsViewModel.cs Sudoku.App/Views/SettingsPage.xaml
git commit -m "feat(app): add ShowKoans setting and Settings page toggle"
```

---

## Task 4: `KoanService`

**Goal:** A singleton service that lazy-loads `koans.json` once, parses it via `KoanParser`, filters out sharp-tone entries, and returns a random koan.

**Files:**
- Create: `Sudoku.App/Services/KoanService.cs`
- Modify: `Sudoku.App/MauiProgram.cs` (DI registration)

### Steps

- [ ] **Step 4.1: Create `Sudoku.App/Services/KoanService.cs`.**

```csharp
using Sudoku.Core.Game;

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

- [ ] **Step 4.2: Register `KoanService` as a singleton in `Sudoku.App/MauiProgram.cs`.**

In the existing `// Core services` block, add the new line. The relevant region after the change:

```csharp
        // Core services
        builder.Services.AddSingleton<ISolver, BitmaskSolver>();
        builder.Services.AddSingleton<ITechniqueSolver, TechniqueSolver>();
        builder.Services.AddSingleton<DifficultyGrader>();
        builder.Services.AddSingleton<SudokuGenerator>();
        builder.Services.AddSingleton<SettingsService>();
        builder.Services.AddSingleton<KoanService>();                                  // NEW
        builder.Services.AddSingleton(_ => new GamePersistenceService(FileSystem.AppDataDirectory));
```

- [ ] **Step 4.3: Build to verify.**

```powershell
dotnet build
```

Expected: build succeeds.

- [ ] **Step 4.4: Commit.**

```powershell
git add Sudoku.App/Services/KoanService.cs Sudoku.App/MauiProgram.cs
git commit -m "feat(app): add KoanService for random koan selection"
```

---

## Task 5: `KoanViewModel` and `KoanPage`

**Goal:** The MVVM pair that renders a koan and handles the **Begin** transition. Shell route registered.

**Files:**
- Create: `Sudoku.App/ViewModels/KoanViewModel.cs`
- Create: `Sudoku.App/Views/KoanPage.xaml`
- Create: `Sudoku.App/Views/KoanPage.xaml.cs`
- Modify: `Sudoku.App/MauiProgram.cs` (register VM + Page)
- Modify: `Sudoku.App/AppShell.xaml.cs` (register route)

### Steps

- [ ] **Step 5.1: Create `Sudoku.App/ViewModels/KoanViewModel.cs`.**

```csharp
using System.Text.Json;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Sudoku.App.Services;

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
        catch (Exception ex) when (ex is IOException or JsonException or FileNotFoundException or InvalidOperationException)
        {
            // Asset missing or corrupt — bail straight to the puzzle.
            // The koan is decoration; failing to render one must not block play.
            // Nest the recovery nav in its own try/catch so a navigation failure
            // here cannot leak out of OnAppearing's async void.
            try { await Shell.Current.GoToAsync($"../{nameof(Views.GamePage)}"); }
            catch { /* last resort — leave user on koan page; Begin button still works */ }
        }
    }

    [RelayCommand]
    private static async Task Begin() =>
        await Shell.Current.GoToAsync($"../{nameof(Views.GamePage)}");
}
```

- [ ] **Step 5.2: Create `Sudoku.App/Views/KoanPage.xaml`.**

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:vm="clr-namespace:Sudoku.App.ViewModels"
             x:Class="Sudoku.App.Views.KoanPage"
             x:DataType="vm:KoanViewModel"
             Shell.NavBarIsVisible="False"
             BackgroundColor="{AppThemeBinding Light={StaticResource PageBackgroundLight}, Dark={StaticResource PageBackgroundDark}}">

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

- [ ] **Step 5.3: Create `Sudoku.App/Views/KoanPage.xaml.cs`.**

```csharp
using Sudoku.App.ViewModels;

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

- [ ] **Step 5.4: Register VM and Page in `Sudoku.App/MauiProgram.cs`.**

In the existing `// ViewModels` block, add `KoanViewModel`. In the existing `// Pages` block, add `KoanPage`. The relevant regions after the change:

```csharp
        // ViewModels
        builder.Services.AddTransient<MenuViewModel>();
        builder.Services.AddSingleton<GameViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<KoanViewModel>();                       // NEW

        // Pages
        builder.Services.AddTransient<MenuPage>();
        builder.Services.AddTransient<GamePage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<KoanPage>();                            // NEW
```

- [ ] **Step 5.5: Register the route in `Sudoku.App/AppShell.xaml.cs`.**

The whole file becomes:

```csharp
namespace Sudoku.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(Views.GamePage), typeof(Views.GamePage));
        Routing.RegisterRoute(nameof(Views.SettingsPage), typeof(Views.SettingsPage));
        Routing.RegisterRoute(nameof(Views.KoanPage), typeof(Views.KoanPage));
    }
}
```

- [ ] **Step 5.6: Build to verify.**

```powershell
dotnet build
```

Expected: build succeeds. If the XAML compiler complains about `BeginCommand` not being found, double-check the `[RelayCommand]` attribute on `Begin` — the toolkit generates `BeginCommand` from the method name `Begin`. If it complains about `KoanText` or `KoanFontSize` types not matching, check that `x:DataType="vm:KoanViewModel"` is set on the root `<ContentPage>`.

- [ ] **Step 5.7: Commit.**

```powershell
git add Sudoku.App/ViewModels/KoanViewModel.cs Sudoku.App/Views/KoanPage.xaml Sudoku.App/Views/KoanPage.xaml.cs Sudoku.App/MauiProgram.cs Sudoku.App/AppShell.xaml.cs
git commit -m "feat(app): add KoanViewModel and KoanPage with dynamic font sizing"
```

---

## Task 6: Wire `MenuViewModel` to navigate to KoanPage

**Goal:** Tapping a difficulty button navigates to `KoanPage` when `ShowKoans` is true, otherwise directly to `GamePage` as today.

**Files:**
- Modify: `Sudoku.App/ViewModels/MenuViewModel.cs`

### Steps

- [ ] **Step 6.1: Update `MenuViewModel` constructor and `NewGame` body.**

The whole file becomes:

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Sudoku.App.Services;
using Sudoku.Core;
using Sudoku.Core.Game;
using Sudoku.Core.Generators;
using Sudoku.Core.Solvers;

namespace Sudoku.App.ViewModels;

public partial class MenuViewModel : ObservableObject
{
    private readonly SudokuGenerator _generator;
    private readonly ISolver _solver;
    private readonly GamePersistenceService _persistenceService;
    private readonly GameViewModel _gameViewModel;
    private readonly SettingsService _settingsService;

    [ObservableProperty]
    public partial bool HasSavedGame { get; set; }

    [ObservableProperty]
    public partial string SavedGameInfo { get; set; } = "";

    [ObservableProperty]
    public partial bool IsGenerating { get; set; }

    public MenuViewModel(SudokuGenerator generator, ISolver solver,
        GamePersistenceService persistenceService, GameViewModel gameViewModel,
        SettingsService settingsService)
    {
        _generator = generator;
        _solver = solver;
        _persistenceService = persistenceService;
        _gameViewModel = gameViewModel;
        _settingsService = settingsService;
    }

    public async Task CheckForSavedGameAsync()
    {
        var state = await _persistenceService.LoadAsync();
        if (state != null)
        {
            HasSavedGame = true;
            var minutes = state.ElapsedSeconds / 60;
            var seconds = state.ElapsedSeconds % 60;
            SavedGameInfo = $"{state.Difficulty.DisplayName()} — {minutes}:{seconds:D2}";
        }
        else
        {
            HasSavedGame = false;
            SavedGameInfo = "";
        }
    }

    [RelayCommand]
    private async Task ContinueGame()
    {
        var state = await _persistenceService.LoadAsync();
        if (state == null) return;
        _gameViewModel.LoadState(state);
        await Shell.Current.GoToAsync(nameof(Views.GamePage));
    }

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

        var route = _settingsService.Load().ShowKoans
            ? nameof(Views.KoanPage)
            : nameof(Views.GamePage);
        await Shell.Current.GoToAsync(route);
    }
}
```

- [ ] **Step 6.2: Build to verify.**

```powershell
dotnet build
```

Expected: build succeeds. If DI complains at runtime that `MenuViewModel` cannot be constructed, double-check that `SettingsService` is still registered as a singleton (it has been since before this work — verify by searching `MauiProgram.cs` for `AddSingleton<SettingsService>()`).

- [ ] **Step 6.3: Run the full test suite.**

```powershell
dotnet test
```

Expected: all tests pass.

- [ ] **Step 6.4: Commit.**

```powershell
git add Sudoku.App/ViewModels/MenuViewModel.cs
git commit -m "feat(app): route new puzzles through koan page when ShowKoans is enabled"
```

---

## Task 7: Smoke test on Windows + Android, light + dark theme

**Goal:** End-to-end verification on both target platforms in both themes. Catches anything not covered by unit tests (XAML rendering, Shell navigation, `Preferences.Default` persistence, file-asset deployment).

**No code changes in this task by default.** If any check fails, file the issue, fix it, and re-run the affected check.

### Steps

- [ ] **Step 7.1: Run the Windows MAUI build.**

```powershell
dotnet build Sudoku.App -f net10.0-windows10.0.19041.0 -t:Run
```

Expected: app launches, shows the menu with the cherry-blossom motif.

- [ ] **Step 7.2: Run the seven smoke checks on Windows in dark theme (default).**

| # | Check | Expected |
|---|---|---|
| 1 | Tap any difficulty button | Brief generation spinner, then the KoanPage appears with cherry-blossom in dark tones, koan centered, **Begin** button anchored at the bottom |
| 2 | Tap **Begin** | Puzzle loads. Tap the back affordance on the puzzle page → returns to menu (not koan) |
| 3 | Open Settings → toggle **Zen Koans** off → return to menu → tap a difficulty | Puzzle loads directly, no koan page in between |
| 4 | Toggle **Zen Koans** back on, close the app, relaunch | Toggle is still on (Preferences persisted) |
| 5 | Start ten new puzzles in succession | Koans visibly vary; sharp-tone koans (ids 103, 104, 105, 106) never appear |
| 6 | (Debug-only, optional) Rename `Resources/Raw/koans.json` to `koans-broken.json` and start a new puzzle | App falls through to puzzle without a crash; rename back after testing |
| 7 | Inspect a 1-line koan (e.g. *"Just this."*) and a 4-line koan (e.g. id 102) | Short feels weighty (32pt); long stays readable (20pt); neither crowds the **Begin** button |

- [ ] **Step 7.3: Toggle to light theme via Settings → Dark Theme = off, repeat checks 1, 2, 5, 7 in light theme on Windows.**

Expected: all readable, no contrast issues. KoanPage background uses `PageBackgroundLight`; text uses `#3d3632`; sakura overlay uses `sakura_branch_light.png`. The watercolor light overlay should be subtle.

- [ ] **Step 7.4: Run the Android MAUI build (emulator or device).**

```powershell
dotnet build Sudoku.App -f net10.0-android
```

Then deploy and launch via your usual Android workflow (VS / VS Code MAUI extension, or `dotnet build Sudoku.App -f net10.0-android -t:Run` if your machine is configured for it).

- [ ] **Step 7.5: Repeat checks 1–7 on Android in dark theme.**

Pay special attention to check 2 (the compound `"../GamePage"` route) — Shell's behavior on Android is the primary reason we chose this route over `//` or `RemovePage`. There should be **no flicker** between KoanPage and GamePage; the transition should be a single page-push animation.

- [ ] **Step 7.6: Toggle to light theme on Android, repeat checks 1, 2, 5, 7.**

- [ ] **Step 7.7: If everything passes, commit any cumulative fixes (if any). If no fixes were needed, no commit in this step.**

```powershell
git status
# If clean: nothing to commit; smoke test complete.
# If dirty: commit fixes with messages describing what failed and the fix.
```

- [ ] **Step 7.8: (Optional) Open a PR.**

```powershell
git push -u origin feat/zen-koan-loading-screen
gh pr create --title "feat: zen koan loading screen" --body "$(cat <<'EOF'
## Summary
- New full-screen `KoanPage` displays a randomly-selected zen koan between the main menu and a new puzzle, with **Begin** advancing to the puzzle via Shell's compound route so the koan never appears in the back stack.
- New `ShowKoans` setting (default `true`) on the Settings page bypasses the koan when off.
- Pure parsing/filtering logic in `Sudoku.Core/Game/`; MAUI-specific glue in `Sudoku.App/`.
- Sharp-tone koans excluded from the active pool by default; retained in JSON as future-toggleable content.
- Per-koan font sizing by line count so short koans feel weighty and long koans stay readable.

## Test plan
- [x] `dotnet test` — 11 new tests (`KoanParserTests`, `KoansAssetTests`) pass; all existing tests still pass
- [x] Windows MAUI smoke test (dark + light theme) — 7-check list from `docs/superpowers/plans/2026-05-01-zen-koan-loading-screen.md` Task 7
- [x] Android MAUI smoke test (dark + light theme) — same 7 checks

🤖 Generated with [Claude Code](https://claude.com/claude-code)
EOF
)"
```

---

## Self-review checklist for this plan

I'm running the writing-plans self-review against the spec.

**Spec coverage:**

| Spec section | Implemented in |
|---|---|
| §1 Asset (`koans.json`) | Task 2.1 |
| §2 Model + parser in `Sudoku.Core` | Task 1.3, 1.4 (with tests in 1.1) |
| §3 `KoanService` | Task 4.1 |
| §4 `KoanViewModel` (incl. font sizing, load-once guard, catch-all fallback) | Task 5.1 |
| §5 `KoanPage` XAML + code-behind | Task 5.2, 5.3 |
| §6 Navigation flow (compound route) | Task 5.1 (the `BeginCommand`), Task 6.1 (the menu branch) |
| §7 `MenuViewModel` constructor + branch | Task 6.1 |
| §8 Settings (model, service, VM, page row) | Task 3 (all four steps) |
| §9 DI and route registration | Task 4.2, 5.4, 5.5 |
| Testing → `KoanParserTests` | Task 1.1 |
| Testing → `KoansAssetTests` | Task 2.3 |
| Testing → `<Link>` to koans.json | Task 2.2 |
| Testing → manual smoke checklist | Task 7 (Steps 7.2 through 7.6) |

No gaps.

**Placeholder scan:** No "TBD", "TODO", or "implement later" markers. Every step has actual code or commands. Test methods are spelled out in full.

**Type consistency:** `Koan` properties (`Id`/`Text`/`Tone`) match across Core POCO, parser tests, asset tests, and JSON property names (case-insensitive deserializer). `KoanService.GetRandomAsync` signature matches `KoanViewModel.LoadAsync`'s call. `KoanFontSize` (double, default 26) matches XAML binding. `BeginCommand` matches the `Begin` method via the toolkit's source-generator naming. Route name `nameof(KoanPage)` is consistent across `AppShell.xaml.cs` registration, `MenuViewModel` push, and the directory the file is in (`Views/`).

Plan is ready.

---

**Plan complete and saved to `docs/superpowers/plans/2026-05-01-zen-koan-loading-screen.md`. Two execution options:**

**1. Subagent-Driven (recommended)** — I dispatch a fresh subagent per task, review between tasks, fast iteration. Best for plans with discrete, well-bounded tasks like this one (each task ends in a commit).

**2. Inline Execution** — I execute tasks in this session using executing-plans, batch execution with checkpoints for review. Better if you want to be in the loop on every step.

**Which approach?**
