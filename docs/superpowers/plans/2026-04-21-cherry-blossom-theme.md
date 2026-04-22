# Cherry Blossom Theme Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a subtle sakura watercolor branch to the top-right corner of every screen plus splash, and shift the board's row/column/box highlight from warm cream to a near-imperceptible pink.

**Architecture:** Two static SVG files (light/dark variants with identical geometry) are dropped into `Sudoku.App/Resources/Images/`, picked up by MAUI's resizetizer glob, and referenced from each page via `AppThemeBinding` on an `<Image>` element. A parity test in `Sudoku.Tests` loads both SVGs and asserts that every element/attribute other than color/opacity values matches.

**Tech Stack:** MAUI (`net10.0-android` / `net10.0-windows10.0.19041.0`), XAML, C# 12, NUnit 4, FluentAssertions, `System.Xml.Linq`.

**Spec:** `docs/superpowers/specs/2026-04-21-cherry-blossom-theme-design.md`

---

## File Structure

**Create:**

- `Sudoku.App/Resources/Images/sakura_branch_light.svg` — watercolor branch for light theme.
- `Sudoku.App/Resources/Images/sakura_branch_dark.svg` — same geometry, dusty-rose palette + lower root opacity.
- `Sudoku.Tests/SakuraBranchParityTests.cs` — enforces geometry parity between the two SVGs.

**Modify:**

- `Sudoku.App/Controls/SudokuBoardDrawable.cs` — two `static readonly Color` fields.
- `Sudoku.App/Views/MenuPage.xaml` — one new `<Image>` element.
- `Sudoku.App/Views/SettingsPage.xaml` — one new `<Image>` element.
- `Sudoku.App/Views/GamePage.xaml` — one new `<Image>` element.
- `Sudoku.App/Resources/Splash/splash.svg` — branch paths inserted behind the existing Ensō.
- `Sudoku.Tests/Sudoku.Tests.csproj` — copy the two SVGs into test output.

---

## Task 1: Wire SVG files into the test project's output

**Files:**
- Modify: `Sudoku.Tests/Sudoku.Tests.csproj`

- [ ] **Step 1: Add the two SVGs to the test project's `None` item group so they are copied next to the test assembly at build time**

Open `Sudoku.Tests/Sudoku.Tests.csproj`. Find the existing `<ItemGroup>` that contains `sudokus.json` / `sudokus.txt`:

```xml
<ItemGroup>
  <None Update="sudokus.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
  <None Update="sudokus.txt">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

Add a new `<ItemGroup>` immediately below it:

```xml
<ItemGroup>
  <None Include="..\Sudoku.App\Resources\Images\sakura_branch_light.svg" Link="sakura_branch_light.svg">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
  <None Include="..\Sudoku.App\Resources\Images\sakura_branch_dark.svg" Link="sakura_branch_dark.svg">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

The `Link` attribute ensures the SVG lands at the root of the test output directory (not inside a nested `Resources/Images/` subtree).

- [ ] **Step 2: Verify the project still builds even though the SVGs don't exist yet**

Run:

```bash
dotnet build Sudoku.Tests
```

Expected: build succeeds. The `None Include=…` with a missing file emits a warning (`MSB3246` or similar), not an error — that's fine for now because Task 3 and Task 4 will create the files. If the build fails outright, re-check the csproj syntax.

- [ ] **Step 3: Commit**

```bash
git add Sudoku.Tests/Sudoku.Tests.csproj
git commit -m "test: wire sakura branch SVGs into test output"
```

---

## Task 2: Write the SVG parity test (red)

**Files:**
- Create: `Sudoku.Tests/SakuraBranchParityTests.cs`

- [ ] **Step 1: Create the test file**

Create `Sudoku.Tests/SakuraBranchParityTests.cs` with this exact content:

