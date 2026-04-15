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
