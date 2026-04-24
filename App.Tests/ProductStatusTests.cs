using BlazorApp.Models;

namespace App.Tests;

public class ProductStatusTests
{
    [Fact]
    public void StatusFor_AboveThreshold_ReturnsAvailable()
    {
        Assert.Equal(ProductStatus.Available, Product.StatusFor(31));
    }

    [Fact]
    public void StatusFor_WellAboveThreshold_ReturnsAvailable()
    {
        Assert.Equal(ProductStatus.Available, Product.StatusFor(1000));
    }

    [Fact]
    public void StatusFor_AtThreshold_ReturnsAvailable()
    {
        Assert.Equal(ProductStatus.Available, Product.StatusFor(30));
    }

    [Fact]
    public void StatusFor_JustBelowThreshold_ReturnsSoldOut()
    {
        Assert.Equal(ProductStatus.SoldOut, Product.StatusFor(29));
    }

    [Fact]
    public void StatusFor_BelowThreshold_ReturnsSoldOut()
    {
        Assert.Equal(ProductStatus.SoldOut, Product.StatusFor(0));
    }

    [Fact]
    public void StatusFor_NegativeStock_ReturnsSoldOut()
    {
        Assert.Equal(ProductStatus.SoldOut, Product.StatusFor(-1));
    }
}
