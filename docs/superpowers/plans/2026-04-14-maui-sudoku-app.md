# MAUI Sudoku App Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a .NET MAUI Sudoku game targeting Android and Windows with on-demand puzzle generation, candidate mode, undo, error checking, timer, save/resume, and dark/light themes.

**Architecture:** MVVM with CommunityToolkit.Mvvm. Game logic lives in model classes (testable without MAUI). The Sudoku grid renders via a custom `IDrawable` on a `GraphicsView`. ViewModels are thin orchestrators binding models to views. Services handle persistence via JSON files and MAUI Preferences.

**Tech Stack:** .NET 8.0 MAUI, CommunityToolkit.Mvvm, Sudoku.Core (existing library), NUnit + FluentAssertions (existing test infrastructure)

**Spec:** `docs/superpowers/specs/2026-04-14-maui-sudoku-app-design.md`

**Code Conventions (from `.editorconfig`):**
- Private fields: `_camelCase`; private static fields: `s_camelCase`
- Prefer `var` over explicit types
- Allman brace style (braces on new lines)
- File-scoped namespaces

---

## File Map

### New Files — Sudoku.App

| File | Responsibility |
|------|---------------|
| `Sudoku.App/Sudoku.App.csproj` | MAUI project targeting Android + Windows, references Sudoku.Core |
| `Sudoku.App/MauiProgram.cs` | DI registration, service configuration |
| `Sudoku.App/App.xaml` | Theme resource dictionaries (dark + light) |
| `Sudoku.App/App.xaml.cs` | Theme switching, navigation setup |
| `Sudoku.App/AppShell.xaml` | Shell navigation routes |
| `Sudoku.App/AppShell.xaml.cs` | Route registration |
| `Sudoku.App/Models/CellState.cs` | Per-cell state: value, candidates, excluded, isGiven, isError |
| `Sudoku.App/Models/UndoAction.cs` | Record: row, col, previousValue, previousCandidates, previousExcluded |
| `Sudoku.App/Models/GameState.cs` | Full game state: board, solution, cells, undo stack, timer, errors |
| `Sudoku.App/Models/GameSettings.cs` | User preferences: theme, highlight options |
| `Sudoku.App/Services/SettingsService.cs` | Read/write GameSettings via MAUI Preferences |
| `Sudoku.App/Services/GamePersistenceService.cs` | Serialize/deserialize GameState to JSON file |
| `Sudoku.App/Controls/SudokuBoardDrawable.cs` | IDrawable: grid lines, cell backgrounds, numbers, candidates |
| `Sudoku.App/ViewModels/GameViewModel.cs` | Game logic: input, undo, timer, error checking, auto-candidates |
| `Sudoku.App/ViewModels/MenuViewModel.cs` | Difficulty selection, puzzle generation, continue detection |
| `Sudoku.App/ViewModels/SettingsViewModel.cs` | Binds GameSettings to UI, persists changes |
| `Sudoku.App/Views/GamePage.xaml` | Game screen layout: grid + controls |
| `Sudoku.App/Views/GamePage.xaml.cs` | Touch handling, GraphicsView wiring |
| `Sudoku.App/Views/MenuPage.xaml` | Menu screen: title, continue, difficulty buttons |
| `Sudoku.App/Views/MenuPage.xaml.cs` | Navigation to GamePage/SettingsPage |
| `Sudoku.App/Views/SettingsPage.xaml` | Settings toggles UI |
| `Sudoku.App/Views/SettingsPage.xaml.cs` | Minimal code-behind |

### New Files — Sudoku.Tests

| File | Responsibility |
|------|---------------|
| `Sudoku.Tests/App/CellStateTests.cs` | CellState behavior tests |
| `Sudoku.Tests/App/GameStateTests.cs` | GameState logic tests: input, undo, candidates, errors, win |
| `Sudoku.Tests/App/GameSettingsTests.cs` | GameSettings defaults tests |

### Modified Files

| File | Change |
|------|--------|
| `Sudoku.sln` | Add Sudoku.App project reference |

---

## Task 1: Project Scaffold

**Files:**
- Create: `Sudoku.App/Sudoku.App.csproj`
- Create: `Sudoku.App/MauiProgram.cs`
- Create: `Sudoku.App/App.xaml`
- Create: `Sudoku.App/App.xaml.cs`
- Create: `Sudoku.App/AppShell.xaml`
- Create: `Sudoku.App/AppShell.xaml.cs`
- Create: `Sudoku.App/Platforms/Android/AndroidManifest.xml`
- Create: `Sudoku.App/Platforms/Android/MainActivity.cs`
- Create: `Sudoku.App/Platforms/Android/MainApplication.cs`
- Create: `Sudoku.App/Platforms/Windows/App.xaml`
- Create: `Sudoku.App/Platforms/Windows/App.xaml.cs`
- Create: `Sudoku.App/Platforms/Windows/Package.appxmanifest`
- Modify: `Sudoku.sln`

- [ ] **Step 1: Create the MAUI project from template**

```bash
cd D:/src/Github/sudoku
dotnet new maui -n Sudoku.App -o Sudoku.App
```

This generates the full MAUI scaffold with platform folders, resources, and boilerplate.

- [ ] **Step 2: Update the csproj to target only Android + Windows**

Replace the contents of `Sudoku.App/Sudoku.App.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFrameworks>net8.0-android;net8.0-windows10.0.19041.0</TargetFrameworks>
    <OutputType>Exe</OutputType>
    <RootNamespace>Sudoku.App</RootNamespace>
    <UseMaui>true</UseMaui>
    <SingleProject>true</SingleProject>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>

    <!-- Display name -->
    <ApplicationTitle>Sudoku</ApplicationTitle>

    <!-- App Identifier -->
    <ApplicationId>com.sudoku.app</ApplicationId>

    <!-- Versions -->
    <ApplicationDisplayVersion>1.0</ApplicationDisplayVersion>
    <ApplicationVersion>1</ApplicationVersion>

    <SupportedOSPlatformVersion Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'android'">21.0</SupportedOSPlatformVersion>
    <SupportedOSPlatformVersion Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'windows'">10.0.17763.0</SupportedOSPlatformVersion>
    <TargetPlatformMinVersion Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'windows'">10.0.17763.0</TargetPlatformMinVersion>
  </PropertyGroup>

  <ItemGroup>
    <MauiIcon Include="Resources\AppIcon\appicon.svg" ForegroundFile="Resources\AppIcon\appiconfg.svg" Color="#1a1a1a" />
    <MauiSplashScreen Include="Resources\Splash\splash.svg" Color="#1a1a1a" BaseSize="128,128" />
    <MauiImage Include="Resources\Images\*" />
    <MauiFont Include="Resources\Fonts\*" />
    <MauiAsset Include="Resources\Raw\**" LogicalName="%(RecursiveDir)%(Filename)%(Extension)" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.0" />
    <PackageReference Include="Microsoft.Maui.Controls" Version="$(MauiVersion)" />
    <PackageReference Include="Microsoft.Maui.Controls.Compatibility" Version="$(MauiVersion)" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Sudoku.Core\Sudoku.Core.csproj" />
  </ItemGroup>

</Project>
```

- [ ] **Step 3: Delete the iOS, MacCatalyst, and Tizen platform folders**

```bash
rm -rf Sudoku.App/Platforms/iOS Sudoku.App/Platforms/MacCatalyst Sudoku.App/Platforms/Tizen
```

- [ ] **Step 4: Replace App.xaml with minimal shell setup**

Replace `Sudoku.App/App.xaml`:

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<Application xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="Sudoku.App.App">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="Resources/Styles/Colors.xaml" />
                <ResourceDictionary Source="Resources/Styles/Styles.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

Replace `Sudoku.App/App.xaml.cs`:

```csharp
namespace Sudoku.App;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}
```

- [ ] **Step 5: Create AppShell with placeholder page**

Replace `Sudoku.App/AppShell.xaml`:

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<Shell xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
       xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
       xmlns:views="clr-namespace:Sudoku.App.Views"
       x:Class="Sudoku.App.AppShell"
       Shell.FlyoutBehavior="Disabled"
       Shell.NavBarIsVisible="False">

    <ShellContent ContentTemplate="{DataTemplate views:MenuPage}" />

</Shell>
```

Replace `Sudoku.App/AppShell.xaml.cs`:

```csharp
namespace Sudoku.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(Views.GamePage), typeof(Views.GamePage));
        Routing.RegisterRoute(nameof(Views.SettingsPage), typeof(Views.SettingsPage));
    }
}
```

- [ ] **Step 6: Create placeholder MenuPage so it builds**

Create `Sudoku.App/Views/MenuPage.xaml`:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="Sudoku.App.Views.MenuPage"
             Title="Sudoku">
    <VerticalStackLayout VerticalOptions="Center" HorizontalOptions="Center">
        <Label Text="SUDOKU" FontSize="32" FontAttributes="Bold" HorizontalTextAlignment="Center" />
    </VerticalStackLayout>
</ContentPage>
```

Create `Sudoku.App/Views/MenuPage.xaml.cs`:

```csharp
namespace Sudoku.App.Views;

public partial class MenuPage : ContentPage
{
    public MenuPage()
    {
        InitializeComponent();
    }
}
```

- [ ] **Step 7: Create placeholder GamePage and SettingsPage so AppShell routes resolve**

Create `Sudoku.App/Views/GamePage.xaml`:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="Sudoku.App.Views.GamePage"
             Title="Game">
    <Label Text="Game placeholder" VerticalOptions="Center" HorizontalTextAlignment="Center" />
</ContentPage>
```

Create `Sudoku.App/Views/GamePage.xaml.cs`:

```csharp
namespace Sudoku.App.Views;

public partial class GamePage : ContentPage
{
    public GamePage()
    {
        InitializeComponent();
    }
}
```

Create `Sudoku.App/Views/SettingsPage.xaml`:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="Sudoku.App.Views.SettingsPage"
             Title="Settings">
    <Label Text="Settings placeholder" VerticalOptions="Center" HorizontalTextAlignment="Center" />
</ContentPage>
```

Create `Sudoku.App/Views/SettingsPage.xaml.cs`:

```csharp
namespace Sudoku.App.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
    }
}
```

- [ ] **Step 8: Replace MauiProgram.cs with DI setup**

Replace `Sudoku.App/MauiProgram.cs`:

```csharp
using Sudoku.App.Services;
using Sudoku.App.ViewModels;
using Sudoku.App.Views;
using Sudoku.Core.Generators;
using Sudoku.Core.Solvers;

namespace Sudoku.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Core services
        builder.Services.AddSingleton<ISolver, SimpleSolver>();
        builder.Services.AddSingleton<SudokuGenerator>();
        builder.Services.AddSingleton<SettingsService>();
        builder.Services.AddSingleton<GamePersistenceService>();

        // ViewModels
        builder.Services.AddTransient<MenuViewModel>();
        builder.Services.AddTransient<GameViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();

        // Pages
        builder.Services.AddTransient<MenuPage>();
        builder.Services.AddTransient<GamePage>();
        builder.Services.AddTransient<SettingsPage>();

        return builder.Build();
    }
}
```

- [ ] **Step 9: Create stub service classes so DI resolves**

Create `Sudoku.App/Services/SettingsService.cs`:

```csharp
namespace Sudoku.App.Services;

public class SettingsService
{
}
```

Create `Sudoku.App/Services/GamePersistenceService.cs`:

```csharp
namespace Sudoku.App.Services;

public class GamePersistenceService
{
}
```

Create `Sudoku.App/ViewModels/MenuViewModel.cs`:

```csharp
namespace Sudoku.App.ViewModels;

public class MenuViewModel
{
}
```

Create `Sudoku.App/ViewModels/GameViewModel.cs`:

```csharp
namespace Sudoku.App.ViewModels;

public class GameViewModel
{
}
```

Create `Sudoku.App/ViewModels/SettingsViewModel.cs`:

