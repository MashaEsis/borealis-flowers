using borealis_flowers.api.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace borealis_flowers.api.Data;
/// <summary>
/// Seeds test data for statistics: Customers, DateSchedules, and booked Timeslots.
/// Designed to provide data filterable by year, month, and specialist.
/// </summary>
public static class StatisticsDataSeeder
{
    public static async Task SeedStatisticsTestDataAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DataContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataContext>>();

        // Ensure database schema is up to date
        try
        {
            await db.Database.MigrateAsync();
        }
        catch (Microsoft.Data.Sqlite.SqliteException ex)
        {
            logger.LogWarning("Migration failed: {Message}. Recreating database...", ex.Message);

            // Close the broken connection so file handles are released
            await db.Database.CloseConnectionAsync();

            // Extract file path from the connection string (e.g. "Data Source=Data/sqlite/BorealisFlowers.db")
            var connectionString = db.Database.GetConnectionString() ?? "";
            var dbPath = connectionString
                .Split(';')
                .Select(s => s.Trim())
                .FirstOrDefault(s => s.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
                ?.Substring("Data Source=".Length)
                .Trim();

            if (!string.IsNullOrEmpty(dbPath))
            {
                // Delete main db file and WAL/SHM companions
                foreach (var file in new[] { dbPath, dbPath + "-shm", dbPath + "-wal" })
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                        logger.LogInformation("Deleted: {File}", file);
                    }
                }
            }

            // Clear SQLite connection pool so new connections don't reuse the old broken one
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

