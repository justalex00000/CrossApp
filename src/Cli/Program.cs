using Core;
using Core.Abstractions;
using Core.Services;
using Core.Storage;

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