```csharp
namespace Sudoku.App.ViewModels;

public class SettingsViewModel
{
}
```

- [ ] **Step 10: Add project to solution and delete template MainPage**

```bash
cd D:/src/Github/sudoku
dotnet sln add Sudoku.App/Sudoku.App.csproj
rm Sudoku.App/MainPage.xaml Sudoku.App/MainPage.xaml.cs
```

- [ ] **Step 11: Build and verify**

```bash
dotnet build Sudoku.App
```

Expected: Build succeeds with 0 errors. Warnings about unused stubs are fine.

- [ ] **Step 12: Commit**

```bash
git add Sudoku.App/ Sudoku.sln
git commit -m "feat: scaffold MAUI project with placeholder pages and DI setup"
```

---

## Task 2: Models — CellState and UndoAction

**Files:**
- Create: `Sudoku.App/Models/CellState.cs`
- Create: `Sudoku.App/Models/UndoAction.cs`
- Create: `Sudoku.Tests/App/CellStateTests.cs`

- [ ] **Step 1: Write CellState tests**

Create `Sudoku.Tests/App/CellStateTests.cs`:

```csharp
using FluentAssertions;

using Sudoku.App.Models;

namespace Sudoku.Tests.App;

[TestFixture]
public class CellStateTests
{
    [Test]
    public void NewCell_HasDefaultValues()
    {
        var cell = new CellState();

        cell.Value.Should().Be(0);
        cell.IsGiven.Should().BeFalse();
        cell.IsError.Should().BeFalse();
        cell.Candidates.Should().AllBeEquivalentTo(false);
        cell.ExcludedCandidates.Should().AllBeEquivalentTo(false);
    }

    [Test]
    public void HasValue_ReturnsTrueWhenNonZero()
    {
        var cell = new CellState { Value = 5 };

        cell.HasValue.Should().BeTrue();
    }

    [Test]
    public void HasValue_ReturnsFalseWhenZero()
    {
        var cell = new CellState();

        cell.HasValue.Should().BeFalse();
    }

    [Test]
    public void HasCandidate_ReturnsTrueForSetCandidate()
    {
        var cell = new CellState();
        cell.Candidates[2] = true; // candidate 3 (1-indexed → 0-indexed)

        cell.HasCandidate(3).Should().BeTrue();
    }

    [Test]
    public void HasCandidate_ReturnsFalseForUnsetCandidate()
    {
        var cell = new CellState();

        cell.HasCandidate(5).Should().BeFalse();
    }

    [Test]
    public void ToggleCandidate_FlipsCandidateState()
    {
        var cell = new CellState();

        cell.ToggleCandidate(7);
        cell.HasCandidate(7).Should().BeTrue();

        cell.ToggleCandidate(7);
        cell.HasCandidate(7).Should().BeFalse();
    }

    [Test]
    public void ClearCandidates_RemovesAllCandidates()
    {
        var cell = new CellState();
        cell.Candidates[0] = true;
        cell.Candidates[4] = true;
        cell.Candidates[8] = true;

        cell.ClearCandidates();

        cell.Candidates.Should().AllBeEquivalentTo(false);
    }

    [Test]
    public void CloneCandidates_ReturnsIndependentCopy()
    {
        var cell = new CellState();
        cell.Candidates[3] = true;

        var clone = cell.CloneCandidates();
        clone[3] = false;

        cell.Candidates[3].Should().BeTrue();
    }

    [Test]
    public void CloneExcludedCandidates_ReturnsIndependentCopy()
    {
        var cell = new CellState();
        cell.ExcludedCandidates[5] = true;

        var clone = cell.CloneExcludedCandidates();
        clone[5] = false;

        cell.ExcludedCandidates[5].Should().BeTrue();
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

```bash
dotnet test Sudoku.Tests --filter "FullyQualifiedName~CellStateTests"
```

Expected: Compilation error — `CellState` does not exist yet.

- [ ] **Step 3: Implement CellState**

Create `Sudoku.App/Models/CellState.cs`:

```csharp
namespace Sudoku.App.Models;

public class CellState
{
    public int Value { get; set; }
    public bool IsGiven { get; set; }
    public bool IsError { get; set; }
    public bool[] Candidates { get; set; } = new bool[9];
    public bool[] ExcludedCandidates { get; set; } = new bool[9];

    public bool HasValue => Value != 0;

    public bool HasCandidate(int number) => Candidates[number - 1];

    public void ToggleCandidate(int number)
    {
        Candidates[number - 1] = !Candidates[number - 1];
    }

    public void ClearCandidates()
    {
        Array.Clear(Candidates);
    }

    public bool[] CloneCandidates()
    {
        return (bool[])Candidates.Clone();
    }

    public bool[] CloneExcludedCandidates()
    {
        return (bool[])ExcludedCandidates.Clone();
    }
}
```

- [ ] **Step 4: Implement UndoAction**

Create `Sudoku.App/Models/UndoAction.cs`:

```csharp
namespace Sudoku.App.Models;

public record UndoAction(
    int Row,
    int Col,
    int PreviousValue,
    bool[] PreviousCandidates,
    bool[] PreviousExcludedCandidates);
```

- [ ] **Step 5: Add project reference from Tests to App**

The `CellState` lives in `Sudoku.App`, which is a MAUI project. MAUI projects can't be directly referenced from a standard test project. Instead, we need to add the test project reference conditionally or put the models in a shared location.

The simplest solution: add a project reference from Sudoku.Tests to Sudoku.App won't work because Sudoku.App targets `net8.0-android` and `net8.0-windows`, not `net8.0`.

**Solution:** Move the Models into Sudoku.Core since they're plain C# with no MAUI dependencies. This keeps them testable.

Move the model files to `Sudoku.Core/App/` namespace — but actually, let's keep it cleaner. The models `CellState`, `UndoAction`, `GameState`, and `GameSettings` are pure data classes with no MAUI dependencies. Place them directly in `Sudoku.App/Models/` but create a parallel set of test files that reference them via a `<ProjectReference>` with a specific `TargetFramework` condition.

**Better solution:** Since models are plain C# and we want them testable, put them in Sudoku.Core under a new namespace `Sudoku.Core.Game`:

Move files:
- `Sudoku.App/Models/CellState.cs` → `Sudoku.Core/Game/CellState.cs` (namespace `Sudoku.Core.Game`)
- `Sudoku.App/Models/UndoAction.cs` → `Sudoku.Core/Game/UndoAction.cs` (namespace `Sudoku.Core.Game`)

Update `Sudoku.Core/Game/CellState.cs`:

```csharp
namespace Sudoku.Core.Game;

public class CellState
{
    public int Value { get; set; }
    public bool IsGiven { get; set; }
    public bool IsError { get; set; }
    public bool[] Candidates { get; set; } = new bool[9];
    public bool[] ExcludedCandidates { get; set; } = new bool[9];

    public bool HasValue => Value != 0;

    public bool HasCandidate(int number) => Candidates[number - 1];

    public void ToggleCandidate(int number)
    {
        Candidates[number - 1] = !Candidates[number - 1];
    }

    public void ClearCandidates()
    {
        Array.Clear(Candidates);
    }

    public bool[] CloneCandidates()
    {
        return (bool[])Candidates.Clone();
    }

    public bool[] CloneExcludedCandidates()
    {
        return (bool[])ExcludedCandidates.Clone();
    }
}
```

Update `Sudoku.Core/Game/UndoAction.cs`:

```csharp
namespace Sudoku.Core.Game;

public record UndoAction(
    int Row,
    int Col,
    int PreviousValue,
    bool[] PreviousCandidates,
    bool[] PreviousExcludedCandidates);
```

Update the test file `using` to `Sudoku.Core.Game` instead of `Sudoku.App.Models`:

```csharp
using Sudoku.Core.Game;
```

- [ ] **Step 6: Run tests to verify they pass**

```bash
dotnet test Sudoku.Tests --filter "FullyQualifiedName~CellStateTests"
```

Expected: All 9 tests PASS.

- [ ] **Step 7: Commit**

```bash
git add Sudoku.Core/Game/ Sudoku.Tests/App/
git commit -m "feat: add CellState and UndoAction models with tests"
```

---

## Task 3: Models — GameSettings

**Files:**
- Create: `Sudoku.Core/Game/GameSettings.cs`
- Create: `Sudoku.Tests/App/GameSettingsTests.cs`

- [ ] **Step 1: Write GameSettings tests**

Create `Sudoku.Tests/App/GameSettingsTests.cs`:

```csharp
using FluentAssertions;

using Sudoku.Core.Game;

namespace Sudoku.Tests.App;

[TestFixture]
public class GameSettingsTests
{
    [Test]
    public void Defaults_HighlightRelatedCells_IsTrue()
    {
        var settings = new GameSettings();
        settings.HighlightRelatedCells.Should().BeTrue();
    }

    [Test]
    public void Defaults_HighlightSameNumbers_IsTrue()
    {
        var settings = new GameSettings();
        settings.HighlightSameNumbers.Should().BeTrue();
    }

    [Test]
    public void Defaults_ShowErrors_IsTrue()
    {
        var settings = new GameSettings();
        settings.ShowErrors.Should().BeTrue();
    }

    [Test]
    public void Defaults_AutoRemoveCandidates_IsFalse()
    {
        var settings = new GameSettings();
        settings.AutoRemoveCandidates.Should().BeFalse();
    }

