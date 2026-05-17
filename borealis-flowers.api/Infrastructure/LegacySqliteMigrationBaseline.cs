using borealis_flowers.api.Data;
using Microsoft.EntityFrameworkCore;

namespace borealis_flowers.api.Infrastructure;

/// <summary>
/// Если база уже содержит таблицы (например, перенесённый SQLite-файл), а строки о применении
/// <see cref="InitialMigrationId"/> в <c>__EFMigrationsHistory</c> нет, то <see cref="DatabaseFacade.Migrate"/>
/// снова выполнит <c>CREATE TABLE</c> и упадёт. Один раз помечаем начальную миграцию как применённую.
/// </summary>
public static class LegacySqliteMigrationBaseline
{
    /// <summary>Должен совпадать с атрибутом [Migration("…")] у <c>InitialCreate</c> в Data/Migrations.</summary>
    public const string InitialMigrationId = "20260517110359_InitialCreate";

    /// <summary>Совпадает с HasAnnotation("ProductVersion", …) в snapshot / Designer.</summary>
    public const string ProductVersion = "10.0.3";

    public static void StampInitialMigrationIfLegacyDatabase(DataContext db)
    {
        db.Database.ExecuteSqlRaw(
            """
            CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                "MigrationId" TEXT NOT NULL PRIMARY KEY,
                "ProductVersion" TEXT NOT NULL
            );
            """);

        db.Database.ExecuteSqlRaw(
            """
            INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            SELECT {0}, {1}
            WHERE EXISTS (SELECT 1 FROM sqlite_master WHERE type='table' AND name='Customers')
              AND NOT EXISTS (
                  SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = {0});
            """,
            InitialMigrationId,
            ProductVersion);
    }
}
