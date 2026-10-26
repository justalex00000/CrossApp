using Core.Abstractions;
using Core.Events;

namespace Cli.Notifications;

public sealed class ConsoleSink : INotificationSink
{
    public void Notify(CatalogChangedEventArgs e)
    {
        Console.WriteLine(
            $"[ConsoleSink] {e.Kind} | ID: {e.Id} | " +
            $"Зміна: {e.Delta:+0;-0} | " +
            $"Залишок: {e.QuantityAfter}");
    }
}
