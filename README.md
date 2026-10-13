# CrossApp

Наскрізний проєкт з кросплатформного програмування.

**Предметна область:** склад.  
**Сутності:** Product (товар), StockBatch (партія), Warehouse (склад), Movement (переміщення).  
**Призначення:** облік залишків товарів по партіях.

## Структура проєкту

```text
CrossApp/
├── src/
│   ├── Cli/
│   │   ├── Program.cs
│   │   └── Cli.csproj
│   └── Core/
│       ├── Domain/
│       │   ├── Product.cs
│       │   ├── ProductTransferService.cs
│       │   └── Order.cs
│       ├── Dto/
│       │   ├── ProductDto.cs
│       │   ├── IImportItem.cs
│       │   ├── ImportResult.cs
│       │   └── WarehouseDto.cs
│       ├── Import/
│       │   ├── ProductCsvImporter.cs
│       │   ├── ProductJsonImporter.cs
│       │   └── ProductDomainMapper.cs
│       ├── EnvironmentInfo.cs
│       └── Core.csproj
├── data/
│   ├── sample.csv
│   └── sample.json
├── CrossApp.sln
└── README.md
```

## Збірка

```bash
dotnet build
```

## Запуск

Запуск із файлом за замовчуванням:

```bash
dotnet run --project src/Cli
```

Запуск із CSV-файлом:

```bash
dotnet run --project src/Cli -- data/sample.csv
```

Запуск із JSON-файлом:

```bash
dotnet run --project src/Cli -- data/sample.json
```

Програма імпортує дані, виводить завантажені записи та повідомлення про помилки, створює доменні об'єкти й демонструє перевірку інваріантів.

## Доменна модель

Клас `Product` представляє товар і контролює його стан. Поле залишку є приватним, а властивість `Quantity` доступна лише для читання.

Створення товару виконується через фабричний метод `Create()`, який перевіряє вхідні дані. Методи `RegisterArrival()` та `Issue()` змінюють залишок із перевіркою допустимих значень.

Методи `ToDto()` та `FromDto()` забезпечують перетворення між доменною сутністю та DTO.

### Інваріанти Product

| Інваріант | Перевірка |
|---|---|
| Ідентифікатор, SKU, назва й одиниця виміру не можуть бути порожніми | `Create()` |
| Початковий залишок не може бути від'ємним | `Create()` |
| Кількість приходу та видачі має бути більшою за нуль | `RegisterArrival()`, `Issue()` |
| Не можна видати більше товару, ніж є на залишку | `Issue()` |

Для некоректних аргументів використовуються `ArgumentException` і `ArgumentOutOfRangeException`, а для недопустимих операцій - `InvalidOperationException`.

### Переміщення та стани замовлення

`ProductTransferService` виконує переміщення між різними позиціями одного товару з однаковим SKU, перевіряючи допустимість операції.

Клас `Order` демонструє керування станами замовлення: `Draft`, `Confirmed` і `Cancelled`. Дозволені переходи визначаються в методі зміни стану; недопустимі переходи викликають виняток.

## Імпорт даних

Програма підтримує CSV та JSON. Для результатів імпорту використовується `ImportResult<T>`, що містить успішно завантажені записи та список помилок.

`ProductDomainMapper` перетворює імпортовані DTO товарів на доменні об'єкти. Дані, що порушують доменні інваріанти, не створюються як доменні сутності.

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

- **Self-contained** - містить .NET Runtime, тому встановлення .NET не потрібне.
- **Framework-dependent** - не містить Runtime, тому на системі має бути встановлений сумісний .NET Runtime.

## Середовище

- Windows 11 x64
- Cli — .NET 10.0
- Core — .NET 8.0 та .NET 10.0
- Мова програмування — C#
