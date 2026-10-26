using Core;
using Core.Abstractions;
using Core.Services;
using Core.Storage;
using Core.Events;

Console.WriteLine("========== ЛАБОРАТОРНА РОБОТА 5 ==========");
ICatalogStore store = StoreFactory.Create(args);

if (args.Contains("--cache"))
{
    store = new CachingCatalogStore(store);
}

var service = new CatalogService(store);

Console.WriteLine($"Сховище: {store.GetType().Name}");

var created = service.Add(
    "SKU-101",
    "Кабель UTP cat6",
    "м",
    100);

Console.WriteLine($"Додано: {created}");

service.Receive(created.Id, 25);

service.Issue(created.Id, 10);

Console.WriteLine($"Після приходу та видачі: {service.Find(created.Id)}");

Console.WriteLine();
Console.WriteLine("=== Товари із залишком менше 50 ===");

foreach (var product in service.Search(p => p.Quantity < 50))
{
    Console.WriteLine(product);
}

Console.WriteLine();
Console.WriteLine("=== Усі товари ===");

foreach (var product in service.All())
{
    Console.WriteLine(
        $" {product.Id} {product.Sku,-10} " +
        $"{product.Name,-22} {product.Quantity,5} {product.Unit}");
}

try
{
    service.Receive("missing-id", 10);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Очікувана помилка: {ex.Message}");
}

try
{
    store.Add(created);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Очікувана помилка: {ex.Message}");
}


Console.WriteLine();
Console.WriteLine("========== ЛАБОРАТОРНА РОБОТА 6 ==========");

var logPath = Path.Combine(
    AppContext.BaseDirectory,
    "logs",
    "app.log");

Directory.CreateDirectory(
    Path.GetDirectoryName(logPath)!);

void OnCatalogChanged(
    object? sender,
    CatalogChangedEventArgs e)
{
    var line =
        $"{e.At:HH:mm:ss} {e.Kind,-8} {e.Id} " +
        $"{e.Delta,4:+0;-0} → залишок {e.QuantityAfter}";

    Console.WriteLine(line);

    File.AppendAllText(
        logPath,
        line + Environment.NewLine);
}

int changes = 0;

EventHandler<CatalogChangedEventArgs> counterHandler =
    (_, _) => changes++;

service.Changed += OnCatalogChanged;
service.Changed += counterHandler;

var eventProduct = service.Add(
    "SKU-LAB06",
    "Тестовий товар для подій",
    "шт",
    50);

service.Receive(eventProduct.Id, 25);
service.Issue(eventProduct.Id, 10);

Console.WriteLine($"Подій отримано: {changes}");

service.Changed -= OnCatalogChanged;

service.Receive(eventProduct.Id, 5);

Console.WriteLine(
    $"Подій отримано після відписки першого підписника: {changes}");

int changesBeforeError = changes;

try
{
    service.Receive(eventProduct.Id, -5);
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Очікувана помилка: {ex.Message}");
}

Console.WriteLine(
    $"Подій після невдалої операції: {changes} " +
    $"(до операції: {changesBeforeError})");

Console.WriteLine();
Console.WriteLine("=== Пошук товарів із залишком менше 20 ===");

foreach (var product in service.Search(p => p.Quantity < 20))
{
    Console.WriteLine(product);
}

service.Changed -= counterHandler;

Console.WriteLine();
Console.WriteLine($"Журнал подій: {logPath}");
