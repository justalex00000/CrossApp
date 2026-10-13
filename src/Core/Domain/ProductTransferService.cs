
namespace Core.Domain;

public static class ProductTransferService
{
    public static void Transfer(
        Product source,
        Product destination,
        int amount)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        if (ReferenceEquals(source, destination))
            throw new InvalidOperationException(
                "Джерело та призначення мають бути різними товарами");

        if (source.Sku != destination.Sku)
            throw new InvalidOperationException(
                "Можна переміщувати лише товари з однаковим SKU");

        source.Issue(amount);
        destination.RegisterArrival(amount);
    }
}
