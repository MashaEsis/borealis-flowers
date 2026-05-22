using borealis_flowers.api.Data;
using Microsoft.EntityFrameworkCore;

namespace borealis_flowers.api.Infrastructure;

/// <summary>
/// Добавляет колонки каталога в существующую SQLite-базу, если миграция ещё не применилась.
/// </summary>
public static class ServiceCatalogSchemaPatcher
{
    public const string MigrationId = "20260521133529_AddServiceCatalogFields";

    public static void Apply(DataContext db)
    {
        if (!ColumnExists(db, "Services", "FlowerComposition"))
        {
            db.Database.ExecuteSqlRaw(
                """ALTER TABLE "Services" ADD COLUMN "FlowerComposition" TEXT NULL;""");
        }

        if (!ColumnExists(db, "Services", "ImageUrl"))
        {
            db.Database.ExecuteSqlRaw(
                """ALTER TABLE "Services" ADD COLUMN "ImageUrl" TEXT NULL;""");
        }

        db.Database.ExecuteSqlRaw(
            """
            INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            SELECT {0}, {1}
            WHERE NOT EXISTS (
                SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = {0});
            """,
            MigrationId,
            LegacySqliteMigrationBaseline.ProductVersion);
    }

    static bool ColumnExists(DataContext db, string table, string column)
    {
        var connection = db.Database.GetDbConnection();
        bool wasOpen = connection.State == System.Data.ConnectionState.Open;
        if (!wasOpen)
            connection.Open();

        try
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = $"PRAGMA table_info('{table}');";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string name = reader.GetString(1);
                if (string.Equals(name, column, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
        finally
        {
            if (!wasOpen)
                connection.Close();
        }
    }
}
