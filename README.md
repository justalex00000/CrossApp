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
│       ├── Abstractions/
│       │   └── ICatalogStore.cs
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
│       ├── Services/
│       │   └── CatalogService.cs
│       ├── Storage/
│       │   ├── InMemoryCatalogStore.cs
│       │   ├── FileCatalogStore.cs
│       │   ├── StoreFactory.cs
│       │   └── CachingCatalogStore.cs
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

### Імпорт даних

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

### Робота з каталогом товарів

Запуск із використанням сховища в пам'яті:

```bash
dotnet run --project src/Cli
```

Запуск із файловим сховищем:

```bash
dotnet run --project src/Cli -- --file
```

Запуск із кешуванням сховища в пам'яті:

```bash
dotnet run --project src/Cli -- --cache
```

Запуск із файловим сховищем та кешуванням:

```bash
dotnet run --project src/Cli -- --file --cache
```

Параметр `--file` вмикає збереження каталогу у JSON-файл. Параметр `--cache` додає декоратор кешування до вибраного сховища.

Програма демонструє додавання товару, прихід і видачу, пошук товарів за залишком, виведення каталогу та обробку помилок.

## Доменна модель

Клас `Product` представляє товар і контролює його стан. Поле залишку є приватним, а властивість `Quantity` доступна лише для читання.

Створення товару виконується через фабричний метод `Create()`, який перевіряє вхідні дані. Методи `RegisterArrival()` та `Issue()` змінюють залишок із перевіркою допустимих значень.

Методи `ToDto()` та `FromDto()` забезпечують перетворення між доменною сутністю та DTO.

### Інваріанти Product

### Інваріанти доменної моделі

**Product - товар**

| Інваріант | Реалізація |
|---|---|
| Ідентифікатор, SKU, назва й одиниця виміру не можуть бути порожніми | `Product.Create()` |
| Початковий залишок не може бути від'ємним | `Product.Create()` |
| Кількість приходу та видачі має бути більшою за нуль | `RegisterArrival()`, `Issue()` |
| Не можна видати більше товару, ніж є на залишку | `Issue()` |

**Переміщення товару**

| Інваріант | Реалізація |
|---|---|
| Джерело та призначення мають бути різними об'єктами | `ProductTransferService.Transfer()` |
| Переміщувати можна лише товари з однаковим SKU | `ProductTransferService.Transfer()` |
| Кількість переміщення має бути більшою за нуль | Перевіряється методом `Issue()` |
| Джерело повинно мати достатній залишок | Перевіряється методом `Issue()` |

**Order - замовлення**

| Інваріант | Реалізація |
|---|---|
| Ідентифікатор замовлення не може бути порожнім | `Order.Create()` |
| Нове замовлення створюється у стані `Draft` | Конструктор `Order` |
| Дозволені переходи: `Draft → Confirmed`, `Draft → Cancelled`, `Confirmed → Cancelled` | `Order.ChangeStatus()` |
| Недопустимі переходи станів заборонені | `Order.ChangeStatus()` |

Перевірки інваріантів виконуються до зміни стану відповідного об'єкта. Якщо умова порушена, генерується виняток, який повідомляє про причину помилки.

### Переміщення та стани замовлення

`ProductTransferService` виконує переміщення між різними позиціями одного товару з однаковим SKU, перевіряючи допустимість операції.

Клас `Order` демонструє керування станами замовлення: `Draft`, `Confirmed` і `Cancelled`. Дозволені переходи визначаються в методі зміни стану; недопустимі переходи викликають виняток.

## Імпорт даних

Програма підтримує CSV та JSON. Для результатів імпорту використовується `ImportResult<T>`, що містить успішно завантажені записи та список помилок.

`ProductDomainMapper` перетворює імпортовані DTO товарів на доменні об'єкти. Дані, що порушують доменні інваріанти, не створюються як доменні сутності.

## Інтерфейс ICatalogStore

`ICatalogStore` визначає спільний контракт для роботи з каталогом товарів. Він надає методи:

- `List()` - отримання списку товарів;
- `GetById(string id)` - пошук товару за ідентифікатором;
- `Add(Product item)` - додавання товару;
- `Update(Product item)` - оновлення товару;
- `Remove(string id)` - видалення товару.

Інтерфейс дозволяє використовувати різні реалізації сховища без зміни бізнес-логіки.

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
