using Sudoku.Core.Game;

namespace Sudoku.Tests.App;

[TestFixture]
public class WinMessagesTests
{
    [Test]
    public void Count_ReturnsAtLeast100()
    {
        WinMessages.Count.Should().BeGreaterThanOrEqualTo(100);
    }

    [Test]
    public void GetRandom_ReturnsNonEmptyString()
    {
        var message = WinMessages.GetRandom();

        message.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public void GetRandom_ReturnsDifferentValues()
    {
        var messages = Enumerable.Range(0, 50)
            .Select(_ => WinMessages.GetRandom())
            .Distinct()
            .ToList();

        messages.Count.Should().BeGreaterThan(1);
    }
}
