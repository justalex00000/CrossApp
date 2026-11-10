using Core.Domain;

namespace Core.Reports;

public static class ReportService
{
    public static IReadOnlyList<StockLine> LowStock(
        IEnumerable<Product> source,
        int threshold) =>
        source
            .Where(p => p.Quantity < threshold)
            .OrderBy(p => p.Quantity)
            .ThenBy(p => p.Sku, StringComparer.Ordinal)
            .Select(p => new StockLine(
                p.Sku, p.Name, p.Quantity, p.Unit))
            .ToList();


    public static IReadOnlyList<StockLine> TopByQuantity(
        IEnumerable<Product> source,
        int n) =>
        source
            .TopBy(p => p.Quantity, n, p => p.Sku)
            .Select(p => new StockLine(
                p.Sku, p.Name, p.Quantity, p.Unit))
            .ToList();


    public static IReadOnlyList<GroupSummary> ByUnit(
        IEnumerable<Product> source) =>
        source
            .GroupBy(p => p.Unit, StringComparer.OrdinalIgnoreCase)
            .Select(g => new GroupSummary(
                g.Key,
                g.Count(),
                g.Sum(p => p.Quantity)))
            .OrderByDescending(s => s.TotalQuantity)
            .ToList();


    public static (int Items, int Total, double Average, int Max)
        Totals(IEnumerable<Product> source)
    {
        var list = source.ToList();

        if (list.Count == 0)
            return (0, 0, 0d, 0);

        return (
            list.Count,
            list.Sum(p => p.Quantity),
            list.Average(p => p.Quantity),
            list.Max(p => p.Quantity));
    }


    public static IReadOnlyList<GroupSummary> ByUnitQuerySyntax(
        IEnumerable<Product> source)
    {
        var query =
            from product in source
            group product by product.Unit.ToUpperInvariant() into g
            orderby g.Sum(p => p.Quantity) descending
            select new GroupSummary(
                g.Key,
                g.Count(),
                g.Sum(p => p.Quantity));

        return query.ToList();
    }


    public static IReadOnlyList<StockLine> TopByQuantityExtension(
        IEnumerable<Product> source,
        int n) =>
        source
            .TopBy(p => p.Quantity, n, p => p.Sku)
            .Select(p => new StockLine(
                p.Sku, p.Name, p.Quantity, p.Unit))
            .ToList();


    public static IReadOnlyList<StockLine> LowestByQuantityExtension(
        IEnumerable<Product> source,
        int n) =>
        source
            .TopBy(p => -p.Quantity, n, p => p.Sku)
            .Select(p => new StockLine(
                p.Sku, p.Name, p.Quantity, p.Unit))
            .ToList();


    public static IReadOnlyList<MovementLine> MovementJoin(
        IEnumerable<Product> products,
        IEnumerable<Movement> movements) =>
        products
            .Join(
                movements,
                product => product.Id,
                movement => movement.ProductId,
                (product, movement) =>
                    new MovementLine(
                        product.Sku,
                        movement.Delta))
            .ToList();


    public static IReadOnlyList<MovementGroupLine> MovementGroupJoin(
        IEnumerable<Product> products,
        IEnumerable<Movement> movements) =>
        products
            .GroupJoin(
                movements,
                product => product.Id,
                movement => movement.ProductId,
                (product, productMovements) =>
                    new MovementGroupLine(
                        product.Sku,
                        productMovements.Count()))
            .ToList();
}