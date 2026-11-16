using Core.Domain;

namespace Core.Abstractions;

public interface IStreamingCatalogImporter
{
    IAsyncEnumerable<Product> StreamAsync(
        string path,
        CancellationToken ct = default);
}