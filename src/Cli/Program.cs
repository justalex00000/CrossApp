using Core;
using Core.Abstractions;
using Core.Services;
using Core.Storage;
using Core.Reports;

ICatalogStore store = StoreFactory.Create(args);

if (args.Contains("--cache"))
{
    store = new CachingCatalogStore(store);
}

var service = new CatalogService(store);

Console.WriteLine($"Сховище: {store.GetType().Name}");
Console.WriteLine();

var items = service.All();

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
        Console.WriteLine("=== Групування товарів за одиницями виміру ===");

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

    default:
        Console.WriteLine(
            "Доступні звіти: report-top | report-low | " +
            "report-groups | report-sum");
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