    [Test]
    public void Defaults_IsDarkTheme_IsTrue()
    {
        var settings = new GameSettings();
        settings.IsDarkTheme.Should().BeTrue();
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

```bash
dotnet test Sudoku.Tests --filter "FullyQualifiedName~GameSettingsTests"
```

Expected: Compilation error — `GameSettings` does not exist.

- [ ] **Step 3: Implement GameSettings**

Create `Sudoku.Core/Game/GameSettings.cs`:

```csharp
namespace Sudoku.Core.Game;

public class GameSettings
{
    public bool IsDarkTheme { get; set; } = true;
    public bool HighlightRelatedCells { get; set; } = true;
    public bool HighlightSameNumbers { get; set; } = true;
    public bool ShowErrors { get; set; } = true;
    public bool AutoRemoveCandidates { get; set; }
}
```

- [ ] **Step 4: Run tests to verify they pass**

```bash
dotnet test Sudoku.Tests --filter "FullyQualifiedName~GameSettingsTests"
```

Expected: All 5 tests PASS.

- [ ] **Step 5: Commit**

```bash
git add Sudoku.Core/Game/GameSettings.cs Sudoku.Tests/App/GameSettingsTests.cs
git commit -m "feat: add GameSettings model with defaults and tests"
```

---

## Task 4: Models — GameState

**Files:**
- Create: `Sudoku.Core/Game/GameState.cs`
- Create: `Sudoku.Tests/App/GameStateTests.cs`

This is the core of the game logic. `GameState` manages the board, input, undo, candidates, error checking, and win detection.

- [ ] **Step 1: Write GameState initialization tests**

Create `Sudoku.Tests/App/GameStateTests.cs`:

```csharp
using FluentAssertions;

using Sudoku.Core;
using Sudoku.Core.Game;
using Sudoku.Core.Generators;
using Sudoku.Core.Solvers;

namespace Sudoku.Tests.App;

[TestFixture]
public class GameStateTests
{
    private static (SudokuBoard puzzle, SudokuBoard solution) GenerateTestPuzzle()
    {
        var solver = new SimpleSolver();
        var generator = new SudokuGenerator(solver, new Random(42));
        var puzzle = generator.Generate(Difficulty.Easy);
        var solution = solver.Solve(new SudokuBoard(puzzle));
        return (puzzle, solution);
    }

    [Test]
    public void Initialize_SetsCellsFromPuzzle()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);

        for (var r = 0; r < 9; r++)
        {
            for (var c = 0; c < 9; c++)
            {
                state.Cells[r, c].Value.Should().Be(puzzle[r, c]);
                if (puzzle[r, c] != 0)
                {
                    state.Cells[r, c].IsGiven.Should().BeTrue();
                }
                else
                {
                    state.Cells[r, c].IsGiven.Should().BeFalse();
                }
            }
        }
    }

    [Test]
    public void Initialize_SetsMetadata()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);

        state.Difficulty.Should().Be(Difficulty.Easy);
        state.ElapsedSeconds.Should().Be(0);
        state.ErrorCount.Should().Be(0);
    }

    [Test]
    public void PlaceNumber_SetsValueOnEmptyCell()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);
        var (r, c) = FindEmptyCell(state);
        var correctValue = solution[r, c];

        state.PlaceNumber(r, c, correctValue);

        state.Cells[r, c].Value.Should().Be(correctValue);
    }

    [Test]
    public void PlaceNumber_DoesNotChangeGivenCell()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);
        var (r, c) = FindGivenCell(state);
        var original = state.Cells[r, c].Value;

        state.PlaceNumber(r, c, 1);

        state.Cells[r, c].Value.Should().Be(original);
    }

    [Test]
    public void PlaceNumber_WrongValue_MarksError()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);
        var settings = new GameSettings { ShowErrors = true };
        var (r, c) = FindEmptyCell(state);
        var wrongValue = solution[r, c] == 9 ? 1 : solution[r, c] + 1;

        state.PlaceNumber(r, c, wrongValue, settings);

        state.Cells[r, c].IsError.Should().BeTrue();
        state.ErrorCount.Should().Be(1);
    }

    [Test]
    public void PlaceNumber_WrongValue_ShowErrorsOff_DoesNotMarkError()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);
        var settings = new GameSettings { ShowErrors = false };
        var (r, c) = FindEmptyCell(state);
        var wrongValue = solution[r, c] == 9 ? 1 : solution[r, c] + 1;

        state.PlaceNumber(r, c, wrongValue, settings);

        state.Cells[r, c].IsError.Should().BeFalse();
        state.ErrorCount.Should().Be(0);
    }

    [Test]
    public void PlaceNumber_CorrectValue_NoError()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);
        var settings = new GameSettings { ShowErrors = true };
        var (r, c) = FindEmptyCell(state);

        state.PlaceNumber(r, c, solution[r, c], settings);

        state.Cells[r, c].IsError.Should().BeFalse();
    }

    [Test]
    public void PlaceNumber_PushesUndoAction()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);
        var (r, c) = FindEmptyCell(state);

        state.PlaceNumber(r, c, solution[r, c]);

        state.UndoStack.Should().HaveCount(1);
        state.UndoStack.Peek().Row.Should().Be(r);
        state.UndoStack.Peek().Col.Should().Be(c);
        state.UndoStack.Peek().PreviousValue.Should().Be(0);
    }

    [Test]
    public void Undo_RestoresPreviousValue()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);
        var (r, c) = FindEmptyCell(state);

        state.PlaceNumber(r, c, solution[r, c]);
        state.Undo();

        state.Cells[r, c].Value.Should().Be(0);
        state.UndoStack.Should().BeEmpty();
    }

    [Test]
    public void ClearCell_RemovesValueAndPushesUndo()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);
        var (r, c) = FindEmptyCell(state);

        state.PlaceNumber(r, c, solution[r, c]);
        state.ClearCell(r, c);

        state.Cells[r, c].Value.Should().Be(0);
        state.UndoStack.Should().HaveCount(2);
    }

    [Test]
    public void ClearCell_DoesNotClearGivenCell()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);
        var (r, c) = FindGivenCell(state);
        var original = state.Cells[r, c].Value;

        state.ClearCell(r, c);

        state.Cells[r, c].Value.Should().Be(original);
    }

    [Test]
    public void ToggleCandidate_OnEmptyCell_TogglesCandidateAndPushesUndo()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);
        var (r, c) = FindEmptyCell(state);

        state.ToggleCandidate(r, c, 5);

        state.Cells[r, c].HasCandidate(5).Should().BeTrue();
        state.UndoStack.Should().HaveCount(1);
    }

    [Test]
    public void ToggleCandidate_OnCellWithValue_DoesNothing()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);
        var (r, c) = FindGivenCell(state);

        state.ToggleCandidate(r, c, 5);

        state.UndoStack.Should().BeEmpty();
    }

    [Test]
    public void ClearCandidates_RemovesAllAndPushesUndo()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);
        var (r, c) = FindEmptyCell(state);
        state.ToggleCandidate(r, c, 3);
        state.ToggleCandidate(r, c, 7);

        state.ClearCandidates(r, c);

        state.Cells[r, c].HasCandidate(3).Should().BeFalse();
        state.Cells[r, c].HasCandidate(7).Should().BeFalse();
    }

    [Test]
    public void ComputeAutoCandidates_FillsValidCandidates()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);
        var (r, c) = FindEmptyCell(state);

        state.ComputeAutoCandidates();

        // The cell should have at least one candidate (the solution value)
        state.Cells[r, c].HasCandidate(solution[r, c]).Should().BeTrue();
    }

    [Test]
    public void ComputeAutoCandidates_RespectsExcludedCandidates()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);
        var (r, c) = FindEmptyCell(state);
        var correctValue = solution[r, c];

        // Manually exclude the correct value
        state.Cells[r, c].ExcludedCandidates[correctValue - 1] = true;

        state.ComputeAutoCandidates();

        state.Cells[r, c].HasCandidate(correctValue).Should().BeFalse();
    }

    [Test]
    public void ClearAutoCandidates_RemovesCandidatesButKeepsExcluded()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);
        var (r, c) = FindEmptyCell(state);
        state.Cells[r, c].ExcludedCandidates[2] = true;

        state.ComputeAutoCandidates();
        state.ClearAutoCandidates();

        state.Cells[r, c].Candidates.Should().AllBeEquivalentTo(false);
        state.Cells[r, c].ExcludedCandidates[2].Should().BeTrue();
    }

    [Test]
    public void IsComplete_ReturnsFalse_WhenCellsEmpty()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);

        state.IsComplete().Should().BeFalse();
    }

    [Test]
    public void IsComplete_ReturnsTrue_WhenAllCorrect()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);

        // Fill all empty cells with correct values
        for (var r = 0; r < 9; r++)
        {
            for (var c = 0; c < 9; c++)
            {
                if (!state.Cells[r, c].IsGiven)
                {
                    state.PlaceNumber(r, c, solution[r, c]);
                }
            }
        }

        state.IsComplete().Should().BeTrue();
    }

    [Test]
    public void AutoRemoveCandidates_RemovesFromRelatedCells()
    {
        var (puzzle, solution) = GenerateTestPuzzle();
        var state = new GameState(puzzle, solution, Difficulty.Easy);
        var settings = new GameSettings { AutoRemoveCandidates = true };

        // First compute auto candidates so cells have candidates
        state.ComputeAutoCandidates();

        var (r, c) = FindEmptyCell(state);
        var value = solution[r, c];

        state.PlaceNumber(r, c, value, settings);

        // Check that the value was removed as a candidate from cells in same row
        for (var col = 0; col < 9; col++)
        {
            if (col != c && !state.Cells[r, col].HasValue)
            {
                state.Cells[r, col].HasCandidate(value).Should().BeFalse();
            }
        }
    }

    private static (int r, int c) FindEmptyCell(GameState state)
    {
        for (var r = 0; r < 9; r++)
        {
            for (var c = 0; c < 9; c++)
            {
                if (!state.Cells[r, c].IsGiven && state.Cells[r, c].Value == 0)
                {
                    return (r, c);
                }
            }
        }
        throw new InvalidOperationException("No empty cell found");
    }

    private static (int r, int c) FindGivenCell(GameState state)
    {
        for (var r = 0; r < 9; r++)
        {
            for (var c = 0; c < 9; c++)
            {
                if (state.Cells[r, c].IsGiven)
                {
                    return (r, c);
                }
            }
        }
        throw new InvalidOperationException("No given cell found");
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

```bash
dotnet test Sudoku.Tests --filter "FullyQualifiedName~GameStateTests"
```

Expected: Compilation error — `GameState` does not exist.

- [ ] **Step 3: Implement GameState**

Create `Sudoku.Core/Game/GameState.cs`:

```csharp
using Sudoku.Core.Solvers;

namespace Sudoku.Core.Game;

public class GameState
{
    public CellState[,] Cells { get; set; } = new CellState[9, 9];
    public int[][] Puzzle { get; set; } = Array.Empty<int[]>();
    public int[][] Solution { get; set; } = Array.Empty<int[]>();
    public Difficulty Difficulty { get; set; }
    public int ElapsedSeconds { get; set; }
    public int ErrorCount { get; set; }
    public Stack<UndoAction> UndoStack { get; set; } = new();

    // Parameterless constructor for JSON deserialization
    public GameState()
    {
        for (var r = 0; r < 9; r++)
        {
            for (var c = 0; c < 9; c++)
            {
                Cells[r, c] = new CellState();
            }
        }
    }

    public GameState(SudokuBoard puzzle, SudokuBoard solution, Difficulty difficulty) : this()
    {
        Difficulty = difficulty;
        Puzzle = BoardToJagged(puzzle);
        Solution = BoardToJagged(solution);

        for (var r = 0; r < 9; r++)
        {
            for (var c = 0; c < 9; c++)
            {
                Cells[r, c].Value = puzzle[r, c];
                Cells[r, c].IsGiven = puzzle[r, c] != 0;
            }
        }
    }

    public void PlaceNumber(int row, int col, int number, GameSettings? settings = null)
    {
        var cell = Cells[row, col];
        if (cell.IsGiven)
        {
            return;
        }

        PushUndo(row, col);

        cell.Value = number;
        cell.ClearCandidates();

        // Error checking
        if (settings?.ShowErrors == true)
        {
            cell.IsError = number != Solution[row][col];
            if (cell.IsError)
            {
                ErrorCount++;
            }
        }
        else
        {
            cell.IsError = false;
        }

        // Auto-remove candidates from related cells
        if (settings?.AutoRemoveCandidates == true)
        {
            RemoveCandidateFromRelatedCells(row, col, number);
        }
    }

    public void ClearCell(int row, int col)
    {
        var cell = Cells[row, col];
        if (cell.IsGiven)
        {
            return;
        }

        PushUndo(row, col);
        cell.Value = 0;
        cell.IsError = false;
    }

    public void ToggleCandidate(int row, int col, int number)
    {
        var cell = Cells[row, col];
        if (cell.IsGiven || cell.HasValue)
        {
            return;
        }

        PushUndo(row, col);
        cell.ToggleCandidate(number);

        // If user manually removes a candidate, track it in excluded
        if (!cell.HasCandidate(number))
        {
            cell.ExcludedCandidates[number - 1] = true;
        }
        else
        {
            cell.ExcludedCandidates[number - 1] = false;
        }
    }

    public void ClearCandidates(int row, int col)
    {
        var cell = Cells[row, col];
        if (cell.IsGiven || cell.HasValue)
        {
            return;
        }

        PushUndo(row, col);

        // Mark all current candidates as excluded
        for (var i = 0; i < 9; i++)
        {
            if (cell.Candidates[i])
            {
                cell.ExcludedCandidates[i] = true;
            }
        }
        cell.ClearCandidates();
    }

    public void Undo()
    {
        if (UndoStack.Count == 0)
        {
            return;
        }

        var action = UndoStack.Pop();
        var cell = Cells[action.Row, action.Col];
        cell.Value = action.PreviousValue;
        cell.IsError = false;
        Array.Copy(action.PreviousCandidates, cell.Candidates, 9);
        Array.Copy(action.PreviousExcludedCandidates, cell.ExcludedCandidates, 9);
    }

    public void ComputeAutoCandidates()
    {
        for (var r = 0; r < 9; r++)
        {
            for (var c = 0; c < 9; c++)
            {
                var cell = Cells[r, c];
                if (cell.IsGiven || cell.HasValue)
                {
                    continue;
                }

                for (var n = 1; n <= 9; n++)
                {
                    var isValid = !IsNumberInRow(r, n)
                                  && !IsNumberInColumn(c, n)
                                  && !IsNumberInBox(r, c, n);

                    cell.Candidates[n - 1] = isValid && !cell.ExcludedCandidates[n - 1];
                }
            }
        }
    }

    public void ClearAutoCandidates()
    {
        for (var r = 0; r < 9; r++)
        {
            for (var c = 0; c < 9; c++)
            {
                var cell = Cells[r, c];
                if (!cell.IsGiven && !cell.HasValue)
                {
                    cell.ClearCandidates();
                }
            }
        }
    }

    public bool IsComplete()
    {
        for (var r = 0; r < 9; r++)
        {
            for (var c = 0; c < 9; c++)
            {
                if (Cells[r, c].Value != Solution[r][c])
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void PushUndo(int row, int col)
    {
        var cell = Cells[row, col];
        UndoStack.Push(new UndoAction(
            row, col,
            cell.Value,
            cell.CloneCandidates(),
            cell.CloneExcludedCandidates()));
    }

    private void RemoveCandidateFromRelatedCells(int row, int col, int number)
    {
        for (var i = 0; i < 9; i++)
        {
            // Same row
            RemoveCandidateIfEmpty(row, i, number);
            // Same column
            RemoveCandidateIfEmpty(i, col, number);
        }

        // Same box
        var boxRow = (row / 3) * 3;
        var boxCol = (col / 3) * 3;
        for (var r = boxRow; r < boxRow + 3; r++)
        {
            for (var c = boxCol; c < boxCol + 3; c++)
            {
                RemoveCandidateIfEmpty(r, c, number);
            }
        }
    }

    private void RemoveCandidateIfEmpty(int row, int col, int number)
    {
        var cell = Cells[row, col];
        if (!cell.IsGiven && !cell.HasValue)
        {
            cell.Candidates[number - 1] = false;
            cell.ExcludedCandidates[number - 1] = true;
        }
    }

    private bool IsNumberInRow(int row, int number)
    {
        for (var c = 0; c < 9; c++)
        {
            if (Cells[row, c].Value == number)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsNumberInColumn(int col, int number)
    {
        for (var r = 0; r < 9; r++)
        {
            if (Cells[r, col].Value == number)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsNumberInBox(int row, int col, int number)
    {
        var boxRow = (row / 3) * 3;
        var boxCol = (col / 3) * 3;
        for (var r = boxRow; r < boxRow + 3; r++)
        {
            for (var c = boxCol; c < boxCol + 3; c++)
            {
                if (Cells[r, c].Value == number)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private static int[][] BoardToJagged(SudokuBoard board)
    {
        var jagged = new int[9][];
        for (var r = 0; r < 9; r++)
        {
            jagged[r] = new int[9];
            for (var c = 0; c < 9; c++)
            {
                jagged[r][c] = board[r, c];
            }
        }
        return jagged;
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

```bash
dotnet test Sudoku.Tests --filter "FullyQualifiedName~GameStateTests"
```

Expected: All 19 tests PASS.

- [ ] **Step 5: Commit**

```bash
git add Sudoku.Core/Game/GameState.cs Sudoku.Tests/App/GameStateTests.cs
git commit -m "feat: add GameState with input, undo, candidates, error checking, and win detection"
```

---

## Task 5: Services — SettingsService

**Files:**
- Create: `Sudoku.App/Services/SettingsService.cs` (replace stub)

The `SettingsService` wraps MAUI `Preferences` to persist `GameSettings`. This cannot be unit-tested outside MAUI (Preferences requires the platform), so we implement it directly without tests.

- [ ] **Step 1: Implement SettingsService**

Replace `Sudoku.App/Services/SettingsService.cs`:

```csharp
using Sudoku.Core.Game;

namespace Sudoku.App.Services;

public class SettingsService
{
    private const string IsDarkThemeKey = "IsDarkTheme";
    private const string HighlightRelatedCellsKey = "HighlightRelatedCells";
    private const string HighlightSameNumbersKey = "HighlightSameNumbers";
    private const string ShowErrorsKey = "ShowErrors";
    private const string AutoRemoveCandidatesKey = "AutoRemoveCandidates";

    public GameSettings Load()
    {
        return new GameSettings
        {
            IsDarkTheme = Preferences.Default.Get(IsDarkThemeKey, true),
            HighlightRelatedCells = Preferences.Default.Get(HighlightRelatedCellsKey, true),
            HighlightSameNumbers = Preferences.Default.Get(HighlightSameNumbersKey, true),
            ShowErrors = Preferences.Default.Get(ShowErrorsKey, true),
            AutoRemoveCandidates = Preferences.Default.Get(AutoRemoveCandidatesKey, false)
        };
    }

    public void Save(GameSettings settings)
    {
        Preferences.Default.Set(IsDarkThemeKey, settings.IsDarkTheme);
        Preferences.Default.Set(HighlightRelatedCellsKey, settings.HighlightRelatedCells);
        Preferences.Default.Set(HighlightSameNumbersKey, settings.HighlightSameNumbers);
        Preferences.Default.Set(ShowErrorsKey, settings.ShowErrors);
        Preferences.Default.Set(AutoRemoveCandidatesKey, settings.AutoRemoveCandidates);
    }
}
```

- [ ] **Step 2: Build to verify**

```bash
dotnet build Sudoku.App
```

Expected: Build succeeds.

- [ ] **Step 3: Commit**

```bash
git add Sudoku.App/Services/SettingsService.cs
git commit -m "feat: add SettingsService for persisting GameSettings via MAUI Preferences"
```

---

## Task 6: Services — GamePersistenceService

**Files:**
- Create: `Sudoku.App/Services/GamePersistenceService.cs` (replace stub)

Uses `System.Text.Json` to serialize `GameState` to a JSON file in `FileSystem.AppDataDirectory`. The `CellState[,]` 2D array and `Stack<UndoAction>` need a custom converter for JSON serialization.

- [ ] **Step 1: Implement GamePersistenceService**

Replace `Sudoku.App/Services/GamePersistenceService.cs`:

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;

using Sudoku.Core.Game;

namespace Sudoku.App.Services;

public class GamePersistenceService
{
    private static readonly JsonSerializerOptions s_options = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new CellStateArrayConverter(), new UndoStackConverter() }
    };

    private static string SavePath =>
        Path.Combine(FileSystem.AppDataDirectory, "game_state.json");

    public async Task SaveAsync(GameState state)
    {
        var json = JsonSerializer.Serialize(state, s_options);
        await File.WriteAllTextAsync(SavePath, json);
    }

    public async Task<GameState?> LoadAsync()
    {
        if (!File.Exists(SavePath))
        {
            return null;
        }

        var json = await File.ReadAllTextAsync(SavePath);
        return JsonSerializer.Deserialize<GameState>(json, s_options);
    }

    public void Delete()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
        }
    }

    public bool HasSavedGame() => File.Exists(SavePath);
}

/// <summary>
/// Converts CellState[,] to/from a flat JSON array of CellState objects (row-major order).
/// </summary>
public class CellStateArrayConverter : JsonConverter<CellState[,]>
{
    public override CellState[,] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var flat = JsonSerializer.Deserialize<CellState[]>(ref reader, options)
                   ?? Array.Empty<CellState>();
        var cells = new CellState[9, 9];
        for (var i = 0; i < flat.Length && i < 81; i++)
        {
            cells[i / 9, i % 9] = flat[i];
        }
        return cells;
    }

    public override void Write(Utf8JsonWriter writer, CellState[,] value, JsonSerializerOptions options)
    {
        var flat = new CellState[81];
        for (var r = 0; r < 9; r++)
        {
            for (var c = 0; c < 9; c++)
            {
                flat[r * 9 + c] = value[r, c];
            }
        }
        JsonSerializer.Serialize(writer, flat, options);
    }
}

/// <summary>
/// Converts Stack&lt;UndoAction&gt; to/from a JSON array (preserving stack order).
/// </summary>
public class UndoStackConverter : JsonConverter<Stack<UndoAction>>
{
    public override Stack<UndoAction> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var list = JsonSerializer.Deserialize<List<UndoAction>>(ref reader, options)
                   ?? new List<UndoAction>();
        // Reverse so the stack order is preserved (list[0] = top of stack)
        list.Reverse();
        return new Stack<UndoAction>(list);
    }

    public override void Write(Utf8JsonWriter writer, Stack<UndoAction> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.ToArray(), options);
    }
}
```

- [ ] **Step 2: Build to verify**

```bash
dotnet build Sudoku.App
```

Expected: Build succeeds.

- [ ] **Step 3: Commit**

```bash
git add Sudoku.App/Services/GamePersistenceService.cs
git commit -m "feat: add GamePersistenceService with JSON serialization and custom converters"
```

---

## Task 7: Controls — SudokuBoardDrawable

**Files:**
- Create: `Sudoku.App/Controls/SudokuBoardDrawable.cs`

This is the custom `IDrawable` that renders the Sudoku grid onto a `GraphicsView`. It reads from `GameState` and `GameSettings` to determine what to draw.

- [ ] **Step 1: Implement SudokuBoardDrawable**

Create `Sudoku.App/Controls/SudokuBoardDrawable.cs`:

```csharp
using Sudoku.Core.Game;

namespace Sudoku.App.Controls;

public class SudokuBoardDrawable : IDrawable
{
    public GameState? State { get; set; }
    public GameSettings Settings { get; set; } = new();
    public int SelectedRow { get; set; } = -1;
    public int SelectedCol { get; set; } = -1;
    public bool IsDarkTheme { get; set; } = true;

    // Dark theme colors
    private static readonly Color s_darkBackground = Color.FromArgb("#1a1a1a");
    private static readonly Color s_darkSelectedCell = Color.FromArgb("#2970c2");
    private static readonly Color s_darkRelatedCell = Color.FromArgb("#1a2640");
    private static readonly Color s_darkSameNumber = Color.FromArgb("#1e3520");
    private static readonly Color s_darkErrorCell = Color.FromArgb("#cc4444");
    private static readonly Color s_darkThinLine = Color.FromArgb("#444444");
    private static readonly Color s_darkThickLine = Color.FromArgb("#888888");
    private static readonly Color s_darkGivenText = Colors.White;
    private static readonly Color s_darkPlayerText = Color.FromArgb("#b0c4de");
    private static readonly Color s_darkCandidateText = Color.FromArgb("#888888");

    // Light theme colors
    private static readonly Color s_lightBackground = Color.FromArgb("#f0f0f0");
    private static readonly Color s_lightSelectedCell = Color.FromArgb("#5b9bd5");
    private static readonly Color s_lightRelatedCell = Color.FromArgb("#d6e4f0");
    private static readonly Color s_lightSameNumber = Color.FromArgb("#d5e8d4");
    private static readonly Color s_lightErrorCell = Color.FromArgb("#f4cccc");
    private static readonly Color s_lightThinLine = Color.FromArgb("#cccccc");
    private static readonly Color s_lightThickLine = Color.FromArgb("#333333");
    private static readonly Color s_lightGivenText = Color.FromArgb("#1a1a1a");
    private static readonly Color s_lightPlayerText = Color.FromArgb("#2970c2");
    private static readonly Color s_lightCandidateText = Color.FromArgb("#888888");

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (State == null)
        {
            return;
        }

        var size = Math.Min(dirtyRect.Width, dirtyRect.Height);
        var cellSize = size / 9f;
        var offsetX = (dirtyRect.Width - size) / 2f;
        var offsetY = (dirtyRect.Height - size) / 2f;

        DrawCellBackgrounds(canvas, cellSize, offsetX, offsetY);
        DrawGridLines(canvas, size, cellSize, offsetX, offsetY);
        DrawNumbers(canvas, cellSize, offsetX, offsetY);
    }

    private void DrawCellBackgrounds(ICanvas canvas, float cellSize, float offsetX, float offsetY)
    {
        var bg = IsDarkTheme ? s_darkBackground : s_lightBackground;
        var selected = IsDarkTheme ? s_darkSelectedCell : s_lightSelectedCell;
        var related = IsDarkTheme ? s_darkRelatedCell : s_lightRelatedCell;
        var sameNum = IsDarkTheme ? s_darkSameNumber : s_lightSameNumber;
        var error = IsDarkTheme ? s_darkErrorCell : s_lightErrorCell;

        var selectedValue = (SelectedRow >= 0 && SelectedCol >= 0)
            ? State!.Cells[SelectedRow, SelectedCol].Value
            : 0;

        for (var r = 0; r < 9; r++)
        {
            for (var c = 0; c < 9; c++)
            {
                var cell = State!.Cells[r, c];
                var color = GetCellColor(r, c, cell, selectedValue, bg, selected, related, sameNum, error);

                canvas.FillColor = color;
                canvas.FillRectangle(
                    offsetX + c * cellSize,
                    offsetY + r * cellSize,
                    cellSize,
                    cellSize);
            }
        }
    }

    private Color GetCellColor(
        int row, int col, CellState cell, int selectedValue,
        Color bg, Color selected, Color related, Color sameNum, Color error)
    {
        // Priority: Error > Selected > Same Number > Related > Default
        if (cell.IsError && Settings.ShowErrors)
        {
            return error;
        }

        if (row == SelectedRow && col == SelectedCol)
        {
            return selected;
        }

        if (Settings.HighlightSameNumbers && selectedValue != 0 && cell.Value == selectedValue)
        {
            return sameNum;
        }

        if (Settings.HighlightRelatedCells && SelectedRow >= 0 && SelectedCol >= 0)
        {
            var sameRow = row == SelectedRow;
            var sameCol = col == SelectedCol;
            var sameBox = (row / 3 == SelectedRow / 3) && (col / 3 == SelectedCol / 3);
            if (sameRow || sameCol || sameBox)
            {
                return related;
            }
        }

        return bg;
    }

    private void DrawGridLines(ICanvas canvas, float size, float cellSize, float offsetX, float offsetY)
    {
        var thinColor = IsDarkTheme ? s_darkThinLine : s_lightThinLine;
        var thickColor = IsDarkTheme ? s_darkThickLine : s_lightThickLine;

        // Thin lines
        canvas.StrokeColor = thinColor;
        canvas.StrokeSize = 1;
        for (var i = 1; i < 9; i++)
        {
            if (i % 3 == 0)
            {
                continue; // drawn as thick lines
            }

            // Vertical
            canvas.DrawLine(
                offsetX + i * cellSize, offsetY,
                offsetX + i * cellSize, offsetY + size);
            // Horizontal
            canvas.DrawLine(
                offsetX, offsetY + i * cellSize,
                offsetX + size, offsetY + i * cellSize);
        }

        // Thick lines (box boundaries + outer border)
        canvas.StrokeColor = thickColor;
        canvas.StrokeSize = 2;
        for (var i = 0; i <= 9; i += 3)
        {
            // Vertical
            canvas.DrawLine(
                offsetX + i * cellSize, offsetY,
                offsetX + i * cellSize, offsetY + size);
            // Horizontal
            canvas.DrawLine(
                offsetX, offsetY + i * cellSize,
                offsetX + size, offsetY + i * cellSize);
        }
    }

    private void DrawNumbers(ICanvas canvas, float cellSize, float offsetX, float offsetY)
    {
        var givenColor = IsDarkTheme ? s_darkGivenText : s_lightGivenText;
        var playerColor = IsDarkTheme ? s_darkPlayerText : s_lightPlayerText;
        var candidateColor = IsDarkTheme ? s_darkCandidateText : s_lightCandidateText;

        for (var r = 0; r < 9; r++)
        {
            for (var c = 0; c < 9; c++)
            {
                var cell = State!.Cells[r, c];
                var x = offsetX + c * cellSize;
                var y = offsetY + r * cellSize;

                if (cell.HasValue)
                {
                    DrawCellValue(canvas, cell, x, y, cellSize, givenColor, playerColor);
                }
                else
                {
                    DrawCandidates(canvas, cell, x, y, cellSize, candidateColor);
                }
            }
        }
    }

    private static void DrawCellValue(
        ICanvas canvas, CellState cell,
        float x, float y, float cellSize,
        Color givenColor, Color playerColor)
    {
        canvas.FontColor = cell.IsGiven ? givenColor : playerColor;
        canvas.FontSize = cellSize * 0.55f;
        canvas.Font = cell.IsGiven
            ? Microsoft.Maui.Graphics.Font.Default
            : Microsoft.Maui.Graphics.Font.Default;

        canvas.DrawString(
            cell.Value.ToString(),
            x, y, cellSize, cellSize,
            HorizontalAlignment.Center,
            VerticalAlignment.Center);
    }

    private static void DrawCandidates(
        ICanvas canvas, CellState cell,
        float x, float y, float cellSize,
        Color candidateColor)
    {
        canvas.FontColor = candidateColor;
        canvas.FontSize = cellSize * 0.25f;

        var miniSize = cellSize / 3f;
        for (var n = 1; n <= 9; n++)
        {
            if (!cell.HasCandidate(n))
            {
                continue;
            }

            var miniRow = (n - 1) / 3;
            var miniCol = (n - 1) % 3;
            var mx = x + miniCol * miniSize;
            var my = y + miniRow * miniSize;

            canvas.DrawString(
                n.ToString(),
                mx, my, miniSize, miniSize,
                HorizontalAlignment.Center,
                VerticalAlignment.Center);
        }
    }

    public (int row, int col) HitTest(PointF point, float viewWidth, float viewHeight)
    {
        var size = Math.Min(viewWidth, viewHeight);
        var cellSize = size / 9f;
        var offsetX = (viewWidth - size) / 2f;
        var offsetY = (viewHeight - size) / 2f;

        var col = (int)((point.X - offsetX) / cellSize);
        var row = (int)((point.Y - offsetY) / cellSize);

        if (row < 0 || row > 8 || col < 0 || col > 8)
        {
            return (-1, -1);
        }

        return (row, col);
    }
}
```

- [ ] **Step 2: Build to verify**

```bash
dotnet build Sudoku.App
```

Expected: Build succeeds.

- [ ] **Step 3: Commit**

```bash
git add Sudoku.App/Controls/SudokuBoardDrawable.cs
git commit -m "feat: add SudokuBoardDrawable with grid rendering, highlights, and candidates"
```

---

## Task 8: ViewModels — GameViewModel

**Files:**
- Modify: `Sudoku.App/ViewModels/GameViewModel.cs` (replace stub)

- [ ] **Step 1: Implement GameViewModel**

Replace `Sudoku.App/ViewModels/GameViewModel.cs`:

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Sudoku.App.Controls;
using Sudoku.App.Services;
using Sudoku.Core.Game;

namespace Sudoku.App.ViewModels;

public partial class GameViewModel : ObservableObject, IDisposable
{
    private readonly GamePersistenceService _persistenceService;
    private readonly SettingsService _settingsService;
    private System.Timers.Timer? _timer;

    [ObservableProperty]
    private GameState? _state;

    [ObservableProperty]
    private GameSettings _settings = new();

    [ObservableProperty]
    private int _selectedRow = -1;

    [ObservableProperty]
    private int _selectedCol = -1;

    [ObservableProperty]
    private bool _isNormalMode = true;

    [ObservableProperty]
    private bool _isAutoCandidateMode;

    [ObservableProperty]
    private string _timerText = "0:00";

    [ObservableProperty]
    private string _difficultyText = "";

    [ObservableProperty]
    private int _errorCount;

    [ObservableProperty]
    private bool _isGameComplete;

    public SudokuBoardDrawable Drawable { get; } = new();
    public Action? RequestInvalidate { get; set; }

    public GameViewModel(GamePersistenceService persistenceService, SettingsService settingsService)
    {
        _persistenceService = persistenceService;
        _settingsService = settingsService;
        Settings = _settingsService.Load();
    }

    public void LoadState(GameState state)
    {
        State = state;
        DifficultyText = state.Difficulty.ToString();
        ErrorCount = state.ErrorCount;
        UpdateTimerText();
        UpdateDrawable();
        StartTimer();
    }

    [RelayCommand]
    private void SelectCell(PointF point)
    {
        if (State == null || Drawable == null)
        {
            return;
        }

        // The actual view size is passed via command parameter workaround — 
        // we'll calculate from the drawable's last known size
        // For now, use the hit test with view dimensions passed separately
    }

    public void OnCellTapped(int row, int col)
    {
        if (State == null || IsGameComplete)
        {
            return;
        }

        SelectedRow = row;
        SelectedCol = col;
        UpdateDrawable();
    }

    [RelayCommand]
    private void NumberInput(int number)
    {
        if (State == null || SelectedRow < 0 || SelectedCol < 0 || IsGameComplete)
        {
            return;
        }

        if (IsNormalMode)
        {
            State.PlaceNumber(SelectedRow, SelectedCol, number, Settings);
            ErrorCount = State.ErrorCount;

            if (IsAutoCandidateMode)
            {
                State.ComputeAutoCandidates();
            }

            if (State.IsComplete())
            {
                IsGameComplete = true;
                StopTimer();
            }
        }
        else
        {
            State.ToggleCandidate(SelectedRow, SelectedCol, number);
        }

        UpdateDrawable();
        _ = SaveGameAsync();
    }

    [RelayCommand]
    private void ClearInput()
    {
        if (State == null || SelectedRow < 0 || SelectedCol < 0 || IsGameComplete)
        {
            return;
        }

        if (IsNormalMode)
        {
            State.ClearCell(SelectedRow, SelectedCol);

            if (IsAutoCandidateMode)
            {
                State.ComputeAutoCandidates();
            }
        }
        else
        {
            State.ClearCandidates(SelectedRow, SelectedCol);
        }

        UpdateDrawable();
        _ = SaveGameAsync();
    }

    [RelayCommand]
    private void UndoMove()
    {
        if (State == null || IsGameComplete)
        {
            return;
        }

        State.Undo();

        if (IsAutoCandidateMode)
        {
            State.ComputeAutoCandidates();
        }

        ErrorCount = State.ErrorCount;
        UpdateDrawable();
        _ = SaveGameAsync();
    }

    partial void OnIsNormalModeChanged(bool value)
    {
        // Nothing extra needed — the UI reflects the mode
    }

    partial void OnIsAutoCandidateModeChanged(bool value)
    {
        if (State == null)
        {
            return;
        }

        if (value)
        {
            State.ComputeAutoCandidates();
        }
        else
        {
            State.ClearAutoCandidates();
        }

        UpdateDrawable();
    }

    private void UpdateDrawable()
    {
        Drawable.State = State;
        Drawable.Settings = Settings;
        Drawable.SelectedRow = SelectedRow;
        Drawable.SelectedCol = SelectedCol;
        Drawable.IsDarkTheme = Settings.IsDarkTheme;
        RequestInvalidate?.Invoke();
    }

    private void StartTimer()
    {
        _timer?.Dispose();
        _timer = new System.Timers.Timer(1000);
        _timer.Elapsed += (_, _) =>
        {
            if (State == null)
            {
                return;
            }

            State.ElapsedSeconds++;
            MainThread.BeginInvokeOnMainThread(UpdateTimerText);
        };
        _timer.Start();
    }

    public void StopTimer()
    {
        _timer?.Stop();
    }

    public void ResumeTimer()
    {
        if (!IsGameComplete)
        {
            _timer?.Start();
        }
    }

    private void UpdateTimerText()
    {
        if (State == null)
        {
            return;
        }

        var minutes = State.ElapsedSeconds / 60;
        var seconds = State.ElapsedSeconds % 60;
        TimerText = $"{minutes}:{seconds:D2}";
    }

    private async Task SaveGameAsync()
    {
        if (State != null)
        {
            await _persistenceService.SaveAsync(State);
        }
    }

    public void RefreshSettings()
    {
        Settings = _settingsService.Load();
        UpdateDrawable();
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
```

- [ ] **Step 2: Build to verify**

```bash
dotnet build Sudoku.App
```

Expected: Build succeeds.

- [ ] **Step 3: Commit**

```bash
git add Sudoku.App/ViewModels/GameViewModel.cs
git commit -m "feat: add GameViewModel with input handling, timer, undo, and auto-candidates"
```

---

## Task 9: Views — GamePage

**Files:**
- Modify: `Sudoku.App/Views/GamePage.xaml` (replace placeholder)
- Modify: `Sudoku.App/Views/GamePage.xaml.cs` (replace placeholder)

- [ ] **Step 1: Implement GamePage.xaml**

Replace `Sudoku.App/Views/GamePage.xaml`:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:vm="clr-namespace:Sudoku.App.ViewModels"
             x:Class="Sudoku.App.Views.GamePage"
             x:DataType="vm:GameViewModel"
             Shell.NavBarIsVisible="False"
             BackgroundColor="{AppThemeBinding Light={StaticResource PageBackgroundLight}, Dark={StaticResource PageBackgroundDark}}">

    <Grid RowDefinitions="Auto,Auto,*,Auto,Auto,Auto" Padding="8">

        <!-- Row 0: Navigation bar -->
        <Grid ColumnDefinitions="Auto,*,Auto" Padding="8,4">
            <Button Text="&#9664; Back"
                    BackgroundColor="Transparent"
                    TextColor="{AppThemeBinding Light=#666666, Dark=#888888}"
                    FontSize="14"
                    Clicked="OnBackClicked"
                    Grid.Column="0" />
            <Button Text="&#9881;"
                    BackgroundColor="Transparent"
                    TextColor="{AppThemeBinding Light=#666666, Dark=#888888}"
                    FontSize="20"
                    Clicked="OnSettingsClicked"
                    Grid.Column="2" />
        </Grid>

        <!-- Row 1: Info bar -->
        <Grid ColumnDefinitions="*,*,*" Padding="8,4" Grid.Row="1">
            <VerticalStackLayout HorizontalOptions="Center">
                <Label Text="DIFFICULTY"
                       FontSize="10"
                       TextColor="{AppThemeBinding Light=#999999, Dark=#888888}"
                       HorizontalTextAlignment="Center" />
                <Label Text="{Binding DifficultyText}"
                       FontSize="16"
                       FontAttributes="Bold"
                       TextColor="{AppThemeBinding Light=#2970c2, Dark=#5b9bd5}"
                       HorizontalTextAlignment="Center" />
            </VerticalStackLayout>
            <VerticalStackLayout HorizontalOptions="Center" Grid.Column="1">
                <Label Text="TIME"
                       FontSize="10"
                       TextColor="{AppThemeBinding Light=#999999, Dark=#888888}"
                       HorizontalTextAlignment="Center" />
                <Label Text="{Binding TimerText}"
                       FontSize="16"
                       FontAttributes="Bold"
                       TextColor="{AppThemeBinding Light=#1a1a1a, Dark=#e8e8e8}"
                       HorizontalTextAlignment="Center" />
            </VerticalStackLayout>
            <VerticalStackLayout HorizontalOptions="Center" Grid.Column="2">
                <Label Text="ERRORS"
                       FontSize="10"
                       TextColor="{AppThemeBinding Light=#999999, Dark=#888888}"
                       HorizontalTextAlignment="Center" />
                <Label Text="{Binding ErrorCount}"
                       FontSize="16"
                       FontAttributes="Bold"
                       TextColor="{AppThemeBinding Light=#1a1a1a, Dark=#e8e8e8}"
                       HorizontalTextAlignment="Center" />
            </VerticalStackLayout>
        </Grid>

        <!-- Row 2: Sudoku grid -->
        <GraphicsView x:Name="BoardView"
                       Grid.Row="2"
                       HorizontalOptions="Center"
                       VerticalOptions="Center" />

        <!-- Row 3: Mode toggle + Undo -->
        <Grid ColumnDefinitions="*,Auto" Padding="12,8" Grid.Row="3">
            <HorizontalStackLayout Spacing="0">
                <Button Text="Normal"
                        WidthRequest="100"
                        HeightRequest="40"
                        FontSize="13"
                        CornerRadius="6"
                        BackgroundColor="{Binding IsNormalMode, Converter={StaticResource BoolToModeColorConverter}}"
                        TextColor="{Binding IsNormalMode, Converter={StaticResource BoolToModeTextConverter}}"
                        Clicked="OnNormalModeClicked" />
                <Button Text="Candidate"
                        WidthRequest="100"
                        HeightRequest="40"
                        FontSize="13"
                        CornerRadius="6"
                        BackgroundColor="{Binding IsNormalMode, Converter={StaticResource InverseBoolToModeColorConverter}}"
                        TextColor="{Binding IsNormalMode, Converter={StaticResource InverseBoolToModeTextConverter}}"
                        Clicked="OnCandidateModeClicked" />
            </HorizontalStackLayout>
            <Button Text="Undo"
                    WidthRequest="80"
                    HeightRequest="40"
                    FontSize="13"
                    CornerRadius="6"
                    BackgroundColor="{AppThemeBinding Light=#e0e0e0, Dark=#2a2a2a}"
                    TextColor="{AppThemeBinding Light=#666666, Dark=#888888}"
                    Command="{Binding UndoMoveCommand}"
                    Grid.Column="1" />
        </Grid>

        <!-- Row 4: Number pad -->
        <Grid ColumnDefinitions="*,*,*,*,*"
              RowDefinitions="Auto,Auto"
              ColumnSpacing="8"
              RowSpacing="8"
              Padding="12,4"
              Grid.Row="4">
            <Button Text="1" Style="{StaticResource NumberPadButton}" Clicked="OnNumberClicked" Grid.Row="0" Grid.Column="0" />
            <Button Text="2" Style="{StaticResource NumberPadButton}" Clicked="OnNumberClicked" Grid.Row="0" Grid.Column="1" />
            <Button Text="3" Style="{StaticResource NumberPadButton}" Clicked="OnNumberClicked" Grid.Row="0" Grid.Column="2" />
            <Button Text="4" Style="{StaticResource NumberPadButton}" Clicked="OnNumberClicked" Grid.Row="0" Grid.Column="3" />
            <Button Text="5" Style="{StaticResource NumberPadButton}" Clicked="OnNumberClicked" Grid.Row="0" Grid.Column="4" />
            <Button Text="6" Style="{StaticResource NumberPadButton}" Clicked="OnNumberClicked" Grid.Row="1" Grid.Column="0" />
            <Button Text="7" Style="{StaticResource NumberPadButton}" Clicked="OnNumberClicked" Grid.Row="1" Grid.Column="1" />
            <Button Text="8" Style="{StaticResource NumberPadButton}" Clicked="OnNumberClicked" Grid.Row="1" Grid.Column="2" />
            <Button Text="9" Style="{StaticResource NumberPadButton}" Clicked="OnNumberClicked" Grid.Row="1" Grid.Column="3" />
            <Button Text="✕" Style="{StaticResource NumberPadButton}" Clicked="OnClearClicked" Grid.Row="1" Grid.Column="4" />
        </Grid>

        <!-- Row 5: Auto Candidate -->
        <HorizontalStackLayout HorizontalOptions="Center" Spacing="8" Padding="8" Grid.Row="5">
            <CheckBox IsChecked="{Binding IsAutoCandidateMode}"
                      Color="{AppThemeBinding Light=#2970c2, Dark=#5b9bd5}" />
            <Label Text="Auto Candidate Mode"
                   VerticalTextAlignment="Center"
                   TextColor="{AppThemeBinding Light=#666666, Dark=#aaaaaa}"
                   FontSize="13" />
        </HorizontalStackLayout>

    </Grid>

</ContentPage>
```

- [ ] **Step 2: Implement GamePage.xaml.cs**

Replace `Sudoku.App/Views/GamePage.xaml.cs`:

```csharp
using Sudoku.App.ViewModels;

namespace Sudoku.App.Views;

[QueryProperty(nameof(ViewModel), "ViewModel")]
public partial class GamePage : ContentPage
{
    private GameViewModel? _viewModel;

    public GameViewModel? ViewModel
    {
        get => _viewModel;
        set
        {
            _viewModel = value;
            if (_viewModel != null)
            {
                BindingContext = _viewModel;
                SetupBoard();
            }
        }
    }

    public GamePage(GameViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel?.RefreshSettings();
        _viewModel?.ResumeTimer();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel?.StopTimer();
    }

    private void SetupBoard()
    {
        if (_viewModel == null)
        {
            return;
        }

        BoardView.Drawable = _viewModel.Drawable;
        _viewModel.RequestInvalidate = () =>
        {
            MainThread.BeginInvokeOnMainThread(() => BoardView.Invalidate());
        };

        BoardView.StartInteraction += OnBoardTouched;

        // Make the board square and responsive
        BoardView.SizeChanged += (_, _) =>
        {
            var size = Math.Min(BoardView.Width, BoardView.Height);
            if (size > 0)
            {
                BoardView.WidthRequest = size;
                BoardView.HeightRequest = size;
            }
        };
    }

    private void OnBoardTouched(object? sender, TouchEventArgs e)
    {
        if (_viewModel == null || e.Touches.Length == 0)
        {
            return;
        }

        var point = e.Touches[0];
        var (row, col) = _viewModel.Drawable.HitTest(
            point,
            (float)BoardView.Width,
            (float)BoardView.Height);

        if (row >= 0 && col >= 0)
        {
            _viewModel.OnCellTapped(row, col);
        }
    }

    private void OnNumberClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && int.TryParse(button.Text, out var number))
        {
            _viewModel?.NumberInputCommand.Execute(number);
        }
    }

    private void OnClearClicked(object? sender, EventArgs e)
    {
        _viewModel?.ClearInputCommand.Execute(null);
    }

    private void OnNormalModeClicked(object? sender, EventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.IsNormalMode = true;
        }
    }

    private void OnCandidateModeClicked(object? sender, EventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.IsNormalMode = false;
        }
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnSettingsClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(SettingsPage));
    }
}
```

- [ ] **Step 3: Build to verify**

The build will fail because we referenced styles (`NumberPadButton`, converters, etc.) not yet defined. We'll add these in Task 12 (Theme). For now, the structural code is in place.

```bash
dotnet build Sudoku.App
```

Expected: Build errors about missing static resources. Note these for Task 12.

- [ ] **Step 4: Commit (work in progress)**

```bash
git add Sudoku.App/Views/GamePage.xaml Sudoku.App/Views/GamePage.xaml.cs
git commit -m "feat: add GamePage with grid, number pad, and controls layout"
```

---

## Task 10: ViewModels — MenuViewModel + MenuPage

**Files:**
- Modify: `Sudoku.App/ViewModels/MenuViewModel.cs` (replace stub)
- Modify: `Sudoku.App/Views/MenuPage.xaml` (replace placeholder)
- Modify: `Sudoku.App/Views/MenuPage.xaml.cs` (replace placeholder)

- [ ] **Step 1: Implement MenuViewModel**

Replace `Sudoku.App/ViewModels/MenuViewModel.cs`:

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

    [ObservableProperty]
    private bool _hasSavedGame;

    [ObservableProperty]
    private string _savedGameInfo = "";

    [ObservableProperty]
    private bool _isGenerating;

    public MenuViewModel(
        SudokuGenerator generator,
        ISolver solver,
        GamePersistenceService persistenceService,
        GameViewModel gameViewModel)
    {
        _generator = generator;
        _solver = solver;
        _persistenceService = persistenceService;
        _gameViewModel = gameViewModel;
    }

    public async Task CheckForSavedGameAsync()
    {
        var state = await _persistenceService.LoadAsync();
        if (state != null)
        {
            HasSavedGame = true;
            var minutes = state.ElapsedSeconds / 60;
            var seconds = state.ElapsedSeconds % 60;
            SavedGameInfo = $"{state.Difficulty} — {minutes}:{seconds:D2}";
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
        if (state == null)
        {
            return;
        }

        _gameViewModel.LoadState(state);
        await Shell.Current.GoToAsync(nameof(Views.GamePage));
    }

    [RelayCommand]
    private async Task NewGame(string difficultyName)
    {
        if (IsGenerating)
        {
            return;
        }

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
        await Shell.Current.GoToAsync(nameof(Views.GamePage));
    }
}
```

- [ ] **Step 2: Implement MenuPage.xaml**

Replace `Sudoku.App/Views/MenuPage.xaml`:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:vm="clr-namespace:Sudoku.App.ViewModels"
             x:Class="Sudoku.App.Views.MenuPage"
             x:DataType="vm:MenuViewModel"
             Shell.NavBarIsVisible="False"
             BackgroundColor="{AppThemeBinding Light={StaticResource PageBackgroundLight}, Dark={StaticResource PageBackgroundDark}}">

