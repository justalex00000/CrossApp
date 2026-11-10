using System.Diagnostics;

using Core;
using Core.Abstractions;
using Core.Services;
using Core.Storage;
using Core.Reports;
using Core.Domain;

ICatalogStore store = StoreFactory.Create(args);

if (args.Contains("--cache"))
{
    store = new CachingCatalogStore(store);
}

var service = new CatalogService(store);

Console.WriteLine($"Сховище: {store.GetType().Name}");
Console.WriteLine();

var items = service.All();

var movements = new List<Movement>
{
    new("M001", items[0].Id, 20, DateTimeOffset.Now),
    new("M002", items[0].Id, -5, DateTimeOffset.Now),
    new("M003", items[1].Id, 10, DateTimeOffset.Now),
    new("M004", items[2].Id, -3, DateTimeOffset.Now),
    new("M005", items[1].Id, -2, DateTimeOffset.Now)
};

var command = args.FirstOrDefault(a => !a.StartsWith("--"))
              ?? "report-top";

switch (command)
{
    case "report-top":
        Console.WriteLine("=== Топ-5 товарів за кількістю ===");
        Print(ReportService.TopByQuantity(items, 5));
        break;

    case "report-low":
        Console.WriteLine("=== Товари із залишком менше 10 ===");
        Print(ReportService.LowStock(items, 10));
        break;

    case "report-groups":
        Console.WriteLine("=== Групування за одиницями виміру ===");

        foreach (var g in ReportService.ByUnit(items))
        {
            Console.WriteLine(
                $" {g.Key,-6} позицій: {g.Count,3} сумарно: {g.TotalQuantity,6}");
        }

        break;

    case "report-sum":
        Console.WriteLine("=== Загальна статистика ===");

        var t = ReportService.Totals(items);

        Console.WriteLine(
            $" позицій {t.Items}, всього {t.Total}, " +
            $"середнє {t.Average:F2}, макс {t.Max}");

        break;

    case "report-groups-query":
        Console.WriteLine("=== Групування за одиницями виміру (query syntax) ===");

        foreach (var g in ReportService.ByUnitQuerySyntax(items))
        {
            Console.WriteLine(
                $" {g.Key,-6} позицій: {g.Count,3} сумарно: {g.TotalQuantity,6}");
        }

        break;

    case "report-top-extension":
        Console.WriteLine("=== Топ-5 товарів через TopBy ===");
        Print(ReportService.TopByQuantityExtension(items, 5));
        break;

    case "report-low-extension":
        Console.WriteLine("=== Товари з найменшою кількістю через TopBy ===");
        Print(ReportService.LowestByQuantityExtension(items, 5));
        break;

    case "report-join":
        Console.WriteLine("=== Звіт із використанням Join ===");

        foreach (var line in ReportService.MovementJoin(items, movements))
        {
            Console.WriteLine(
                $" {line.Sku,-10} зміна: {line.Delta,6}");
        }

        break;

    case "report-groupjoin":
        Console.WriteLine("=== Звіт із використанням GroupJoin ===");

        foreach (var line in ReportService.MovementGroupJoin(items, movements))
        {
            Console.WriteLine(
                $" {line.Sku,-10} кількість операцій: {line.Count,4}");
        }

        break;

    case "benchmark":
        Console.WriteLine("=== Порівняння LINQ-запитів ===");
        RunBenchmark();
        break;

    default:
        Console.WriteLine("Доступні команди:");
        Console.WriteLine(
            "report-top | report-low | report-groups | report-sum");
        Console.WriteLine(
            "report-groups-query | report-top-extension | report-low-extension");
        Console.WriteLine(
            "report-join | report-groupjoin | benchmark");
        break;
}

static void Print(IReadOnlyList<StockLine> lines)
{
    foreach (var l in lines)
    {
        Console.WriteLine(
            $" {l.Sku,-10} {l.Name,-22} {l.Quantity,6} {l.Unit}");
    }
}

static void RunBenchmark()
{
    var products = Enumerable.Range(1, 100_000)
        .Select(i => Product.Create(
            i.ToString(),
            $"SKU-{i}",
            $"Товар {i}",
            "шт",
            i % 1000))
        .ToList();

    var stopwatch = Stopwatch.StartNew();

    var resultWithList = products
        .Where(p => p.Quantity > 500)
        .ToList()
        .OrderByDescending(p => p.Quantity)
        .Take(10)
        .ToList();

    stopwatch.Stop();
    var timeWithList = stopwatch.ElapsedMilliseconds;

    stopwatch.Restart();

    var resultWithoutList = products
        .Where(p => p.Quantity > 500)
        .OrderByDescending(p => p.Quantity)
        .Take(10)
        .ToList();

    stopwatch.Stop();
    var timeWithoutList = stopwatch.ElapsedMilliseconds;

    Console.WriteLine($"З проміжним ToList(): {timeWithList} мс");
    Console.WriteLine($"Без проміжного ToList(): {timeWithoutList} мс");

    Console.WriteLine(
        $"Результати однакові: {resultWithList.SequenceEqual(resultWithoutList)}");
}