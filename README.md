# CrossApp

Наскрізний проєкт з крос-платформного програмування.  
Предметна область: Склад. Сутності: Product (товар), StockBatch (партія), Warehouse (склад), Movement (переміщення).  
Призначення: облік залишків товарів по партіях.  

## Запуск

dotnet build  
dotnet run --project src/Cli  
Для отримання інформації одним JSON-рядком використовується:  
dotnet run --project src/Cli --json

## Середовище

.NET SDK 8.0, Windows 11 x64

## Розміри self-contained публікацій

win-x64   | 71 MB |  
linux-x64 | 71 MB |