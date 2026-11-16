using Core.Domain;

namespace Core.Tests;

public static class TestData
{
    public static List<Product> Items()
    {
        return new List<Product>
        {
            Product.Create("test001", "SKU-BIG", "Великий товар", "шт", 100),
            Product.Create("test002", "SKU-MID", "Середній товар", "шт", 50),
            Product.Create("test003", "SKU-SMALL", "Малий товар", "шт", 5),
            Product.Create("test004", "SKU-LOW", "Мінімальний товар", "м", 1)
        };
    }
}