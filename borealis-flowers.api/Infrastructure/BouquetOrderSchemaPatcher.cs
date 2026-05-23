using borealis_flowers.api.Data;
using Microsoft.EntityFrameworkCore;

namespace borealis_flowers.api.Infrastructure;

/// <summary>
/// Привязка букетов к флористам и поля доставки в заказах.
/// </summary>
public static class BouquetOrderSchemaPatcher
{
    public const string MigrationId = "20260520120000_BouquetFloristAndDelivery";

    public static void Apply(DataContext db)
    {
        if (!ColumnExists(db, "Services", "SpecialistId"))
        {
            db.Database.ExecuteSqlRaw(
                """ALTER TABLE "Services" ADD COLUMN "SpecialistId" TEXT NULL;""");
        }

        if (!ColumnExists(db, "Requests", "CardMessage"))
        {
            db.Database.ExecuteSqlRaw(
                """ALTER TABLE "Requests" ADD COLUMN "CardMessage" TEXT NULL;""");
        }

        if (!ColumnExists(db, "Requests", "DeliveryAddress"))
        {
            db.Database.ExecuteSqlRaw(
                """ALTER TABLE "Requests" ADD COLUMN "DeliveryAddress" TEXT NULL;""");
        }

        if (!ColumnExists(db, "Requests", "DeliveryLatitude"))
        {
            db.Database.ExecuteSqlRaw(
                """ALTER TABLE "Requests" ADD COLUMN "DeliveryLatitude" REAL NULL;""");
        }

        if (!ColumnExists(db, "Requests", "DeliveryLongitude"))
        {
            db.Database.ExecuteSqlRaw(
                """ALTER TABLE "Requests" ADD COLUMN "DeliveryLongitude" REAL NULL;""");
        }

        if (!ColumnExists(db, "Requests", "CustomerPhoneSnapshot"))
        {
            db.Database.ExecuteSqlRaw(
                """ALTER TABLE "Requests" ADD COLUMN "CustomerPhoneSnapshot" TEXT NULL;""");
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
