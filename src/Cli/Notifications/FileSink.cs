using Core.Abstractions;
using Core.Events;

namespace Cli.Notifications;

public sealed class FileSink : INotificationSink
{
    private readonly string _path;

    public FileSink(string path)
    {
        _path = Path.GetFullPath(path);
    }

    public void Notify(CatalogChangedEventArgs e)
    {
        Directory.CreateDirectory(
            Path.GetDirectoryName(_path)!);

        string line =
            $"{e.At:HH:mm:ss} {e.Kind,-8} {e.Id} " +
            $"{e.Delta,4:+0;-0} → залишок {e.QuantityAfter}";

        File.AppendAllText(
            _path,
            line + Environment.NewLine);
    }
}