```csharp
using System.Xml.Linq;

namespace Sudoku.Tests;

public class SakuraBranchParityTests
{
    private static readonly HashSet<string> s_colorAttributes = new(StringComparer.Ordinal)
    {
        "fill",
        "stroke",
        "opacity",
        "stop-color",
        "stop-opacity",
        "stdDeviation"
    };

    [Test]
    public void LightAndDarkSvgsShouldHaveIdenticalGeometry()
    {
        var baseDir = TestContext.CurrentContext.TestDirectory;
        var lightPath = Path.Combine(baseDir, "sakura_branch_light.svg");
        var darkPath = Path.Combine(baseDir, "sakura_branch_dark.svg");

        File.Exists(lightPath).Should().BeTrue($"missing SVG: {lightPath}");
        File.Exists(darkPath).Should().BeTrue($"missing SVG: {darkPath}");

        var light = XDocument.Load(lightPath);
        var dark = XDocument.Load(darkPath);

        AssertElementParity(light.Root!, dark.Root!, "/" + light.Root!.Name.LocalName);
    }

    private static void AssertElementParity(XElement light, XElement dark, string path)
    {
        light.Name.LocalName.Should().Be(dark.Name.LocalName,
            $"element name differs at {path}");

        var lightAttrs = light.Attributes()
            .Where(a => !s_colorAttributes.Contains(a.Name.LocalName))
            .OrderBy(a => a.Name.LocalName, StringComparer.Ordinal)
            .ToList();
        var darkAttrs = dark.Attributes()
            .Where(a => !s_colorAttributes.Contains(a.Name.LocalName))
            .OrderBy(a => a.Name.LocalName, StringComparer.Ordinal)
            .ToList();

        lightAttrs.Select(a => a.Name.LocalName).Should().Equal(
            darkAttrs.Select(a => a.Name.LocalName),
            $"geometry attribute set differs at {path}");

        for (var i = 0; i < lightAttrs.Count; i++)
        {
            lightAttrs[i].Value.Should().Be(darkAttrs[i].Value,
                $"attribute {lightAttrs[i].Name.LocalName} differs at {path}");
        }

        var lightChildren = light.Elements().ToList();
        var darkChildren = dark.Elements().ToList();

        lightChildren.Count.Should().Be(darkChildren.Count,
            $"child count differs at {path}");

        for (var i = 0; i < lightChildren.Count; i++)
        {
            var childPath = $"{path}/{lightChildren[i].Name.LocalName}[{i}]";
            AssertElementParity(lightChildren[i], darkChildren[i], childPath);
        }
    }
}
```

