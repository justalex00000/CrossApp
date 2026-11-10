namespace Core.Domain;

public sealed class Movement
{
    public string Id { get; }
    public string ProductId { get; }
    public int Delta { get; }
    public DateTimeOffset Date { get; }

    public Movement(
        string id,
        string productId,
        int delta,
        DateTimeOffset date)
    {
        Id = id;
        ProductId = productId;
        Delta = delta;
        Date = date;
    }
}