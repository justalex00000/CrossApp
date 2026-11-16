using System.Diagnostics;
using System.Text.Json;

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

var command = args.FirstOrDefault(a => !a.StartsWith("--"))
              ?? "report-top";

switch (command)
{
    case "report-top":
    {
        var items = service.All();
        Console.WriteLine("=== Топ-5 товарів за кількістю ===");
        Print(ReportService.TopByQuantity(items, 5));
        break;
    }

    case "report-low":
    {
        var items = service.All();
        Console.WriteLine("=== Товари із залишком менше 10 ===");
        Print(ReportService.LowStock(items, 10));
        break;
    }

    case "report-groups":
    {
        var items = service.All();
        Console.WriteLine("=== Групування за одиницями виміру ===");

        foreach (var g in ReportService.ByUnit(items))
        {
            Console.WriteLine(
                $" {g.Key,-6} позицій: {g.Count,3} сумарно: {g.TotalQuantity,6}");
        }

        break;
    }

    case "report-sum":
    {
        var items = service.All();
        Console.WriteLine("=== Загальна статистика ===");

        var t = ReportService.Totals(items);

        Console.WriteLine(
            $" позицій {t.Items}, всього {t.Total}, " +
            $"середнє {t.Average:F2}, макс {t.Max}");

        break;
    }

    case "report-groups-query":
    {
        var items = service.All();
        Console.WriteLine("=== Групування за одиницями виміру (query syntax) ===");

        foreach (var g in ReportService.ByUnitQuerySyntax(items))
        {
            Console.WriteLine(
                $" {g.Key,-6} позицій: {g.Count,3} сумарно: {g.TotalQuantity,6}");
        }

        break;
    }

    case "report-top-extension":
    {
        var items = service.All();
        Console.WriteLine("=== Топ-5 товарів через TopBy ===");
        Print(ReportService.TopByQuantityExtension(items, 5));
        break;
    }

    case "report-low-extension":
    {
        var items = service.All();
        Console.WriteLine("=== Товари з найменшою кількістю через TopBy ===");
        Print(ReportService.LowestByQuantityExtension(items, 5));
        break;
    }

    case "report-join":
    {
        var items = service.All();
        Console.WriteLine("=== Звіт із використанням Join ===");

        if (items.Count < 3)
        {
            Console.WriteLine("Для звіту потрібно щонайменше 3 товари.");
            break;
        }

        foreach (var line in ReportService.MovementJoin(items, CreateMovements(items)))
        {
            Console.WriteLine($" {line.Sku,-10} зміна: {line.Delta,6}");
        }

        break;
    }

    case "report-groupjoin":
    {
        var items = service.All();
        Console.WriteLine("=== Звіт із використанням GroupJoin ===");

        if (items.Count < 3)
        {
            Console.WriteLine("Для звіту потрібно щонайменше 3 товари.");
            break;
        }

        foreach (var line in ReportService.MovementGroupJoin(items, CreateMovements(items)))
        {
            Console.WriteLine(
                $" {line.Sku,-10} кількість операцій: {line.Count,4}");
        }

        break;
    }

    case "benchmark":
        Console.WriteLine("=== Порівняння LINQ-запитів ===");
        RunBenchmark();
        break;

    case "report-deferred":
    {
        Console.WriteLine("=== Демонстрація відкладеного виконання LINQ ===");

        var source = service.All().ToList();
        var query = source.Where(p => p.Quantity < 10);

        Console.WriteLine($"Кількість товарів до додавання: {query.Count()}");

        source.Add(Product.Create(
            "tmp00001", "SKU-999", "Тестовий товар", "шт", 3));

        Console.WriteLine($"Кількість товарів після додавання: {query.Count()}");

        var snapshot = query.ToList();
        source.Clear();

        Console.WriteLine(
            $"Кількість у збереженому списку після очищення: {snapshot.Count}");

        break;
    }

    case "import-async":
    {
        Console.WriteLine("=== Асинхронний імпорт JSON із прогресом ===");

        var dataPath = Path.GetFullPath("data/sample.json");

        using var userCts = new CancellationTokenSource();
        using var timeoutCts = new CancellationTokenSource();
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            userCts.Token,
            timeoutCts.Token);

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            userCts.Cancel();
        };

        // Таймаут для демонстрації можна задати аргументом --timeout.
        if (args.Contains("--timeout"))
        {
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(30));
        }

        var progress = new Progress<int>(percent =>
        {
            Console.Write($"\rІмпорт: {percent}%   ");
        });

        try
        {
            var importer = new JsonCatalogImporter();

            var added = await service.ImportAsync(
                importer,
                dataPath,
                linkedCts.Token,
                progress);

            Console.WriteLine();
            Console.WriteLine($"Імпортовано записів: {added}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine();

            if (timeoutCts.IsCancellationRequested &&
                !userCts.IsCancellationRequested)
            {
                Console.WriteLine("Час очікування імпорту вичерпано.");
            }
            else
            {
                Console.WriteLine("Імпорт скасовано.");
            }
        }
        catch (DomainException ex)
        {
            Console.WriteLine($"Дані відхилено: {ex.Message}");
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"Файл не знайдено: {dataPath}");
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Помилка формату JSON: {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Помилка читання файлу: {ex.Message}");
        }

        break;
    }

    case "import-stream":
    {
        Console.WriteLine("=== Потоковий імпорт JSON ===");

        var dataPath = Path.GetFullPath("data/sample.json");

        using var userCts = new CancellationTokenSource();
        using var timeoutCts = new CancellationTokenSource();
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            userCts.Token,
            timeoutCts.Token);

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            userCts.Cancel();
        };

        if (args.Contains("--timeout"))
        {
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(30));
        }

        try
        {
            IStreamingCatalogImporter importer = new JsonCatalogImporter();

            var added = await service.ImportStreamAsync(
                importer.StreamAsync(dataPath, linkedCts.Token),
                linkedCts.Token);

            Console.WriteLine($"Потоково імпортовано записів: {added}");
        }
        catch (OperationCanceledException)
        {
            if (timeoutCts.IsCancellationRequested &&
                !userCts.IsCancellationRequested)
            {
                Console.WriteLine("Час очікування потокового імпорту вичерпано.");
            }
            else
            {
                Console.WriteLine("Потоковий імпорт скасовано.");
            }
        }
        catch (DomainException ex)
        {
            Console.WriteLine($"Дані відхилено: {ex.Message}");
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"Файл не знайдено: {dataPath}");
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Помилка формату JSON: {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Помилка читання файлу: {ex.Message}");
        }

        break;
    }

    default:
        Console.WriteLine("Доступні команди:");
        Console.WriteLine(
            "report-top | report-low | report-groups | report-sum");
        Console.WriteLine(
            "report-groups-query | report-top-extension | report-low-extension");
        Console.WriteLine(
            "report-join | report-groupjoin | benchmark | report-deferred");
        Console.WriteLine("import-async [--timeout]");
        Console.WriteLine("import-stream [--timeout]");
        break;
}

static List<Movement> CreateMovements(IReadOnlyList<Product> items)
{
    return new List<Movement>
    {
        new("M001", items[0].Id, 20, DateTimeOffset.Now),
        new("M002", items[0].Id, -5, DateTimeOffset.Now),
        new("M003", items[1].Id, 10, DateTimeOffset.Now),
        new("M004", items[2].Id, -3, DateTimeOffset.Now),
        new("M005", items[1].Id, -2, DateTimeOffset.Now)
    };
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