    <Grid RowDefinitions="*,Auto" Padding="24">

        <VerticalStackLayout VerticalOptions="Center" Spacing="8">

            <!-- Title -->
            <Label Text="SUDOKU"
                   FontSize="36"
                   FontAttributes="Bold"
                   HorizontalTextAlignment="Center"
                   TextColor="{AppThemeBinding Light=#1a1a1a, Dark=#e8e8e8}" />
            <Label Text="PUZZLE GAME"
                   FontSize="12"
                   CharacterSpacing="4"
                   HorizontalTextAlignment="Center"
                   TextColor="{AppThemeBinding Light=#999999, Dark=#666666}" />

            <!-- Continue Button -->
            <Button Text="{Binding SavedGameInfo, StringFormat='Continue — {0}'}"
                    IsVisible="{Binding HasSavedGame}"
                    BackgroundColor="#2970c2"
                    TextColor="White"
                    FontSize="16"
                    FontAttributes="Bold"
                    HeightRequest="56"
                    CornerRadius="10"
                    Margin="0,32,0,0"
                    Command="{Binding ContinueGameCommand}" />

            <!-- Loading Spinner -->
            <ActivityIndicator IsRunning="{Binding IsGenerating}"
                               IsVisible="{Binding IsGenerating}"
                               Color="#2970c2"
                               HeightRequest="40"
                               Margin="0,16,0,0" />

