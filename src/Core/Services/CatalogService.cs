using Core.Abstractions;
using Core.Domain;
using Core.Events;

namespace Core.Services;

public sealed class CatalogService
{
    private readonly ICatalogStore _store;

    private readonly IReadOnlyList<INotificationSink> _sinks;

    public event EventHandler<CatalogChangedEventArgs>? Changed;

    public CatalogService(
        ICatalogStore store,
        IEnumerable<INotificationSink>? sinks = null)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _sinks = sinks?.ToList() ?? [];
    }

    private void OnChanged(
        Product product,
        CatalogChangeKind kind,
        int delta,
        int? quantityAfter = null)
    {
        var eventArgs = new CatalogChangedEventArgs(
            product.Id,
            kind,
            delta,
            quantityAfter ?? product.Quantity);

        Changed?.Invoke(this, eventArgs);

        foreach (var sink in _sinks)
        {
            sink.Notify(eventArgs);
        }
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

        OnChanged(product, CatalogChangeKind.Added, quantity);

        return product;
    }

    public void Receive(string id, int quantity)
    {
        Product product = _store.GetById(id)
            ?? throw new InvalidOperationException(
                $"Немає запису з id={id}.");

        product.RegisterArrival(quantity);
        _store.Update(product);

        OnChanged(product, CatalogChangeKind.Received, quantity);
    }

    public void Issue(string id, int quantity)
    {
        Product product = _store.GetById(id)
            ?? throw new InvalidOperationException(
                $"Немає запису з id={id}.");

        product.Issue(quantity);
        _store.Update(product);

        OnChanged(product, CatalogChangeKind.Issued, -quantity);
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
        Product? product = _store.GetById(id);

        if (product is null)
            return false;

        int removedQuantity = product.Quantity;

        if (!_store.Remove(id))
            return false;

        OnChanged(
            product,
            CatalogChangeKind.Removed,
            -removedQuantity,
            0);

        return true;
    }

    public IReadOnlyList<Product> Search(Func<Product, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return _store.List()
            .Where(predicate)
            .ToList();
    }
}
