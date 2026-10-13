
using Core.Domain;
using Core.Dto;

namespace Core.Import;

public static class ProductDomainMapper
{
    public static ImportResult<Product> Map(
        ImportResult<IImportItem> source)
    {
        var items = new List<Product>();
        var errors = new List<string>(source.Errors);

        int number = 0;

        foreach (IImportItem item in source.Items)
        {
            number++;

            if (item is not ProductDto dto)
                continue;

            try
            {
                items.Add(Product.FromDto(dto));
            }
            catch (ArgumentException ex)
            {
                errors.Add($"запис {number}: {ex.Message}");
            }
        }

        return new ImportResult<Product>(items, errors);
    }
}