            <!-- New Game Header -->
            <Label Text="NEW GAME"
                   FontSize="11"
                   CharacterSpacing="2"
                   HorizontalTextAlignment="Center"
                   TextColor="{AppThemeBinding Light=#999999, Dark=#666666}"
                   Margin="0,24,0,4" />

            <!-- Difficulty Buttons -->
            <Button Text="Easy" TextColor="#b5bd68"
                    Style="{StaticResource DifficultyButton}"
                    Command="{Binding NewGameCommand}" CommandParameter="Easy" />
            <Button Text="Medium" TextColor="#f0c674"
                    Style="{StaticResource DifficultyButton}"
                    Command="{Binding NewGameCommand}" CommandParameter="Medium" />
            <Button Text="Hard" TextColor="#de935f"
                    Style="{StaticResource DifficultyButton}"
                    Command="{Binding NewGameCommand}" CommandParameter="Hard" />
            <Button Text="Expert" TextColor="#cc6666"
                    Style="{StaticResource DifficultyButton}"
                    Command="{Binding NewGameCommand}" CommandParameter="Expert" />
            <Button Text="Evil" TextColor="#b294bb"
                    Style="{StaticResource DifficultyButton}"
                    Command="{Binding NewGameCommand}" CommandParameter="Evil" />

        </VerticalStackLayout>

