using Core.Dto;

namespace Core.Import;

public static class ProductCsvImporter
{
    private const char Separator = ';';

    public static ImportResult<IImportItem> Load(string path)
    {
        var items = new List<IImportItem>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            if (number == 1 && line.StartsWith("type", StringComparison.OrdinalIgnoreCase))
                continue;

            switch (ParseLine(line))
            {
                case ParseOk ok:
                    items.Add(ok.Value);
                    break;

                case ParseFailed failed:
                    errors.Add($"рядок {number}: {failed.Reason}");
                    break;
            }
        }

        return new ImportResult<IImportItem>(items, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(
            Separator,
            StringSplitOptions.TrimEntries);

        return parts switch
        {
            ["P", var id, var sku, var name, var unit, var qty]
                when string.IsNullOrWhiteSpace(sku) ||
                     string.IsNullOrWhiteSpace(name)
                => new ParseFailed("SKU або назва порожні"),

            ["P", _, _, _, _, var qty]
                when !int.TryParse(qty, out int q) || q < 0
                => new ParseFailed(
                    $"кількість '{qty}' не є невід'ємним числом"),

            ["P", var id, var sku, var name, var unit, var qty]
                => new ParseOk(
                    new ProductDto(
                        id,
                        sku,
                        name,
                        unit,
                        int.Parse(qty))),

            ["W", var id, var name, var address]
                when string.IsNullOrWhiteSpace(name)
                => new ParseFailed("назва складу порожня"),

            ["W", var id, var name, var address]
                when string.IsNullOrWhiteSpace(address)
                => new ParseFailed("адреса складу порожня"),

            ["W", var id, var name, var address]
                => new ParseOk(
                    new WarehouseDto(
                        id,
                        name,
                        address)),

            ["P", ..] =>
                new ParseFailed(
                    $"для товару очікую 6 колонок, отримав {parts.Length}"),

            ["W", ..] =>
                new ParseFailed(
                    $"для складу очікую 4 колонки, отримав {parts.Length}"),

            [var type, ..] =>
                new ParseFailed($"невідомий тип '{type}'"),

            _ =>
                new ParseFailed("некоректний формат рядка")
        };
    }

    private abstract record ParseOutcome;

    private sealed record ParseOk(IImportItem Value) : ParseOutcome;

    private sealed record ParseFailed(string Reason) : ParseOutcome;
}