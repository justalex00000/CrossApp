using Core.Abstractions;
using Core.Domain;

namespace Core.Services;

public sealed class CatalogService
{
    private readonly ICatalogStore _store;

    public CatalogService(ICatalogStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public Product Add(string sku, string name, string unit, int quantity)
    {
        Product product = Product.Create(
            Guid.NewGuid().ToString("N")[..8],
            sku,
            name,
            unit,
            quantity);

        _store.Add(product);

        return product;
    }

    public void Receive(string id, int quantity)
    {
        Product product = _store.GetById(id)
            ?? throw new InvalidOperationException(
                $"Немає запису з id={id}.");

        product.RegisterArrival(quantity);
        _store.Update(product);
    }

    public void Issue(string id, int quantity)
    {
        Product product = _store.GetById(id)
            ?? throw new InvalidOperationException(
                $"Немає запису з id={id}.");

        product.Issue(quantity);
        _store.Update(product);
    }

    public IReadOnlyList<Product> All()
    {
        return _store.List();
    }

    public Product? Find(string id)
    {
        return _store.GetById(id);
    }

    public bool Remove(string id)
    {
        return _store.Remove(id);
    }

    public IReadOnlyList<Product> Search(Func<Product, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return _store.List()
            .Where(predicate)
            .ToList();
    }
}