        <!-- Settings Link -->
        <Button Text="&#9881; Settings"
                BackgroundColor="Transparent"
                TextColor="{AppThemeBinding Light=#999999, Dark=#666666}"
                FontSize="14"
                Clicked="OnSettingsClicked"
                Grid.Row="1"
                HorizontalOptions="Center"
                Margin="0,0,0,16" />

    </Grid>

</ContentPage>
```

- [ ] **Step 3: Implement MenuPage.xaml.cs**

Replace `Sudoku.App/Views/MenuPage.xaml.cs`:

```csharp
using Sudoku.App.ViewModels;

namespace Sudoku.App.Views;

public partial class MenuPage : ContentPage
{
    private readonly MenuViewModel _viewModel;

    public MenuPage(MenuViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CheckForSavedGameAsync();
    }

    private async void OnSettingsClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(SettingsPage));
    }
}
```

- [ ] **Step 4: Build to verify**

```bash
dotnet build Sudoku.App
```

Expected: May have resource warnings (styles not yet defined). Core compilation should succeed.

- [ ] **Step 5: Commit**

```bash
git add Sudoku.App/ViewModels/MenuViewModel.cs Sudoku.App/Views/MenuPage.xaml Sudoku.App/Views/MenuPage.xaml.cs
git commit -m "feat: add MenuPage with difficulty selection, continue game, and puzzle generation"
```

---

## Task 11: ViewModels — SettingsViewModel + SettingsPage

**Files:**
- Modify: `Sudoku.App/ViewModels/SettingsViewModel.cs` (replace stub)
- Modify: `Sudoku.App/Views/SettingsPage.xaml` (replace placeholder)
- Modify: `Sudoku.App/Views/SettingsPage.xaml.cs` (replace placeholder)

- [ ] **Step 1: Implement SettingsViewModel**

Replace `Sudoku.App/ViewModels/SettingsViewModel.cs`:

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
    private bool _isDarkTheme = true;

    [ObservableProperty]
    private bool _highlightRelatedCells = true;

    [ObservableProperty]
    private bool _highlightSameNumbers = true;

    [ObservableProperty]
    private bool _showErrors = true;

    [ObservableProperty]
    private bool _autoRemoveCandidates;

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
        AutoRemoveCandidates = _settings.AutoRemoveCandidates;
    }

    partial void OnIsDarkThemeChanged(bool value)
    {
        _settings.IsDarkTheme = value;
        SaveAndApplyTheme();
    }

    partial void OnHighlightRelatedCellsChanged(bool value)
    {
        _settings.HighlightRelatedCells = value;
        Save();
    }

    partial void OnHighlightSameNumbersChanged(bool value)
    {
        _settings.HighlightSameNumbers = value;
        Save();
    }

    partial void OnShowErrorsChanged(bool value)
    {
        _settings.ShowErrors = value;
        Save();
    }

    partial void OnAutoRemoveCandidatesChanged(bool value)
    {
        _settings.AutoRemoveCandidates = value;
        Save();
    }

    private void Save()
    {
        _settingsService.Save(_settings);
    }

    private void SaveAndApplyTheme()
    {
        Save();
        if (Application.Current != null)
        {
            Application.Current.UserAppTheme = _settings.IsDarkTheme
                ? AppTheme.Dark
                : AppTheme.Light;
        }
    }
}
```

