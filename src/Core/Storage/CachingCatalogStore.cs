using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class CachingCatalogStore : ICatalogStore
{
    private readonly ICatalogStore _inner;

    private IReadOnlyList<Product>? _listCache;

    private readonly Dictionary<string, Product?> _byIdCache =
        new(StringComparer.OrdinalIgnoreCase);

    public CachingCatalogStore(ICatalogStore inner)
    {
        _inner = inner
            ?? throw new ArgumentNullException(nameof(inner));
    }

    public IReadOnlyList<Product> List()
    {
        if (_listCache is not null)
            return _listCache;

        _listCache = _inner.List().ToList();
        return _listCache;
    }

    public Product? GetById(string id)
    {
        if (_byIdCache.TryGetValue(id, out Product? product))
            return product;

        product = _inner.GetById(id);
        _byIdCache[id] = product;

        return product;
    }

    public void Add(Product item)
    {
        _inner.Add(item);

        ClearCache();
    }

    public void Update(Product item)
    {
        _inner.Update(item);

        ClearCache();
    }

    public bool Remove(string id)
    {
        bool removed = _inner.Remove(id);

        ClearCache();

        return removed;
    }

    private void ClearCache()
    {
        _listCache = null;
        _byIdCache.Clear();
    }
}