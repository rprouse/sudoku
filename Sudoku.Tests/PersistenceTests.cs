using FluentAssertions;

using Sudoku.Core;

namespace Sudoku.Tests;

public class PersistenceTests
{
    [Test]
    public void CanLoadFromJson()
    {
        var sudokus = Persistence.LoadFromJson("sudokus.json");
        sudokus.Should().NotBeNull();
        sudokus.Easy.Should().NotBeEmpty();
        sudokus.Medium.Should().NotBeEmpty();
        sudokus.Hard.Should().NotBeEmpty();
        sudokus.Expert.Should().NotBeEmpty();
        sudokus.Evil.Should().NotBeEmpty();

        sudokus.Easy.Should().OnlyContain(s => s.Sudoku.Length == 9);

        sudokus.Easy.Should().HaveCount(100);
        sudokus.Medium.Should().HaveCount(100);
        sudokus.Hard.Should().HaveCount(100);
        sudokus.Expert.Should().HaveCount(100);
        sudokus.Evil.Should().HaveCount(100);
    }
}