- [ ] **Step 2: Implement SettingsPage.xaml**

Replace `Sudoku.App/Views/SettingsPage.xaml`:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:vm="clr-namespace:Sudoku.App.ViewModels"
             x:Class="Sudoku.App.Views.SettingsPage"
             x:DataType="vm:SettingsViewModel"
             Shell.NavBarIsVisible="False"
             BackgroundColor="{AppThemeBinding Light={StaticResource PageBackgroundLight}, Dark={StaticResource PageBackgroundDark}}">

    <Grid RowDefinitions="Auto,*" Padding="24,16">

        <!-- Header -->
        <HorizontalStackLayout Spacing="12">
            <Button Text="&#9664;"
                    BackgroundColor="Transparent"
                    TextColor="{AppThemeBinding Light=#666666, Dark=#888888}"
                    FontSize="18"
                    Clicked="OnBackClicked" />
            <Label Text="Settings"
                   FontSize="22"
                   FontAttributes="Bold"
                   VerticalTextAlignment="Center"
                   TextColor="{AppThemeBinding Light=#1a1a1a, Dark=#e8e8e8}" />
        </HorizontalStackLayout>

        <ScrollView Grid.Row="1" Margin="0,16,0,0">
            <VerticalStackLayout Spacing="0">

                <!-- Appearance Section -->
                <Label Text="APPEARANCE"
                       FontSize="11"
                       CharacterSpacing="2"
                       TextColor="#2970c2"
                       Margin="0,0,0,12" />

                <Grid ColumnDefinitions="*,Auto" Padding="0,14"
                      StyleClass="SettingsRow">
                    <VerticalStackLayout>
                        <Label Text="Dark Theme"
                               FontSize="15"
                               TextColor="{AppThemeBinding Light=#1a1a1a, Dark=#e8e8e8}" />
                        <Label Text="Toggle between dark and light"
                               FontSize="12"
                               TextColor="{AppThemeBinding Light=#999999, Dark=#666666}" />
                    </VerticalStackLayout>
                    <Switch IsToggled="{Binding IsDarkTheme}"
                            OnColor="#2970c2"
                            Grid.Column="1"
                            VerticalOptions="Center" />
                </Grid>

                <!-- Gameplay Section -->
                <Label Text="GAMEPLAY"
                       FontSize="11"
                       CharacterSpacing="2"
                       TextColor="#2970c2"
                       Margin="0,24,0,12" />

                <Grid ColumnDefinitions="*,Auto" Padding="0,14"
                      StyleClass="SettingsRow">
                    <VerticalStackLayout>
                        <Label Text="Highlight Related Cells"
                               FontSize="15"
                               TextColor="{AppThemeBinding Light=#1a1a1a, Dark=#e8e8e8}" />
                        <Label Text="Shade row, column, and box"
                               FontSize="12"
                               TextColor="{AppThemeBinding Light=#999999, Dark=#666666}" />
                    </VerticalStackLayout>
                    <Switch IsToggled="{Binding HighlightRelatedCells}"
                            OnColor="#2970c2"
                            Grid.Column="1"
                            VerticalOptions="Center" />
                </Grid>

                <Grid ColumnDefinitions="*,Auto" Padding="0,14"
                      StyleClass="SettingsRow">
                    <VerticalStackLayout>
                        <Label Text="Highlight Same Numbers"
                               FontSize="15"
                               TextColor="{AppThemeBinding Light=#1a1a1a, Dark=#e8e8e8}" />
                        <Label Text="Tint cells with matching value"
                               FontSize="12"
                               TextColor="{AppThemeBinding Light=#999999, Dark=#666666}" />
                    </VerticalStackLayout>
                    <Switch IsToggled="{Binding HighlightSameNumbers}"
                            OnColor="#2970c2"
                            Grid.Column="1"
                            VerticalOptions="Center" />
                </Grid>

                <Grid ColumnDefinitions="*,Auto" Padding="0,14"
                      StyleClass="SettingsRow">
                    <VerticalStackLayout>
                        <Label Text="Show Errors"
                               FontSize="15"
                               TextColor="{AppThemeBinding Light=#1a1a1a, Dark=#e8e8e8}" />
                        <Label Text="Highlight incorrect values immediately"
                               FontSize="12"
                               TextColor="{AppThemeBinding Light=#999999, Dark=#666666}" />
                    </VerticalStackLayout>
                    <Switch IsToggled="{Binding ShowErrors}"
                            OnColor="#2970c2"
                            Grid.Column="1"
                            VerticalOptions="Center" />
                </Grid>

                <Grid ColumnDefinitions="*,Auto" Padding="0,14"
                      StyleClass="SettingsRow">
                    <VerticalStackLayout>
                        <Label Text="Auto-Remove Candidates"
                               FontSize="15"
                               TextColor="{AppThemeBinding Light=#1a1a1a, Dark=#e8e8e8}" />
                        <Label Text="Remove candidates when placing a number"
                               FontSize="12"
                               TextColor="{AppThemeBinding Light=#999999, Dark=#666666}" />
                    </VerticalStackLayout>
                    <Switch IsToggled="{Binding AutoRemoveCandidates}"
                            OnColor="#2970c2"
                            Grid.Column="1"
                            VerticalOptions="Center" />
                </Grid>

            </VerticalStackLayout>
        </ScrollView>

    </Grid>

