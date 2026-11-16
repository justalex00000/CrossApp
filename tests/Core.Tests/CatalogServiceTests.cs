using Core.Domain;
using Core.Events;
using Core.Services;
using Core.Storage;
using Xunit;

namespace Core.Tests;

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
}
