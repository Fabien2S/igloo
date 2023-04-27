using System.Globalization;

namespace Igloo.Common.Tests;

public class IdentifierTest
{
    [Fact]
    public void IsAllowedNamespace_InputIsDefaultNamespace_ReturnsTrue()
    {
        var allowed = Identifier.IsAllowedNamespace(Identifier.DefaultNamespace);
        Assert.True(allowed);
    }

    [Theory]
    [InlineData("some_underscores")]
    [InlineData("some-dashes")]
    [InlineData("some.period")]
    [InlineData("combined_underscore-dash.period")]
    public void IsAllowedNamespace_ValuesContainsValidChar_ReturnsTrue(string @namespace)
    {
        var allowed = Identifier.IsAllowedNamespace(@namespace);
        Assert.True(allowed);
    }

    [Theory]
    [InlineData("@namespace")]
    [InlineData("forward/slash")]
    [InlineData("two/forward/slashes")]
    public void IsAllowedNamespace_ValuesContainsInvalidChar_ReturnsFalse(string @namespace)
    {
        var allowed = Identifier.IsAllowedNamespace(@namespace);
        Assert.False(allowed);
    }

    [Theory]
    [InlineData("stone")]
    [InlineData("stone_block")]
    [InlineData("stone-block")]
    [InlineData("color/red")]
    [InlineData("deep/hierarchy/chain")]
    public void IsAllowedPath_ValuesContainsValidChar_ReturnsTrue(string path)
    {
        var allowed = Identifier.IsAllowedPath(path);
        Assert.True(allowed);
    }

    [Theory]
    [InlineData("stone@")]
    [InlineData("stone:block")]
    public void IsAllowedPath_ValuesContainsInvalidChar_ReturnsFalse(string path)
    {
        var allowed = Identifier.IsAllowedPath(path);
        Assert.False(allowed);
    }

    [Theory]
    [InlineData("stone")]
    [InlineData(":stone")]
    public void TryParse_InputContainsPathOnly_UsesDefaultNamespace(string id)
    {
        var identifier = Identifier.Parse(id, NumberFormatInfo.InvariantInfo);
        Assert.Equal(Identifier.DefaultNamespace, identifier.Namespace.ToString());
    }
}