</ContentPage>
```

- [ ] **Step 3: Implement SettingsPage.xaml.cs**

Replace `Sudoku.App/Views/SettingsPage.xaml.cs`:

```csharp
using Sudoku.App.ViewModels;

namespace Sudoku.App.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
```

- [ ] **Step 4: Build to verify**

```bash
dotnet build Sudoku.App
```

Expected: Build may show missing resource warnings. Core compilation should pass.

- [ ] **Step 5: Commit**

```bash
git add Sudoku.App/ViewModels/SettingsViewModel.cs Sudoku.App/Views/SettingsPage.xaml Sudoku.App/Views/SettingsPage.xaml.cs
git commit -m "feat: add SettingsPage with theme toggle and gameplay options"
```

---

## Task 12: Theme Resources, Styles, and Converters

**Files:**
- Modify: `Sudoku.App/Resources/Styles/Colors.xaml`
- Modify: `Sudoku.App/Resources/Styles/Styles.xaml`
- Create: `Sudoku.App/Converters/BoolToModeConverters.cs`
- Modify: `Sudoku.App/App.xaml`
- Modify: `Sudoku.App/App.xaml.cs`

- [ ] **Step 1: Define color resources**

Replace `Sudoku.App/Resources/Styles/Colors.xaml`:

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<?xaml-comp compile="true" ?>
<ResourceDictionary
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">

    <!-- Page backgrounds -->
    <Color x:Key="PageBackgroundDark">#1a1a1a</Color>
    <Color x:Key="PageBackgroundLight">#f0f0f0</Color>

    <!-- Accent -->
    <Color x:Key="AccentBlue">#2970c2</Color>

    <!-- Number pad -->
    <Color x:Key="NumberPadDark">#3a3a3a</Color>
    <Color x:Key="NumberPadLight">#d0d0d0</Color>
    <Color x:Key="NumberPadTextDark">#e8e8e8</Color>
    <Color x:Key="NumberPadTextLight">#1a1a1a</Color>

    <!-- Difficulty button -->
    <Color x:Key="DifficultyBgDark">#2a2a2a</Color>
    <Color x:Key="DifficultyBgLight">#e0e0e0</Color>
    <Color x:Key="DifficultyBorderDark">#3a3a3a</Color>
    <Color x:Key="DifficultyBorderLight">#cccccc</Color>

    <!-- Mode buttons -->
    <Color x:Key="ModeActiveBg">#e8e8e8</Color>
    <Color x:Key="ModeActiveText">#1a1a1a</Color>
    <Color x:Key="ModeInactiveBgDark">#2a2a2a</Color>
    <Color x:Key="ModeInactiveTextDark">#888888</Color>

</ResourceDictionary>
```

- [ ] **Step 2: Define styles**

Replace `Sudoku.App/Resources/Styles/Styles.xaml`:

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<?xaml-comp compile="true" ?>
<ResourceDictionary
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">

    <Style x:Key="NumberPadButton" TargetType="Button">
        <Setter Property="FontSize" Value="20" />
        <Setter Property="FontAttributes" Value="Bold" />
        <Setter Property="HeightRequest" Value="56" />
        <Setter Property="CornerRadius" Value="8" />
        <Setter Property="BackgroundColor" Value="{AppThemeBinding Light={StaticResource NumberPadLight}, Dark={StaticResource NumberPadDark}}" />
        <Setter Property="TextColor" Value="{AppThemeBinding Light={StaticResource NumberPadTextLight}, Dark={StaticResource NumberPadTextDark}}" />
    </Style>

    <Style x:Key="DifficultyButton" TargetType="Button">
        <Setter Property="FontSize" Value="15" />
        <Setter Property="FontAttributes" Value="Bold" />
        <Setter Property="HeightRequest" Value="50" />
        <Setter Property="CornerRadius" Value="8" />
        <Setter Property="BackgroundColor" Value="{AppThemeBinding Light={StaticResource DifficultyBgLight}, Dark={StaticResource DifficultyBgDark}}" />
        <Setter Property="BorderColor" Value="{AppThemeBinding Light={StaticResource DifficultyBorderLight}, Dark={StaticResource DifficultyBorderDark}}" />
        <Setter Property="BorderWidth" Value="1" />
    </Style>

</ResourceDictionary>
```

- [ ] **Step 3: Create value converters for mode toggle buttons**

Create `Sudoku.App/Converters/BoolToModeConverters.cs`:

```csharp
using System.Globalization;

namespace Sudoku.App.Converters;

public class BoolToModeColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isActive = value is true;
        return isActive
            ? Color.FromArgb("#e8e8e8")
            : Color.FromArgb("#2a2a2a");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class BoolToModeTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isActive = value is true;
        return isActive
            ? Color.FromArgb("#1a1a1a")
            : Color.FromArgb("#888888");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class InverseBoolToModeColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isActive = value is false;
        return isActive
            ? Color.FromArgb("#e8e8e8")
            : Color.FromArgb("#2a2a2a");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class InverseBoolToModeTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isActive = value is false;
        return isActive
            ? Color.FromArgb("#1a1a1a")
            : Color.FromArgb("#888888");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
```

- [ ] **Step 4: Register converters in App.xaml**

Replace `Sudoku.App/App.xaml`:

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<Application xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:converters="clr-namespace:Sudoku.App.Converters"
             x:Class="Sudoku.App.App">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="Resources/Styles/Colors.xaml" />
                <ResourceDictionary Source="Resources/Styles/Styles.xaml" />
            </ResourceDictionary.MergedDictionaries>

            <converters:BoolToModeColorConverter x:Key="BoolToModeColorConverter" />
            <converters:BoolToModeTextConverter x:Key="BoolToModeTextConverter" />
            <converters:InverseBoolToModeColorConverter x:Key="InverseBoolToModeColorConverter" />
            <converters:InverseBoolToModeTextConverter x:Key="InverseBoolToModeTextConverter" />
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

- [ ] **Step 5: Update App.xaml.cs to apply saved theme on startup**

Replace `Sudoku.App/App.xaml.cs`:

```csharp
using Sudoku.App.Services;

namespace Sudoku.App;

public partial class App : Application
{
    public App(SettingsService settingsService)
    {
        InitializeComponent();

        var settings = settingsService.Load();
        UserAppTheme = settings.IsDarkTheme ? AppTheme.Dark : AppTheme.Light;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}
```

- [ ] **Step 6: Build the full solution**

```bash
dotnet build Sudoku.App
```

Expected: Build succeeds with 0 errors.

- [ ] **Step 7: Commit**

```bash
git add Sudoku.App/Resources/Styles/ Sudoku.App/Converters/ Sudoku.App/App.xaml Sudoku.App/App.xaml.cs
git commit -m "feat: add dark/light theme resources, styles, converters, and theme switching"
```

---

## Task 13: App Lifecycle — Save/Resume and Timer Management

**Files:**
- Modify: `Sudoku.App/App.xaml.cs`

The MAUI `Window` lifecycle events `Stopped`/`Resumed` handle auto-save and timer pause/resume.

- [ ] **Step 1: Add lifecycle handling to App.xaml.cs**

Replace `Sudoku.App/App.xaml.cs`:

```csharp
using Sudoku.App.Services;
using Sudoku.App.ViewModels;

namespace Sudoku.App;

public partial class App : Application
{
    private readonly GameViewModel _gameViewModel;
    private readonly GamePersistenceService _persistenceService;

    public App(SettingsService settingsService, GameViewModel gameViewModel, GamePersistenceService persistenceService)
    {
        InitializeComponent();

        _gameViewModel = gameViewModel;
        _persistenceService = persistenceService;

        var settings = settingsService.Load();
        UserAppTheme = settings.IsDarkTheme ? AppTheme.Dark : AppTheme.Light;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());

        window.Stopped += (_, _) =>
        {
            _gameViewModel.StopTimer();
            if (_gameViewModel.State != null)
            {
                _ = _persistenceService.SaveAsync(_gameViewModel.State);
            }
        };

        window.Resumed += (_, _) =>
        {
            _gameViewModel.ResumeTimer();
        };

        return window;
    }
}
```

- [ ] **Step 2: Build to verify**

```bash
dotnet build Sudoku.App
```

Expected: Build succeeds.

- [ ] **Step 3: Run all tests to verify nothing broke**

```bash
dotnet test
```

Expected: All tests pass.

- [ ] **Step 4: Commit**

```bash
git add Sudoku.App/App.xaml.cs
git commit -m "feat: add app lifecycle handling for auto-save and timer pause/resume"
```

---

## Task 14: Win Dialog and Final Integration

**Files:**
- Modify: `Sudoku.App/ViewModels/GameViewModel.cs`
- Modify: `Sudoku.App/Views/GamePage.xaml.cs`

- [ ] **Step 1: Add win detection notification to GamePage**

Add the following to `GamePage.xaml.cs` in the `SetupBoard` method, after the existing `RequestInvalidate` setup:

```csharp
_viewModel.PropertyChanged += async (_, args) =>
{
    if (args.PropertyName == nameof(GameViewModel.IsGameComplete) && _viewModel.IsGameComplete)
    {
        await DisplayAlert(
            "Congratulations!",
            $"You completed the puzzle!\nTime: {_viewModel.TimerText}\nErrors: {_viewModel.ErrorCount}",
            "OK");

        await Shell.Current.GoToAsync("..");
    }
};
```

- [ ] **Step 2: Build the complete solution**

```bash
dotnet build
```

Expected: Entire solution builds with 0 errors.

- [ ] **Step 3: Run all tests**

```bash
dotnet test
```

Expected: All tests pass (existing + new GameState/CellState/GameSettings tests).

- [ ] **Step 4: Commit**

```bash
git add Sudoku.App/Views/GamePage.xaml.cs
git commit -m "feat: add win detection dialog and navigation back to menu"
```

---

## Task 15: Deploy and Test on Windows

- [ ] **Step 1: Run the app on Windows**

```bash
dotnet build Sudoku.App -f net8.0-windows10.0.19041.0
dotnet run --project Sudoku.App -f net8.0-windows10.0.19041.0
```

- [ ] **Step 2: Manual testing checklist**

Verify these flows work:

1. App launches to MenuPage with dark theme
2. Tap a difficulty → loading spinner → GamePage with generated puzzle
3. Tap cells → blue highlight on selected cell, dim blue on related cells
4. Enter a correct number → appears in blue-white
5. Enter a wrong number → cell turns red, error counter increments
6. Toggle to Candidate mode → tap numbers → small digits appear in 3x3 mini-grid
7. Toggle Auto Candidate → all empty cells populate with valid candidates
8. Toggle Auto Candidate off and back on → manually removed candidates stay removed
9. Undo → restores previous cell state
10. Tap X → clears cell value (Normal) or all candidates (Candidate mode)
11. Settings → toggle theme → app switches light/dark
12. Settings → toggle "Highlight Related Cells" off → related cells no longer highlighted
13. Close and reopen app → Continue button appears with difficulty and time
14. Tap Continue → resumes game with correct state
15. Complete a puzzle → congratulations dialog with time and errors

- [ ] **Step 3: Fix any issues found during testing**

- [ ] **Step 4: Final commit**

```bash
git add -A
git commit -m "fix: address issues found during manual testing"
```
