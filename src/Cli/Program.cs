using Core.Dto;
using Core.Import;
using Core.Domain;

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



Console.WriteLine();
ImportResult<Product> domainResult = ProductDomainMapper.Map(result);

Console.WriteLine(
    $"Доменних товарів створено: {domainResult.Items.Count}");

foreach (string error in domainResult.Errors)
    Console.WriteLine($" ! {error}");


Console.WriteLine();
Console.WriteLine("=== Сценарій 1: успіх ===");

Product domainProduct = Product.Create(
    "P-001", "sku-001", "Цемент М400 25кг", "шт", 100);

Console.WriteLine(domainProduct);

domainProduct.RegisterArrival(50);
domainProduct.Issue(30);

Console.WriteLine(domainProduct);

Console.WriteLine();
Console.WriteLine("=== Сценарій 2: порушення інваріантів ===");

TryDo("видача більша за залишок",
    () => domainProduct.Issue(1000));

TryDo("порожній SKU",
    () => Product.Create("P-002", " ", "Пісок", "т", 10));

TryDo("від'ємний залишок",
    () => Product.Create("P-003", "SKU-003", "Цегла", "шт", -5));

ProductDto dto = domainProduct.ToDto();
Product restored = Product.FromDto(dto);

Console.WriteLine();
Console.WriteLine($"Відновлено з DTO: {restored}");

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine(
            $" {title}: виняток НЕ спрацював — інваріант відсутній!");
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            $" {title}: {ex.GetType().Name} — {ex.Message}");
    }
}

Console.WriteLine();
Console.WriteLine("=== Переміщення між позиціями ===");

Product source = Product.Create(
    "P-010", "SKU-010", "Цемент", "шт", 100);

Product destination = Product.Create(
    "P-011", "SKU-010", "Цемент", "шт", 20);

ProductTransferService.Transfer(source, destination, 30);

Console.WriteLine($"Джерело: {source}");
Console.WriteLine($"Призначення: {destination}");


Console.WriteLine();
Console.WriteLine("=== Стани замовлення ===");

Order order = Order.Create("ORD-001");

Console.WriteLine(order.Status);

order.Confirm();
Console.WriteLine(order.Status);

TryDo("повторне підтвердження",
    () => order.Confirm());

order.Cancel();
Console.WriteLine(order.Status);

return 0;