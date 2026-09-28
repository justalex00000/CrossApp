using Core.Dto;
using Core.Import;

string path = args.Length > 0
    ? args[0]
    : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine(
        $"Файл не знайдено: {Path.GetFullPath(path)}");

    return 1;
}

ImportResult<IImportItem> result =
    Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".csv" => ProductCsvImporter.Load(path),
        ".json" => ProductJsonImporter.Load(path),
        _ => new ImportResult<IImportItem>(
            [],
            [$"Непідтримуваний формат файлу: {Path.GetExtension(path)}"])
    };

Console.WriteLine($"Завантажено записів: {result.Items.Count}");

foreach (IImportItem item in result.Items.Take(5))
{
    switch (item)
    {
        case ProductDto product:
            Console.WriteLine(
                $" Товар:  {product.Id,-6} {product.Sku,-10} {product.Name,-26} {product.Quantity,5} {product.Unit}");
            break;

        case WarehouseDto warehouse:
            Console.WriteLine(
                $" Склад:  {warehouse.Id,-6} {warehouse.Name,-26} {warehouse.Address}");
            break;
    }
}

if (result.Errors.Count > 0)
{
    Console.WriteLine(
        $"Пропущено рядків: {result.Errors.Count}");

    foreach (string e in result.Errors)
    {
        Console.WriteLine($" ! {e}");
    }
}

int total = result.Items.Count + result.Errors.Count;
int accepted = result.Items.Count;
int skipped = result.Errors.Count;

double errorPercent =
    total == 0 ? 0 : skipped * 100.0 / total;

Console.WriteLine(
    $"Усього: {total}, прийнято: {accepted}, " +
    $"пропущено: {skipped}, помилок: {errorPercent:F1}%");

return 0;