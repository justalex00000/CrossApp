using System.Runtime.InteropServices;
using System.Text.Json;

var osDescription = RuntimeInformation.OSDescription;
var osEnvironment = Environment.OSVersion.ToString();
var architecture = RuntimeInformation.ProcessArchitecture.ToString();
var dotnetVersion = Environment.Version.ToString();
var runtime = RuntimeInformation.FrameworkDescription;
var appDirectory = AppContext.BaseDirectory;
var currentDirectory = Environment.CurrentDirectory;
var student = "Курій Олексій - ФеІ-34";
var subject = "Склад";

if (args.Contains("--json"))
{
    var info = new
    {
        Student = student,
        OSDescription = osDescription,
        OSEnvironment = osEnvironment,
        Architecture = architecture,
        DotnetVersion = dotnetVersion,
        Runtime = runtime,
        AppDirectory = appDirectory,
        CurrentDirectory = currentDirectory,
        Subject = subject
    };

    var options = new JsonSerializerOptions
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    Console.WriteLine(JsonSerializer.Serialize(info, options));
}
else
{
    Console.WriteLine("CrossApp – практикум з крос-платформного програмування");
    Console.WriteLine($"Студент: {student}");
    Console.WriteLine(new string('-', 52));

    Console.WriteLine($"ОС (OSDescription)      : {osDescription}");
    Console.WriteLine($"ОС (Environment)        : {osEnvironment}");
    Console.WriteLine($"Архітектура процесу     : {architecture}");
    Console.WriteLine($"Версія .NET (CLR)       : {dotnetVersion}");
    Console.WriteLine($"Runtime                 : {runtime}");
    Console.WriteLine($"Каталог застосунку      : {appDirectory}");
    Console.WriteLine($"Поточний каталог        : {currentDirectory}");

    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"Предметна область       : {subject}");
}
