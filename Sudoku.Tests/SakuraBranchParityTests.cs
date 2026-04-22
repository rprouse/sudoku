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
