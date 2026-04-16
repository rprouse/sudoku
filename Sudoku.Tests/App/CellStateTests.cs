using FluentAssertions;

using Sudoku.Core.Game;

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
        cell.Candidates[2] = true;
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
