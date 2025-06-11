using CleanArchitecture.Domain.Exceptions;
using CleanArchitecture.Domain.ValueObjects;
using NUnit.Framework;

namespace CleanArchitecture.Domain.UnitTests.ValueObjects;

public class ColourTests
{
    [Test]
    public void ShouldReturnCorrectColourCode()
    {
        var code = "#FFFFFF";

        var colour = Colour.From(code);

        Assert.That(colour.Code, Is.EqualTo(code));
    }

    [Test]
    public void ToStringReturnsCode()
    {
        var colour = Colour.White;

        Assert.That(colour.ToString(), Is.EqualTo(colour.Code));
    }

    [Test]
    public void ShouldPerformImplicitConversionToColourCodeString()
    {
        string code = Colour.White;

        Assert.That(code, Is.EqualTo("#FFFFFF"));
    }

    [Test]
    public void ShouldPerformExplicitConversionGivenSupportedColourCode()
    {
        var colour = (Colour)"#FFFFFF";

        Assert.That(colour, Is.EqualTo(Colour.White));
    }

    [Test]
    public void ShouldThrowUnsupportedColourExceptionGivenNotSupportedColourCode()
    {
        Assert.Throws<UnsupportedColourException>(() => Colour.From("##FF33CC"));
    }
}
