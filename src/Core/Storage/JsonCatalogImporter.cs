using System.Text.Json;

using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class JsonCatalogImporter : ICatalogImporter
{
    public async Task<IReadOnlyList<Product>> LoadAsync(
        string path,
        CancellationToken ct = default)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            4096,
            useAsync: true);

        using var document = await JsonDocument.ParseAsync(
            stream,
            cancellationToken: ct);

        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            throw new DomainException(
                $"Файл {Path.GetFileName(path)} не містить списку.");
        }

        var result = new List<Product>();
        var index = 0;

        foreach (var element in document.RootElement.EnumerateArray())
        {
            ct.ThrowIfCancellationRequested();
            index++;

            if (!element.TryGetProperty("type", out var type))
                throw new DomainException(
                    $"Запис {index}: відсутнє поле type.");

            if (type.GetString() != "P")
                continue;

            try
            {
                var dto = element.Deserialize<ProductDto>(
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                result.Add(Product.FromDto(dto!));
            }
            catch (JsonException ex)
            {
                throw new DomainException(
                    $"Запис {index}: некоректний формат товару.",
                    ex);
            }
            catch (ArgumentException ex)
            {
                throw new DomainException(
                    $"Запис {index}: {ex.Message}",
                    ex);
            }
        }

        return result;
    }
}
