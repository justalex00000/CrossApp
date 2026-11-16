using System.Runtime.CompilerServices;
using System.Text.Json;

using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class JsonCatalogImporter :
    ICatalogImporter,
    IStreamingCatalogImporter
{
    public async Task<IReadOnlyList<Product>> LoadAsync(
        string path,
        CancellationToken ct = default)
    {
        var result = new List<Product>();

        await foreach (var product in StreamAsync(path, ct))
        {
            result.Add(product);
        }

        return result;
    }

    public async IAsyncEnumerable<Product> StreamAsync(
        string path,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            4096,
            useAsync: true);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        int index = 0;

        await foreach (var element in
            JsonSerializer.DeserializeAsyncEnumerable<JsonElement>(
                stream,
                cancellationToken: ct))
        {
            ct.ThrowIfCancellationRequested();
            index++;

            if (element.ValueKind != JsonValueKind.Object)
            {
                throw new DomainException(
                    $"Запис {index}: очікувався JSON-об'єкт.");
            }

            if (!element.TryGetProperty("type", out var type))
            {
                throw new DomainException(
                    $"Запис {index}: відсутнє поле type.");
            }

            if (type.ValueKind != JsonValueKind.String)
            {
                throw new DomainException(
                    $"Запис {index}: поле type повинно бути рядком.");
            }

            if (type.GetString() != "P")
                continue;

            Product product;

            try
            {
                var dto = element.Deserialize<ProductDto>(options);

                if (dto is null)
                {
                    throw new DomainException(
                        $"Запис {index}: порожній запис товару.");
                }

                product = Product.FromDto(dto);
            }
            catch (ArgumentException ex)
            {
                throw new DomainException(
                    $"Запис {index}: {ex.Message}",
                    ex);
            }

            yield return product;
        }
    }
}
