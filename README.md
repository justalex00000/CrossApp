# CrossApp

Наскрізний проєкт з крос-платформного програмування.  
Предметна область: Склад. Сутності: Product (товар), StockBatch (партія), Warehouse (склад), Movement (переміщення).  
Призначення: облік залишків товарів по партіях.  

## Структура проєкту

```text
CrossApp/
├── src/
│   ├── Cli/
│   │   ├── Program.cs
│   │   └── Cli.csproj
│   └── Core/
│       ├── EnvironmentInfo.cs
│       └── Core.csproj
├── CrossApp.sln
└── README.md
```

## Збірка

```bash
dotnet build
```

## Запуск

```bash
dotnet run --project src/Cli
```

## Публікація

### Self-contained

```bash
dotnet publish src/Cli -c Release -r win-x64 --self-contained true
```

### Framework-dependent

```bash
dotnet publish src/Cli -c Release -r win-x64 --self-contained false
```

### Single-file

```bash
dotnet publish src/Cli -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

### Trimmed

```bash
dotnet publish src/Cli -c Release -r win-x64 --self-contained true -p:PublishTrimmed=true
```

## Результати публікації

| RID     | Режим               | Розмір | Чи потрібен встановлений runtime |
| ------- | ------------------- | -----: | -------------------------------- |
| win-x64 | Self-contained      |  78 MB | Ні                               |
| win-x64 | Framework-dependent | 229 KB | Так(.NET10)                      |
| win-x64 | Single-file         |  71 MB | Ні                               |
| win-x64 | Trimmed             |  20 MB | Ні                               |

## Середовище

* Windows 11 x64
* Cli - .NET 10.0
* Core - .NET 8.0; .NET 10.0
