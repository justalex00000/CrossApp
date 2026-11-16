using Core.Domain;
using Core.Events;
using Core.Services;
using Core.Storage;
using Core.Abstractions;
using Xunit;

namespace Core.Tests;

public sealed class DelayedImporter : ICatalogImporter
{
    public async Task<IReadOnlyList<Product>> LoadAsync(
        string path,
        CancellationToken ct = default)
    {
        await Task.Delay(Timeout.Infinite, ct);

        return Array.Empty<Product>();
    }
}

public class CatalogServiceTests
{
    [Fact]
    public void Receive_WithNegativeQuantity_Throws_AndKeepsState()
    {
        var store = new InMemoryCatalogStore();
        var service = new CatalogService(store);

        var product = service.Add("SKU-1", "Кабель", "м", 10);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => service.Receive(product.Id, -5));

        Assert.Equal(10, store.GetById(product.Id)!.Quantity);
    }

    [Fact]
    public void Add_RaisesChangedExactlyOnce()
    {
        var service = new CatalogService(new InMemoryCatalogStore());

        var events = new List<CatalogChangedEventArgs>();

        service.Changed += (_, e) => events.Add(e);

        service.Add("SKU-2", "Гвинт", "шт", 7);

        Assert.Single(events);
        Assert.Equal(CatalogChangeKind.Added, events[0].Kind);
    }

    [Fact]
    public async Task ImportAsync_WhenTimeoutExpires_CancelsOperation()
    {
        using var userCts = new CancellationTokenSource();

        using var timeoutCts =
            new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        using var linkedCts =
            CancellationTokenSource.CreateLinkedTokenSource(
                userCts.Token,
                timeoutCts.Token);

        var service = new CatalogService(
            new InMemoryCatalogStore());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(
                new DelayedImporter(),
                "unused.json",
                linkedCts.Token));

        Assert.True(timeoutCts.IsCancellationRequested);
        Assert.False(userCts.IsCancellationRequested);
    }
}
