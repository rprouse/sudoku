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