- [ ] **Step 2: Run the test — expect it to fail (files don't exist yet)**

Run:

```bash
dotnet test Sudoku.Tests --filter "FullyQualifiedName~SakuraBranchParityTests"
```

Expected: **FAIL** with message like `missing SVG: .../sakura_branch_light.svg`. This confirms the test exercises the code path we care about.

- [ ] **Step 3: Commit**

```bash
git add Sudoku.Tests/SakuraBranchParityTests.cs
git commit -m "test: add SVG parity test for sakura branch assets"
```

---

## Task 3: Author `sakura_branch_light.svg`

**Files:**
- Create: `Sudoku.App/Resources/Images/sakura_branch_light.svg`

- [ ] **Step 1: Create the light-theme SVG**

Create `Sudoku.App/Resources/Images/sakura_branch_light.svg` with this exact content:

```xml
<?xml version="1.0" encoding="UTF-8" standalone="no"?>
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 600 900" opacity="0.55">
  <defs>
    <filter id="blur-branch" x="-10%" y="-10%" width="120%" height="120%">
      <feGaussianBlur stdDeviation="2" />
    </filter>
    <filter id="blur-bloom" x="-20%" y="-20%" width="140%" height="140%">
      <feGaussianBlur stdDeviation="4" />
    </filter>
  </defs>
  <path d="M 610 60 Q 520 140 480 220 Q 430 310 380 370 Q 340 410 280 440" fill="none" stroke="#a08778" stroke-width="5" filter="url(#blur-branch)" />
  <path d="M 510 165 Q 545 225 530 305" fill="none" stroke="#a08778" stroke-width="3.5" filter="url(#blur-branch)" />
  <g filter="url(#blur-bloom)">
    <circle cx="595" cy="65" r="22" fill="#f2cdd1" />
    <circle cx="610" cy="85" r="18" fill="#edb9c2" />
    <circle cx="580" cy="100" r="15" fill="#f2cdd1" />
    <circle cx="555" cy="140" r="19" fill="#edb9c2" />
    <circle cx="540" cy="158" r="14" fill="#f2cdd1" />
    <circle cx="510" cy="185" r="18" fill="#f2cdd1" />
    <circle cx="495" cy="200" r="13" fill="#edb9c2" />
    <circle cx="475" cy="240" r="19" fill="#f2cdd1" />
    <circle cx="490" cy="258" r="14" fill="#edb9c2" />
    <circle cx="460" cy="265" r="12" fill="#f2cdd1" />
    <circle cx="430" cy="305" r="17" fill="#edb9c2" />
    <circle cx="445" cy="322" r="13" fill="#f2cdd1" />
    <circle cx="395" cy="360" r="18" fill="#f2cdd1" />
    <circle cx="410" cy="378" r="14" fill="#edb9c2" />
    <circle cx="520" cy="260" r="16" fill="#f2cdd1" />
    <circle cx="532" cy="278" r="12" fill="#edb9c2" />
    <circle cx="345" cy="405" r="16" fill="#f2cdd1" />
    <circle cx="360" cy="420" r="12" fill="#edb9c2" />
    <circle cx="300" cy="440" r="14" fill="#edb9c2" />
    <circle cx="285" cy="452" r="11" fill="#f2cdd1" />
    <circle cx="560" cy="210" r="12" fill="#f2cdd1" />
    <circle cx="550" cy="222" r="9" fill="#edb9c2" />
  </g>
  <ellipse cx="505" cy="225" rx="14" ry="6" transform="rotate(25 505 225)" fill="#a8b89a" filter="url(#blur-branch)" />
  <ellipse cx="420" cy="342" rx="13" ry="6" transform="rotate(40 420 342)" fill="#a8b89a" filter="url(#blur-branch)" />
</svg>
```

- [ ] **Step 2: Run the parity test — still expect FAIL (dark SVG missing)**

Run:

```bash
dotnet test Sudoku.Tests --filter "FullyQualifiedName~SakuraBranchParityTests"
```

Expected: **FAIL** with `missing SVG: .../sakura_branch_dark.svg`. This is the expected red state — proves the test picked up the light file but still demands the dark one.

Do **not** commit yet — commit after Task 4 when the test passes.

---

## Task 4: Author `sakura_branch_dark.svg` with identical geometry

**Files:**
- Create: `Sudoku.App/Resources/Images/sakura_branch_dark.svg`

- [ ] **Step 1: Create the dark-theme SVG**

Create `Sudoku.App/Resources/Images/sakura_branch_dark.svg` with this exact content. Note: every `d`, `cx`, `cy`, `r`, `rx`, `ry`, `x`, `y`, `width`, `height`, `transform`, `stroke-width`, `filter`, and `id` attribute is identical to `sakura_branch_light.svg`. Only `opacity`, `fill`, `stroke`, and `stdDeviation` values differ:

```xml
<?xml version="1.0" encoding="UTF-8" standalone="no"?>
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 600 900" opacity="0.4">
  <defs>
    <filter id="blur-branch" x="-10%" y="-10%" width="120%" height="120%">
      <feGaussianBlur stdDeviation="2" />
    </filter>
    <filter id="blur-bloom" x="-20%" y="-20%" width="140%" height="140%">
      <feGaussianBlur stdDeviation="4" />
    </filter>
  </defs>
  <path d="M 610 60 Q 520 140 480 220 Q 430 310 380 370 Q 340 410 280 440" fill="none" stroke="#6a4f40" stroke-width="5" filter="url(#blur-branch)" />
  <path d="M 510 165 Q 545 225 530 305" fill="none" stroke="#6a4f40" stroke-width="3.5" filter="url(#blur-branch)" />
  <g filter="url(#blur-bloom)">
    <circle cx="595" cy="65" r="22" fill="#a87682" />
    <circle cx="610" cy="85" r="18" fill="#966374" />
    <circle cx="580" cy="100" r="15" fill="#a87682" />
    <circle cx="555" cy="140" r="19" fill="#966374" />
    <circle cx="540" cy="158" r="14" fill="#a87682" />
    <circle cx="510" cy="185" r="18" fill="#a87682" />
    <circle cx="495" cy="200" r="13" fill="#966374" />
    <circle cx="475" cy="240" r="19" fill="#a87682" />
    <circle cx="490" cy="258" r="14" fill="#966374" />
    <circle cx="460" cy="265" r="12" fill="#a87682" />
    <circle cx="430" cy="305" r="17" fill="#966374" />
    <circle cx="445" cy="322" r="13" fill="#a87682" />
    <circle cx="395" cy="360" r="18" fill="#a87682" />
    <circle cx="410" cy="378" r="14" fill="#966374" />
    <circle cx="520" cy="260" r="16" fill="#a87682" />
    <circle cx="532" cy="278" r="12" fill="#966374" />
    <circle cx="345" cy="405" r="16" fill="#a87682" />
    <circle cx="360" cy="420" r="12" fill="#966374" />
    <circle cx="300" cy="440" r="14" fill="#966374" />
    <circle cx="285" cy="452" r="11" fill="#a87682" />
    <circle cx="560" cy="210" r="12" fill="#a87682" />
    <circle cx="550" cy="222" r="9" fill="#966374" />
  </g>
  <ellipse cx="505" cy="225" rx="14" ry="6" transform="rotate(25 505 225)" fill="#6d7a5e" filter="url(#blur-branch)" />
  <ellipse cx="420" cy="342" rx="13" ry="6" transform="rotate(40 420 342)" fill="#6d7a5e" filter="url(#blur-branch)" />
</svg>
```

- [ ] **Step 2: Run the parity test — expect PASS**

Run:

```bash
dotnet test Sudoku.Tests --filter "FullyQualifiedName~SakuraBranchParityTests"
```

Expected: **PASS**. If it fails, compare the two SVG files attribute-by-attribute — every non-color attribute must be character-for-character identical.

- [ ] **Step 3: Commit both SVGs and the now-green test together**

```bash
git add Sudoku.App/Resources/Images/sakura_branch_light.svg Sudoku.App/Resources/Images/sakura_branch_dark.svg
git commit -m "feat: add sakura branch watercolor SVGs (light + dark themes)"
```

---

## Task 5: Shift the board's row/column/box highlight color

**Files:**
- Modify: `Sudoku.App/Controls/SudokuBoardDrawable.cs:16` and `:28`

- [ ] **Step 1: Update the two color constants**

Open `Sudoku.App/Controls/SudokuBoardDrawable.cs`. Find the dark-theme field on line 16:

```csharp
private static readonly Color s_darkRelatedCell = Color.FromArgb("#2a2520");
```

Change to:

```csharp
private static readonly Color s_darkRelatedCell = Color.FromArgb("#2e2224");
```

Find the light-theme field on line 28:

```csharp
private static readonly Color s_lightRelatedCell = Color.FromArgb("#ebe4db");
```

Change to:

```csharp
private static readonly Color s_lightRelatedCell = Color.FromArgb("#f0ddda");
```

No other edits — selected-cell, same-number, and error colors stay as-is.

- [ ] **Step 2: Build the solution**

Run:

```bash
dotnet build Sudoku.Core Sudoku.Tests
```

Expected: build succeeds. (The MAUI app project itself is not built here — it's built in the visual-verification task at the end.)

- [ ] **Step 3: Run the full test suite to confirm nothing regresses**

Run:

```bash
dotnet test
```

Expected: all existing tests pass plus the new parity test.

- [ ] **Step 4: Commit**

```bash
git add Sudoku.App/Controls/SudokuBoardDrawable.cs
git commit -m "feat: shift board related-cell highlight to blush pink"
```

---

## Task 6: Add sakura overlay to MenuPage

**Files:**
- Modify: `Sudoku.App/Views/MenuPage.xaml`

- [ ] **Step 1: Insert the Image as the first child of the outer Grid**

Open `Sudoku.App/Views/MenuPage.xaml`. Find the outer `<Grid>` element (currently opens with `<Grid RowDefinitions="*,Auto" Padding="24">`).

The existing structure is:

```xml
<Grid RowDefinitions="*,Auto" Padding="24">
    <VerticalStackLayout VerticalOptions="Center" Spacing="8">
        ...
    </VerticalStackLayout>

    <Button Text="&#9881; Settings" ... Grid.Row="1" ... />
</Grid>
```

Insert a new `<Image>` as the **first child** of the Grid (before `<VerticalStackLayout>`). Because MAUI Grid renders children in document order with later children on top, placing the Image first means it sits behind everything else automatically — no `ZIndex` needed:

```xml
<Grid RowDefinitions="*,Auto" Padding="24">
    <Image Source="{AppThemeBinding Light=sakura_branch_light.png, Dark=sakura_branch_dark.png}"
           Grid.RowSpan="2"
           InputTransparent="True"
           Aspect="AspectFit"
           HorizontalOptions="End"
           VerticalOptions="Start"
           WidthRequest="320"
           HeightRequest="480" />

    <VerticalStackLayout VerticalOptions="Center" Spacing="8">
        ...
    </VerticalStackLayout>

    <Button Text="&#9881; Settings" ... Grid.Row="1" ... />
</Grid>
```

The rest of the file is unchanged. `Padding="24"` on the Grid still applies to all children, so the Image has the same 24-pt inset as everything else.

- [ ] **Step 2: Commit**

```bash
git add Sudoku.App/Views/MenuPage.xaml
git commit -m "feat: add sakura branch overlay to MenuPage"
```

---

## Task 7: Add sakura overlay to SettingsPage

**Files:**
- Modify: `Sudoku.App/Views/SettingsPage.xaml`

- [ ] **Step 1: Insert the Image as the first child of the outer Grid**

Open `Sudoku.App/Views/SettingsPage.xaml`. Find the outer `<Grid RowDefinitions="Auto,*" Padding="24,16">` (around line 10).

Existing structure:

```xml
<Grid RowDefinitions="Auto,*" Padding="24,16">
    <HorizontalStackLayout Spacing="12">
        ...
    </HorizontalStackLayout>

    <ScrollView Grid.Row="1" Margin="0,16,0,0">
        ...
    </ScrollView>
</Grid>
```

Insert the Image as the first child:

```xml
<Grid RowDefinitions="Auto,*" Padding="24,16">
    <Image Source="{AppThemeBinding Light=sakura_branch_light.png, Dark=sakura_branch_dark.png}"
           Grid.RowSpan="2"
           InputTransparent="True"
           Aspect="AspectFit"
           HorizontalOptions="End"
           VerticalOptions="Start"
           WidthRequest="320"
           HeightRequest="480" />

    <HorizontalStackLayout Spacing="12">
        ...
    </HorizontalStackLayout>

    <ScrollView Grid.Row="1" Margin="0,16,0,0">
        ...
    </ScrollView>
</Grid>
```

- [ ] **Step 2: Commit**

```bash
git add Sudoku.App/Views/SettingsPage.xaml
git commit -m "feat: add sakura branch overlay to SettingsPage"
```

---

## Task 8: Add sakura overlay to GamePage (masked by the board)

**Files:**
- Modify: `Sudoku.App/Views/GamePage.xaml`

- [ ] **Step 1: Insert the Image as the first child of the outer Grid**

Open `Sudoku.App/Views/GamePage.xaml`. Find the outer `<Grid RowDefinitions="Auto,Auto,*,Auto,Auto,Auto" Padding="8">` (around line 10).

Insert the Image as the **first child** of the Grid — before the Row 0 nav bar Grid:

```xml
<Grid RowDefinitions="Auto,Auto,*,Auto,Auto,Auto" Padding="8">

    <!-- Sakura branch overlay — masked by the opaque board in Row 2 -->
    <Image Source="{AppThemeBinding Light=sakura_branch_light.png, Dark=sakura_branch_dark.png}"
           Grid.RowSpan="6"
           InputTransparent="True"
           Aspect="AspectFit"
           HorizontalOptions="End"
           VerticalOptions="Start"
           WidthRequest="320"
           HeightRequest="480" />

    <!-- Row 0: Navigation bar -->
    <Grid ColumnDefinitions="Auto,*,Auto" Padding="8,4">
        ...
```

The board's `GraphicsView` (Row 2) fills its cells with opaque colors in `SudokuBoardDrawable.DrawCellBackgrounds`, which naturally hides the branch anywhere the 9×9 grid overlaps. No explicit clipping logic needed.

Also keep the existing `WinOverlay` Grid (with `Grid.RowSpan="6"`) that comes **after** everything else — because it is a later child, it will continue to render on top of both the content and the new sakura Image, so the win celebration still works correctly.

- [ ] **Step 2: Commit**

```bash
git add Sudoku.App/Views/GamePage.xaml
git commit -m "feat: add sakura branch overlay to GamePage (masked by board)"
```

---

## Task 9: Add the sakura branch behind the splash Ensō

**Files:**
- Modify: `Sudoku.App/Resources/Splash/splash.svg`

- [ ] **Step 1: Rewrite the splash SVG with branch paths inserted before the Ensō paths**

Current content of `Sudoku.App/Resources/Splash/splash.svg`:

```xml
<?xml version="1.0" encoding="UTF-8" standalone="no"?>
<svg width="456" height="456" viewBox="0 0 456 456" xmlns="http://www.w3.org/2000/svg">
    <!-- Ensō — zen brush-stroke circle, larger for splash -->
    <!-- Center: (228, 228), radius: 120, gap at top-right -->

    <!-- Main body — bold confident stroke -->
    <path d="M 320 151 A 120 120 0 1 1 187 115"
          fill="none" stroke="white" stroke-width="28" stroke-linecap="round"/>

    <!-- Tapered tail — brush lifting gently -->
    <path d="M 187 115 A 120 120 0 0 1 249 110"
          fill="none" stroke="white" stroke-width="8" stroke-linecap="round"/>
</svg>
```

Replace the entire file with:

```xml
<?xml version="1.0" encoding="UTF-8" standalone="no"?>
<svg width="456" height="456" viewBox="0 0 456 456" xmlns="http://www.w3.org/2000/svg">
    <defs>
        <filter id="splashBlur" x="-20%" y="-20%" width="140%" height="140%">
            <feGaussianBlur stdDeviation="3" />
        </filter>
    </defs>

    <!-- Sakura branch — light palette, painted BEFORE the Ensō so it sits behind -->
    <path d="M 465 30 Q 410 70 380 120 Q 350 180 310 220"
          fill="none" stroke="#a08778" stroke-width="3" opacity="0.5" filter="url(#splashBlur)" />
    <g opacity="0.6" filter="url(#splashBlur)">
        <circle cx="455" cy="35" r="14" fill="#f2cdd1" />
        <circle cx="465" cy="50" r="12" fill="#edb9c2" />
        <circle cx="445" cy="60" r="10" fill="#f2cdd1" />
        <circle cx="410" cy="90" r="12" fill="#edb9c2" />
        <circle cx="420" cy="105" r="10" fill="#f2cdd1" />
        <circle cx="380" cy="135" r="11" fill="#f2cdd1" />
        <circle cx="395" cy="150" r="9" fill="#edb9c2" />
        <circle cx="350" cy="180" r="11" fill="#f2cdd1" />
        <circle cx="335" cy="195" r="9" fill="#edb9c2" />
        <circle cx="310" cy="215" r="10" fill="#f2cdd1" />
    </g>
    <ellipse cx="395" cy="125" rx="9" ry="4" transform="rotate(30 395 125)" fill="#a8b89a" opacity="0.55" filter="url(#splashBlur)" />

    <!-- Ensō — zen brush-stroke circle, larger for splash -->
    <!-- Center: (228, 228), radius: 120, gap at top-right -->
    <!-- Main body — bold confident stroke -->
    <path d="M 320 151 A 120 120 0 1 1 187 115"
          fill="none" stroke="white" stroke-width="28" stroke-linecap="round"/>

    <!-- Tapered tail — brush lifting gently -->
    <path d="M 187 115 A 120 120 0 0 1 249 110"
          fill="none" stroke="white" stroke-width="8" stroke-linecap="round"/>
</svg>
```

Note: the splash uses the light palette only. Reasoning: MAUI splash is shown before the app's theme preference is reliably applied, and the existing Ensō is white regardless — a dark-specific splash is out of scope (see spec: "Out of scope").

- [ ] **Step 2: Delete the resizetizer cache so the edited splash regenerates**

Per the CLAUDE.md gotcha on resizetizer caching, run:

```bash
find Sudoku.App/obj -type d -name resizetizer -exec rm -rf {} + 2>/dev/null || true
```

(On Windows bash this removes the cached platform splash images. If the directory doesn't exist yet, the command exits cleanly.)

- [ ] **Step 3: Commit**

```bash
git add Sudoku.App/Resources/Splash/splash.svg
git commit -m "feat: paint sakura branch behind Ensō on splash screen"
```

---

## Task 10: Build + visual verification

**Files:** no code changes — this is a human-judgement step.

- [ ] **Step 1: Clean build the MAUI app for Windows**

Run:

```bash
dotnet build Sudoku.App -f net10.0-windows10.0.19041.0
```

Expected: build succeeds. If the resizetizer errors on the new SVGs (missing filter support, filename casing, etc.), read the error carefully and fix in place.

- [ ] **Step 2: Launch the Windows app**

Run:

```bash
dotnet build Sudoku.App -f net10.0-windows10.0.19041.0 -t:Run
```

- [ ] **Step 3: Verify the four visual acceptance criteria**

Tick each when observed on Windows:

- [ ] Splash: cherry-blossom branch visible in the top-right corner behind the white Ensō; Ensō remains crisp on top.
- [ ] MenuPage: branch visible in top-right, subtle; the existing title, difficulty buttons, and settings gear all render unchanged and tappable.
- [ ] SettingsPage: branch visible in top-right; settings rows remain readable and interactive.
- [ ] GamePage: branch visible above the nav bar / info bar in the top-right, **does not** appear through the 9×9 board (board cells are fully opaque); below the number pad is clean (no branch by design).
- [ ] Select any cell in the board: row/column/box related cells tint a very subtle pink (`#f0ddda` in light, `#2e2224` in dark) — noticeably pink compared to the previous warm cream but not loud.
- [ ] Toggle dark theme (Settings → Dark Theme): branch switches to dusty rose and the board's related-cell tint shifts appropriately.

- [ ] **Step 4: If visuals need tuning, iterate on SVG coordinates / Image sizing**

Expected iterations:

- Branch sits in wrong part of page → adjust `WidthRequest` / `HeightRequest` on the `<Image>` in each View, or adjust the branch's coordinates inside the SVG (and mirror the change into the dark file — the parity test will catch a miss).
- Branch looks too strong or too faint → adjust the root `opacity` attribute on each SVG (may differ between light and dark — this is explicitly allowed by the parity test).
- Branch clips at filter edges → expand the filter region (`x="-20%" y="-20%" width="140%" height="140%"`) on `feGaussianBlur`.

After any SVG edit, re-run the parity test:

```bash
dotnet test Sudoku.Tests --filter "FullyQualifiedName~SakuraBranchParityTests"
```

- [ ] **Step 5: Smoke-test Android if available**

If an Android emulator is set up, run:

```bash
dotnet build Sudoku.App -f net10.0-android -t:Run
```

Walk through the same acceptance criteria. If Android rendering differs meaningfully (e.g. blur quality), note it but do not block merge — the feature still ships with light-theme-only splash by spec.

- [ ] **Step 6: Commit any tuning edits made in Step 4**

```bash
git add Sudoku.App/Resources/Images/ Sudoku.App/Views/ Sudoku.App/Resources/Splash/splash.svg
git commit -m "tune: adjust sakura branch sizing/opacity after visual review"
```

(Only commit if tuning was actually needed. If everything looked right first pass, skip this step.)

---

## Self-Review Notes

- **Spec coverage**: every section of the spec maps to a task — SVG assets (Tasks 3, 4), parity test (Tasks 1, 2), board highlight change (Task 5), page integration (Tasks 6, 7, 8), splash (Task 9), visual verification (Task 10). `Colors.xaml` explicitly not touched, as specified.
- **Rendering of `AppThemeBinding` filename**: the plan references `sakura_branch_light.png` / `sakura_branch_dark.png` in XAML because MAUI's resizetizer emits PNG outputs from SVG sources. If the build complains, try `sakura_branch_light` (no extension) — this is a known per-version quirk of MAUI image referencing. Adjust and re-run; not worth a follow-up task.
- **No placeholders**: every code block contains complete content; every command has expected output described.
- **Type/signature consistency**: all references to `s_lightRelatedCell` / `s_darkRelatedCell` match the existing field names in `SudokuBoardDrawable.cs` verified against the file.
- **Commit granularity**: one commit per task so a reviewer can bisect across the theme change cleanly.