            await db.Database.EnsureCreatedAsync();
            logger.LogInformation("Database recreated successfully.");
        }

        // Only seed if there are no booked timeslots yet
        var hasBookedTimeslots = await db.Timeslots.AnyAsync(t => !t.Available && t.CustomerId != null);
        if (hasBookedTimeslots)
        {
            logger.LogInformation("Statistics test data already exists, skipping seed.");
            return;
        }

        logger.LogInformation("Seeding statistics test data...");

        // --- Test Customers ---
        var customers = new List<Customer>
        {
            new() { Id = Guid.Parse("a1000000-0000-0000-0000-000000000001"), Name = "Анна Иванова",     Phone = "+375291111111", Email = "anna@test.com",     FirstVisit = new DateTime(2025, 1, 10), LastVisit = new DateTime(2026, 2, 15) },
            new() { Id = Guid.Parse("a1000000-0000-0000-0000-000000000002"), Name = "Борис Петров",     Phone = "+375292222222", Email = "boris@test.com",    FirstVisit = new DateTime(2025, 1, 12), LastVisit = new DateTime(2026, 1, 20) },
            new() { Id = Guid.Parse("a1000000-0000-0000-0000-000000000003"), Name = "Виктория Сидорова", Phone = "+375293333333", Email = "vika@test.com",     FirstVisit = new DateTime(2025, 2, 5),  LastVisit = new DateTime(2026, 2, 10) },
            new() { Id = Guid.Parse("a1000000-0000-0000-0000-000000000004"), Name = "Григорий Козлов",  Phone = "+375294444444", Email = "grigory@test.com",  FirstVisit = new DateTime(2025, 3, 1),  LastVisit = new DateTime(2025, 12, 20) },
            new() { Id = Guid.Parse("a1000000-0000-0000-0000-000000000005"), Name = "Дарья Новикова",   Phone = "+375295555555", Email = "darya@test.com",    FirstVisit = new DateTime(2025, 5, 15), LastVisit = new DateTime(2026, 2, 1) },
            new() { Id = Guid.Parse("a1000000-0000-0000-0000-000000000006"), Name = "Елена Морозова",   Phone = "+375296666666", Email = "elena@test.com",    FirstVisit = new DateTime(2025, 6, 1),  LastVisit = new DateTime(2026, 1, 5) },
            new() { Id = Guid.Parse("a1000000-0000-0000-0000-000000000007"), Name = "Жанна Волкова",    Phone = "+375297777777", Email = "zhanna@test.com",   FirstVisit = new DateTime(2025, 7, 10), LastVisit = new DateTime(2025, 12, 30) },
            new() { Id = Guid.Parse("a1000000-0000-0000-0000-000000000008"), Name = "Игорь Лебедев",    Phone = "+375298888888", Email = "igor@test.com",     FirstVisit = new DateTime(2025, 9, 1),  LastVisit = new DateTime(2026, 2, 20) },
        };
        // Only add customers that don't already exist
        var existingCustomerIds = await db.Customers.Select(c => c.Id).ToListAsync();
        var newCustomers = customers.Where(c => !existingCustomerIds.Contains(c.Id)).ToList();
        if (newCustomers.Any())
        {
            await db.Customers.AddRangeAsync(newCustomers);
        }

        // Specialists from existing seed data
        var specialistCecile = Guid.Parse("dfe327cd-3efc-42f5-8dfc-f3bce55a49b7"); // Hair - Cecile Hahn
        var specialistFrancisco = Guid.Parse("278666b8-3503-47b0-b5f6-7139563dace6"); // Hair - Francisco Gutkowski
        var specialistWaino = Guid.Parse("b23a6e06-ce61-4445-be74-0cfc5f0a0729"); // Nail - Waino Rath

        // --- Generate DateSchedules + Timeslots across several months/years ---
        var schedules = new List<DateSchedule>();
        var timeslots = new List<Timeslot>();
        var random = new Random(42); // fixed seed for reproducibility

        // Define periods: (year, month, specialist, workingDays in that month)
        var periods = new[]
        {
            // 2025
            (2025, 1,  specialistCecile,    new[] { 13, 14, 15, 16, 17, 20, 21, 22, 23, 24 }),
            (2025, 1,  specialistFrancisco,  new[] { 13, 14, 15, 16, 17, 20, 21, 22 }),
            (2025, 1,  specialistWaino,      new[] { 14, 15, 16, 17, 20, 21 }),
            (2025, 2,  specialistCecile,     new[] { 3, 4, 5, 6, 7, 10, 11, 12, 13, 14 }),
            (2025, 2,  specialistFrancisco,  new[] { 3, 4, 5, 6, 7, 10, 11 }),
            (2025, 2,  specialistWaino,      new[] { 4, 5, 6, 7, 10, 11, 12 }),
            (2025, 3,  specialistCecile,     new[] { 3, 4, 5, 6, 7, 10, 11, 12 }),
            (2025, 3,  specialistFrancisco,  new[] { 3, 4, 5, 6, 7 }),
            (2025, 5,  specialistCecile,     new[] { 5, 6, 7, 12, 13, 14, 19, 20 }),
            (2025, 5,  specialistWaino,      new[] { 5, 6, 7, 12, 13 }),
            (2025, 7,  specialistCecile,     new[] { 1, 2, 3, 7, 8, 9, 14, 15 }),
            (2025, 7,  specialistFrancisco,  new[] { 1, 2, 3, 7, 8, 9 }),
            (2025, 9,  specialistCecile,     new[] { 1, 2, 3, 4, 5, 8, 9, 10 }),
            (2025, 9,  specialistWaino,      new[] { 1, 2, 3, 4, 5 }),
            (2025, 11, specialistCecile,     new[] { 3, 4, 5, 6, 7, 10, 11, 12 }),
            (2025, 11, specialistFrancisco,  new[] { 3, 4, 5, 6, 7 }),
            (2025, 12, specialistCecile,     new[] { 1, 2, 3, 4, 5, 8, 9, 10, 11, 12 }),
            (2025, 12, specialistFrancisco,  new[] { 1, 2, 3, 4, 5 }),
            (2025, 12, specialistWaino,      new[] { 1, 2, 3, 4, 5, 8, 9 }),
            // 2026
            (2026, 1,  specialistCecile,     new[] { 5, 6, 7, 8, 9, 12, 13, 14, 15, 16 }),
            (2026, 1,  specialistFrancisco,  new[] { 5, 6, 7, 8, 9, 12, 13, 14 }),
            (2026, 1,  specialistWaino,      new[] { 6, 7, 8, 9, 12, 13 }),
            (2026, 2,  specialistCecile,     new[] { 2, 3, 4, 5, 6, 9, 10, 11, 12, 13, 16, 17, 18, 19, 20 }),
            (2026, 2,  specialistFrancisco,  new[] { 2, 3, 4, 5, 6, 9, 10, 11, 12 }),
            (2026, 2,  specialistWaino,      new[] { 3, 4, 5, 6, 9, 10, 11, 12, 13 }),
        };

        foreach (var (year, month, specialistId, days) in periods)
        {
            foreach (var day in days)
            {
                var scheduleId = Guid.NewGuid();
                schedules.Add(new DateSchedule
                {
                    Id = scheduleId,
                    SpecialistId = specialistId,
                    Date = new DateTime(year, month, day),
                    IsWorkingDay = true,
                    IsAvailable = true
                });

                // Create timeslots 8:00 - 19:00 for each working day
                for (int hour = 8; hour < 20; hour++)
                {
                    var tsId = Guid.NewGuid();
                    // ~40% chance this slot is booked
                    bool isBooked = random.Next(100) < 40;
                    var customer = isBooked ? customers[random.Next(customers.Count)] : null;

                    timeslots.Add(new Timeslot
                    {
                        Id = tsId,
                        Time = hour,
                        Available = !isBooked,
                        DateScheduleId = scheduleId,
                        CustomerId = customer?.Id
                    });
                }
            }
        }

        await db.DateSchedules.AddRangeAsync(schedules);
        await db.Timeslots.AddRangeAsync(timeslots);
        await db.SaveChangesAsync();

        var bookedCount = timeslots.Count(t => !t.Available);
        logger.LogInformation(
            "Statistics test data seeded: {Customers} customers, {Schedules} date schedules, {Timeslots} timeslots ({Booked} booked).",
            newCustomers.Count, schedules.Count, timeslots.Count, bookedCount);
    }

    /// <summary>
    /// Seeds visit history for ivan.lukyanau@leverx.com:
    /// - several past visits (before today 2026-02-23)
    /// - upcoming visits on 2026-02-28 and 2026-03-08
    /// </summary>
    public static async Task SeedIvanVisitHistoryAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DataContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataContext>>();

        var ivanCustomerId = Guid.Parse("5A330A97-91E2-4161-8934-634799366021");

        // Check if already seeded
        var alreadySeeded = await db.Timeslots.AnyAsync(t => t.CustomerId == ivanCustomerId);
        // if (alreadySeeded)
        // {
        //     logger.LogInformation("Ivan's visit history already exists, skipping seed.");
        //     return;
        // }

        logger.LogInformation("Seeding visit history for ivan.lukyanau@leverx.com...");

        // Ensure customer exists (upsert)
        var existingIvan = await db.Customers.FirstOrDefaultAsync(c => c.Email == "sng.oi.one@gmail.com");
        if (existingIvan == null)
        {
            existingIvan = new Customer
            {
                Id = ivanCustomerId,
                Name = "Ivan Lukyanau",
                Phone = "+375290000000",
                Email = "ivan.lukyanau@leverx.com",
                IsAdmin = false,
                IsMaster = false,
                FirstVisit = new DateTime(2025, 6, 10),
                LastVisit = new DateTime(2026, 2, 20)
            };
            await db.Customers.AddAsync(existingIvan);
        }
        else
        {
            ivanCustomerId = existingIvan.Id;
        }

        // Specialists
        var cecileId = Guid.Parse("dfe327cd-3efc-42f5-8dfc-f3bce55a49b7"); // Cecile Hahn (Hair)
        var franciscoId = Guid.Parse("278666b8-3503-47b0-b5f6-7139563dace6"); // Francisco Gutkowski (Hair)
        var wainoId = Guid.Parse("b23a6e06-ce61-4445-be74-0cfc5f0a0729"); // Waino Rath (Nail)

        // Define visits: (date, hour, specialistId)
        var visits = new[]
        {
            // --- Past visits ---
            (new DateTime(2025, 6, 10), 10, cecileId),     // June 2025
            (new DateTime(2025, 8, 15), 14, franciscoId),  // August 2025
            (new DateTime(2025, 12, 5), 9,  cecileId),     // December 2025
            (new DateTime(2026, 1, 14), 16, franciscoId),  // January 2026
            (new DateTime(2026, 2, 18), 13, cecileId),     // February 2026 (past)
            // --- Upcoming visits ---
            (new DateTime(2026, 2, 28), 11, cecileId),     // 28 Feb 2026
            (new DateTime(2026, 2, 28), 15, franciscoId),  // 28 Feb 2026 (another specialist)
            (new DateTime(2026, 3, 8),  14, cecileId),     // 8 March 2026 (another specialist)
        };

        var nailVisits = new[]
        {
            // --- Past visits ---
            (new DateTime(2025, 10, 3), 11, wainoId),      // October 2025
            (new DateTime(2026, 2, 10), 10, wainoId),      // February 2026 (past)
            // --- Upcoming visits ---
            (new DateTime(2026, 3, 28),  10, wainoId),      // 8 March 2026
        };

        var schedules = new List<DateSchedule>();
        var timeslots = new List<Timeslot>();

        foreach (var (date, hour, specialistId) in nailVisits)
        {
            // Check if a DateSchedule already exists for this specialist+date
            var existingSchedule = await db.DateSchedules
                .FirstOrDefaultAsync(ds => ds.SpecialistId == specialistId && ds.Date == date);

            Guid scheduleId;
            if (existingSchedule != null)
            {
                scheduleId = existingSchedule.Id;
            }
            else
            {
                scheduleId = Guid.NewGuid();
                schedules.Add(new DateSchedule
                {
                    Id = scheduleId,
                    SpecialistId = specialistId,
                    Date = date,
                    IsWorkingDay = true,
                    IsAvailable = true
                });
            }

            timeslots.Add(new Timeslot
            {
                Id = Guid.NewGuid(),
                Time = hour,
                Available = false,
                DateScheduleId = scheduleId,
                CustomerId = ivanCustomerId
            });
        }

        if (schedules.Any())
            await db.DateSchedules.AddRangeAsync(schedules);

        await db.Timeslots.AddRangeAsync(timeslots);
        await db.SaveChangesAsync();

        logger.LogInformation(
            "Ivan's visit history seeded: {Schedules} new date schedules, {Timeslots} booked timeslots ({Past} past, {Upcoming} upcoming).",
            schedules.Count, timeslots.Count,
            visits.Count(v => v.Item1 < DateTime.Today),
            visits.Count(v => v.Item1 >= DateTime.Today));
    }
    public static async Task SeedDefredusVisitHistoryAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DataContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataContext>>();

        var defredusCustomerId = Guid.Parse("d1000000-0000-0000-0000-000000000001");

        // Check if already seeded
        var alreadySeeded = await db.Timeslots.AnyAsync(t => t.CustomerId == defredusCustomerId);
        if (alreadySeeded)
        {
            logger.LogInformation("Defredus visit history already exists, skipping seed.");
            return;
        }

        logger.LogInformation("Seeding visit history for defred885@gmail.com...");

        // Ensure customer exists (upsert)
        var existingDefredus = await db.Customers.FirstOrDefaultAsync(c => c.Email == "defred885@gmail.com");
        if (existingDefredus == null)
        {
            existingDefredus = new Customer
            {
                Id = defredusCustomerId,
                Name = "Александр Крючков",
                Phone = "+375255555555",
                Email = "defred885@gmail.com",
                IsAdmin = false,
                IsMaster = false,
                FirstVisit = new DateTime(2025, 6, 10),
                LastVisit = new DateTime(2026, 2, 20)
            };
            await db.Customers.AddAsync(existingDefredus);
        }
        else
        {
            defredusCustomerId = existingDefredus.Id;
        }

        // Specialists
        var cecileId = Guid.Parse("dfe327cd-3efc-42f5-8dfc-f3bce55a49b7"); // Cecile Hahn (Hair)
        var franciscoId = Guid.Parse("278666b8-3503-47b0-b5f6-7139563dace6"); // Francisco Gutkowski (Hair)
        var wainoId = Guid.Parse("b23a6e06-ce61-4445-be74-0cfc5f0a0729"); // Waino Rath (Nail)

        // Define visits: (date, hour, specialistId)
        var visits = new[]
        {
            // --- Past visits ---
            (new DateTime(2025, 6, 11), 10, cecileId),     // June 2025
            (new DateTime(2025, 8, 16), 14, franciscoId),  // August 2025
            (new DateTime(2025, 10, 4), 11, wainoId),      // October 2025
            (new DateTime(2025, 12, 6), 9,  cecileId),     // December 2025
            (new DateTime(2026, 1, 15), 16, franciscoId),  // January 2026
            (new DateTime(2026, 2, 11), 10, wainoId),      // February 2026 (past)
            (new DateTime(2026, 2, 19), 13, cecileId),     // February 2026 (past)
            // --- Upcoming visits ---
            (new DateTime(2026, 2, 27), 11, cecileId),     // 28 Feb 2026
            (new DateTime(2026, 2, 27), 15, franciscoId),  // 28 Feb 2026 (another specialist)
            (new DateTime(2026, 3, 9),  10, wainoId),      // 8 March 2026
            (new DateTime(2026, 3, 9),  14, cecileId),     // 8 March 2026 (another specialist)
        };

        var schedules = new List<DateSchedule>();
        var timeslots = new List<Timeslot>();

        foreach (var (date, hour, specialistId) in visits)
        {
            // Check if a DateSchedule already exists for this specialist+date
            var existingSchedule = await db.DateSchedules
                .FirstOrDefaultAsync(ds => ds.SpecialistId == specialistId && ds.Date == date);

            Guid scheduleId;
            if (existingSchedule != null)
            {
                scheduleId = existingSchedule.Id;
            }
            else
            {
                scheduleId = Guid.NewGuid();
                schedules.Add(new DateSchedule
                {
                    Id = scheduleId,
                    SpecialistId = specialistId,
                    Date = date,
                    IsWorkingDay = true,
                    IsAvailable = true
                });
            }

            timeslots.Add(new Timeslot
            {
                Id = Guid.NewGuid(),
                Time = hour,
                Available = false,
                DateScheduleId = scheduleId,
                CustomerId = defredusCustomerId
            });
        }

        if (schedules.Any())
            await db.DateSchedules.AddRangeAsync(schedules);

        await db.Timeslots.AddRangeAsync(timeslots);
        await db.SaveChangesAsync();

        logger.LogInformation(
            "Defredus visit history seeded: {Schedules} new date schedules, {Timeslots} booked timeslots ({Past} past, {Upcoming} upcoming).",
            schedules.Count, timeslots.Count,
            visits.Count(v => v.Item1 < DateTime.Today),
            visits.Count(v => v.Item1 >= DateTime.Today));
    }
}

