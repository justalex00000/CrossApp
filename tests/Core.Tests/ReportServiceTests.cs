using Core.Reports;
using Xunit;

namespace Core.Tests;

public class ReportServiceTests
{
    [Theory]
    [InlineData(10, 2)]
    [InlineData(1, 0)]
    public void LowStock_CountsOnlyBelowThreshold(
        int threshold,
        int expected)
    {
        var result = ReportService.LowStock(
            TestData.Items(),
            threshold);

        Assert.Equal(expected, result.Count);
    }

    [Fact]
    public void TopByQuantity_IsDeterministic()
    {
        var result = ReportService.TopByQuantity(
            TestData.Items(),
            2);

        Assert.Equal(
            new[] { "SKU-BIG", "SKU-MID" },
            result.Select(p => p.Sku));
    }
}
