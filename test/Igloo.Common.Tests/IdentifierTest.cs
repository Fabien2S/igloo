using System.Globalization;

namespace Igloo.Common.Tests;

public class IdentifierTest
{
    [Fact]
    public void IsValidNamespace_InputIsDefaultNamespace_ReturnsTrue()
    {
        var allowed = Identifier.IsValidNamespace(Minecraft.Namespace);
        Assert.True(allowed);
    }

    [Theory]
    [InlineData("some_underscores")]
    [InlineData("some-dashes")]
    [InlineData("some.period")]
    [InlineData("combined_underscore-dash.period")]
    public void IsValidNamespace_ValuesContainsValidChar_ReturnsTrue(string @namespace)
    {
        var allowed = Identifier.IsValidNamespace(@namespace);
        Assert.True(allowed);
    }

    [Theory]
    [InlineData("@namespace")]
    [InlineData("forward/slash")]
    [InlineData("two/forward/slashes")]
    public void IsValidNamespace_ValuesContainsInvalidChar_ReturnsFalse(string @namespace)
    {
        var allowed = Identifier.IsValidNamespace(@namespace);
        Assert.False(allowed);
    }

    [Theory]
    [InlineData("stone")]
    [InlineData("stone_block")]
    [InlineData("stone-block")]
    [InlineData("color/red")]
    [InlineData("deep/hierarchy/chain")]
    public void IsValidPath_ValuesContainsValidChar_ReturnsTrue(string path)
    {
        var allowed = Identifier.IsValidPath(path);
        Assert.True(allowed);
    }

    [Theory]
    [InlineData("stone@")]
    [InlineData("stone:block")]
    public void IsValidPath_ValuesContainsInvalidChar_ReturnsFalse(string path)
    {
        var allowed = Identifier.IsValidPath(path);
        Assert.False(allowed);
    }

    [Theory]
    [InlineData("stone")]
    [InlineData(":stone")]
    public void TryParse_InputContainsPathOnly_UsesDefaultNamespace(string id)
    {
        var identifier = Identifier.Parse(id, NumberFormatInfo.InvariantInfo);
        Assert.Equal(Minecraft.Namespace, identifier.Namespace.ToString());
    }
}