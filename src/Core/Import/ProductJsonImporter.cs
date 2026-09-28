using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class ProductJsonImporter
{
    public static ImportResult<ProductDto> Load(string path)
    {
        var items = new List<ProductDto>();
        var errors = new List<string>();

        string json = File.ReadAllText(path);

        try
        {
            using JsonDocument document = JsonDocument.Parse(json);

            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                errors.Add("рядок 1: JSON має містити масив об'єктів");
                return new ImportResult<ProductDto>(items, errors);
            }

            int number = 0;

            foreach (JsonElement element in document.RootElement.EnumerateArray())
            {
                number++;

                try
                {
                    ProductDto? product = JsonSerializer.Deserialize<ProductDto>(
                        element.GetRawText(),
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    if (product is null)
                    {
                        errors.Add($"рядок {number}: порожній запис");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(product.Sku))
                    {
                        errors.Add($"рядок {number}: SKU або назва порожні");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(product.Name))
                    {
                        errors.Add($"рядок {number}: SKU або назва порожні");
                        continue;
                    }

                    if (product.Quantity < 0)
                    {
                        errors.Add(
                            $"рядок {number}: кількість '{product.Quantity}' не є невід'ємним числом");
                        continue;
                    }

                    items.Add(product);
                }
                catch (JsonException)
                {
                    errors.Add(
                        $"рядок {number}: некоректні дані");
                }
                catch (InvalidOperationException)
                {
                    errors.Add(
                        $"рядок {number}: некоректний тип даних");
                }
            }
        }
        catch (JsonException)
        {
            errors.Add("рядок 1: некоректний JSON");
        }

        return new ImportResult<ProductDto>(items, errors);
    }
}