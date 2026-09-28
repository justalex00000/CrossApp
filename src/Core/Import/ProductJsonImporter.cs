using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class ProductJsonImporter
{
    public static ImportResult<IImportItem> Load(string path)
    {
        var items = new List<IImportItem>();
        var errors = new List<string>();

        string json = File.ReadAllText(path);

        try
        {
            using JsonDocument document = JsonDocument.Parse(json);

            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                errors.Add("рядок 1: JSON має містити масив об'єктів");
                return new ImportResult<IImportItem>(items, errors);
            }

            int number = 0;

            foreach (JsonElement element in document.RootElement.EnumerateArray())
            {
                number++;

                try
                {
                    if (element.ValueKind != JsonValueKind.Object)
                    {
                        errors.Add($"рядок {number}: некоректний тип даних");
                        continue;
                    }

                    JsonElement typeElement;

                    if (!element.TryGetProperty("type", out typeElement))
                    {
                        errors.Add($"рядок {number}: не вказано тип запису");
                        continue;
                    }

                    string? type = typeElement.GetString();

                    switch (type)
                    {
                        case "P":
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

                            if (string.IsNullOrWhiteSpace(product.Sku) ||
                                string.IsNullOrWhiteSpace(product.Name))
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
                            break;

                        case "W":
                            WarehouseDto? warehouse = JsonSerializer.Deserialize<WarehouseDto>(
                                element.GetRawText(),
                                new JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true
                                });

                            if (warehouse is null)
                            {
                                errors.Add($"рядок {number}: порожній запис");
                                continue;
                            }

                            if (string.IsNullOrWhiteSpace(warehouse.Name))
                            {
                                errors.Add($"рядок {number}: назва складу порожня");
                                continue;
                            }

                            if (string.IsNullOrWhiteSpace(warehouse.Address))
                            {
                                errors.Add($"рядок {number}: адреса складу порожня");
                                continue;
                            }

                            items.Add(warehouse);
                            break;

                        default:
                            errors.Add($"рядок {number}: невідомий тип '{type}'");
                            break;
                    }
                }
                catch (JsonException)
                {
                    errors.Add($"рядок {number}: некоректні дані");
                }
                catch (InvalidOperationException)
                {
                    errors.Add($"рядок {number}: некоректний тип даних");
                }
            }
        }
        catch (JsonException)
        {
            errors.Add("рядок 1: некоректний JSON");
        }

        return new ImportResult<IImportItem>(items, errors);
    }
}