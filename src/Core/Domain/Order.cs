
namespace Core.Domain;

public enum OrderStatus
{
    Draft,
    Confirmed,
    Cancelled
}

public sealed class Order
{
    public string Id { get; }
    public OrderStatus Status { get; private set; }

    private Order(string id)
    {
        Id = id;
        Status = OrderStatus.Draft;
    }

    public static Order Create(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException(
                "Ідентифікатор замовлення обов'язковий",
                nameof(id));

        return new Order(id.Trim());
    }

    public void Confirm() => ChangeStatus(OrderStatus.Confirmed);

    public void Cancel() => ChangeStatus(OrderStatus.Cancelled);

    private void ChangeStatus(OrderStatus next)
    {
        Status = (Status, next) switch
        {
            (OrderStatus.Draft, OrderStatus.Confirmed)
                => OrderStatus.Confirmed,

            (OrderStatus.Draft, OrderStatus.Cancelled)
                => OrderStatus.Cancelled,

            (OrderStatus.Confirmed, OrderStatus.Cancelled)
                => OrderStatus.Cancelled,

            _ => throw new InvalidOperationException(
                $"Перехід з {Status} до {next} неможливий")
        };
    }
}
