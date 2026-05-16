11 May 2025 - both of databases are updated and the same: chopme.db and visit-me.db
17 May 2025 - visit-me.db is deleted, not necessary to have both, we can remove it ang apply migration again


Statistics

Создан Data/StatisticsDataSeeder.cs
Это extension method для WebApplication, который при запуске в Development-режиме наполняет базу тестовыми данными:
8 тестовых клиентов — с реалистичными русскими именами, телефонами и email.
DateSchedules и Timeslots распределены по:
2 года: 2025 и 2026
Множество месяцев: январь, февраль, март, май, июль, сентябрь, ноябрь, декабрь 2025 + январь, февраль 2026
3 специалиста: Cecile Hahn (Hair), Francisco Gutkowski (Hair), Waino Rath (Nail)
Каждый рабочий день содержит 12 таймслотов (8:00–19:00)
~40% слотов помечены как забронированные (Available = false, привязан случайный клиент)
Защита от повторного сида: если в таблице Customers уже есть записи, сидер пропускается.
Обновлён Program.cs
Добавлен вызов await app.SeedStatisticsTestDataAsync() внутри проверки IsDevelopment() — данные сидятся только в dev-среде.
Как это тестировать
После запуска приложения можно вызвать:
POST /statistics с { "date": "2026-02-10" } — записи за конкретный день
POST /statistics с { "date": "2026-02-10", "specialistId": "dfe327cd-3efc-42f5-8dfc-f3bce55a49b7" } — за день + конкретный специалист
POST /statistics/month с { "monthName": "февраль", "year": 2026 } — все записи за месяц
POST /statistics/month с { "monthName": "январь", "year": 2025, "specialistId": "278666b8-3503-47b0-b5f6-7139563dace6" } — за месяц + специалист


SQL Commands
sqlite3 chopme.db ".tables" 2>&1

