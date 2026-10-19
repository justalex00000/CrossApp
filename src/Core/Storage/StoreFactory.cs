using Core;
using Core.Abstractions;

namespace Core.Storage;

public static class StoreFactory
{
    public static ICatalogStore Create(string[] args)
    {
        if (args.Contains("--file"))
        {
            string path = Path.Combine(
                AppContext.BaseDirectory,
                "data",
                "catalog.json");

            return new FileCatalogStore(path);
        }

        return new InMemoryCatalogStore(SampleData.Products());
    }
}