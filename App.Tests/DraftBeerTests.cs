using BlazorApp.Models;

namespace App.Tests;

public class DraftBeerTests
{
    [Fact]
    public void AnkerPrice_IsStrTimesLiterPrice()
    {
        var beer = CreateBeer(str: 30, pricePrUnit: 24);
        Assert.Equal(720, beer.AnkerPrice);
    }

    [Fact]
    public void AnkerPrice_20LKeg()
    {
        var beer = CreateBeer(str: 20, pricePrUnit: 43.25);
        Assert.Equal(865, beer.AnkerPrice);
    }

    [Fact]
    public void AnkerPrice_WithZeroStr_IsZero()
    {
        var beer = CreateBeer(str: 0, pricePrUnit: 30);
        Assert.Equal(0, beer.AnkerPrice);
    }

    [Fact]
    public void AnkerPrice_WithZeroPrice_IsZero()
    {
        var beer = CreateBeer(str: 30, pricePrUnit: 0);
        Assert.Equal(0, beer.AnkerPrice);
    }

    private static DraftBeer CreateBeer(double str, double pricePrUnit) => new()
    {
        OctopusId = 1,
        WebId = 1,
        WebTitle = "Test 30L",
        PdfTitle = "Test",
        OctopusTitle = "Test",
        Available = 10,
        Category = "TEST",
        Str = str,
        PricePrUnit = pricePrUnit
    };
}
