using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class FileCatalogStore : ICatalogStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    private readonly Dictionary<string, Product> _cache =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly string _path;

private bool _loaded;

public FileCatalogStore(string path)
{
    _path = Path.GetFullPath(path);
}

    private void EnsureLoaded()
    {
        if (_loaded)
            return;

        if (File.Exists(_path))
        {
            List<ProductDto> dtos =
                JsonSerializer.Deserialize<List<ProductDto>>(
                    File.ReadAllText(_path)) ?? [];

            foreach (ProductDto dto in dtos)
            {
                Product product = Product.FromDto(dto);

                _cache[product.Id] = product;
            }
        }

        _loaded = true;
    }

    private void Flush()
    {
        Directory.CreateDirectory(
            Path.GetDirectoryName(_path)!);

        List<ProductDto> dtos = _cache.Values
            .Select(product => product.ToDto())
            .ToList();

        string json = JsonSerializer.Serialize(dtos, Options);

        File.WriteAllText(_path, json);
    }

    public IReadOnlyList<Product> List()
    {
        EnsureLoaded();
        return _cache.Values.ToList();
    }

    public Product? GetById(string id)
    {
        EnsureLoaded();
        return _cache.GetValueOrDefault(id);
    }

    public void Add(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureLoaded();

        if (_cache.ContainsKey(item.Id))
            throw new InvalidOperationException(
                $"Запис з id={item.Id} уже існує.");

        _cache.Add(item.Id, item);

        Flush();
    }

    public void Update(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureLoaded();

        _cache[item.Id] = item;
        Flush();
    }

    public bool Remove(string id)
    {
        EnsureLoaded();

        if (!_cache.Remove(id))
            return false;

        Flush();
        return true;
    }
}