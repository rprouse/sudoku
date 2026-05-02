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
