using borealis_flowers.api.Data;
using Microsoft.EntityFrameworkCore;

namespace borealis_flowers.api.Infrastructure;

/// <summary>
/// Кошелёк клиента, привязанные карты и оплата букетов.
/// </summary>
public static class WalletSchemaPatcher
{
    public const string MigrationId = "20260523120000_WalletAndLoyalty";

    public static void Apply(DataContext db)
    {
        if (!ColumnExists(db, "Customers", "WalletBalance"))
        {
            db.Database.ExecuteSqlRaw(
                """ALTER TABLE "Customers" ADD COLUMN "WalletBalance" REAL NOT NULL DEFAULT 0;""");
        }

        foreach ((string column, string sqlType) in new[]
                 {
                     ("ChargedAmount", "REAL NULL"),
                     ("DiscountPercent", "INTEGER NOT NULL DEFAULT 0"),
                     ("IsPaid", "INTEGER NOT NULL DEFAULT 0"),
                     ("PaidAtUtc", "TEXT NULL"),
                 })
        {
            if (!ColumnExists(db, "Requests", column))
                db.Database.ExecuteSqlRaw($"""ALTER TABLE "Requests" ADD COLUMN "{column}" {sqlType};""");
        }

        if (!TableExists(db, "PaymentCards"))
        {
            db.Database.ExecuteSqlRaw(
                """
                CREATE TABLE "PaymentCards" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_PaymentCards" PRIMARY KEY,
                    "CustomerId" TEXT NOT NULL,
                    "Label" TEXT NOT NULL,
                    "LastFour" TEXT NOT NULL,
                    "IsDefault" INTEGER NOT NULL,
                    "CreatedAt" TEXT NOT NULL,
                    CONSTRAINT "FK_PaymentCards_Customers_CustomerId"
                        FOREIGN KEY ("CustomerId") REFERENCES "Customers" ("Id") ON DELETE CASCADE
                );
                CREATE INDEX "IX_PaymentCards_CustomerId" ON "PaymentCards" ("CustomerId");
                """);
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

    static bool TableExists(DataContext db, string table)
    {
        var connection = db.Database.GetDbConnection();
        bool wasOpen = connection.State == System.Data.ConnectionState.Open;
        if (!wasOpen)
            connection.Open();

        try
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText =
                $"SELECT 1 FROM sqlite_master WHERE type='table' AND name='{table}' LIMIT 1;";
            return cmd.ExecuteScalar() is not null;
        }
        finally
        {
            if (!wasOpen)
                connection.Close();
        }
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
