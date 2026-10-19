using Core.Domain;

namespace Core;

public static class SampleData
{
    public static IEnumerable<Product> Products()
    {
        return
        [
            Product.Create("P-001", "SKU-001", "Цемент М400", "шт", 120),
            Product.Create("P-002", "SKU-002", "Пісок будівельний", "т", 18),
            Product.Create("P-003", "SKU-003", "Цегла червона", "шт", 4200),
            Product.Create("P-004", "SKU-004", "Щебінь", "т", 35),
            Product.Create("P-005", "SKU-005", "Дошка соснова", "шт", 250),
            Product.Create("P-006", "SKU-006", "Гіпсокартон", "лист", 90),
            Product.Create("P-007", "SKU-007", "Шпаклівка", "мішок", 45),
            Product.Create("P-008", "SKU-008", "Фарба біла", "відро", 32),
            Product.Create("P-009", "SKU-009", "Плитка керамічна", "м2", 75),
            Product.Create("P-010", "SKU-010", "Кабель електричний", "м", 500),
            Product.Create("P-011", "SKU-011", "Труба пластикова", "м", 160),
            Product.Create("P-012", "SKU-012", "Саморізи", "упаковка", 80),
            Product.Create("P-013", "SKU-013", "Утеплювач", "упаковка", 60),
            Product.Create("P-014", "SKU-014", "Ґрунтовка", "л", 100),
            Product.Create("P-015", "SKU-015", "Піна монтажна", "балон", 40)
        ];
    }
}