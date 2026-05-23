using borealis_flowers.api.Data;
using Microsoft.EntityFrameworkCore;

namespace borealis_flowers.api.Infrastructure;

public static class PortfolioSchemaPatcher
{
    public const string MigrationId = "20260521180000_AddSpecialistPortfolio";

    public static void Apply(DataContext db)
    {
        if (!ColumnExists(db, "Specialists", "StyleDescription"))
        {
            db.Database.ExecuteSqlRaw(
                """ALTER TABLE "Specialists" ADD COLUMN "StyleDescription" TEXT NULL;""");
        }

        if (!TableExists(db, "SpecialistPortfolioWorks"))
        {
            db.Database.ExecuteSqlRaw(
                """
                CREATE TABLE "SpecialistPortfolioWorks" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_SpecialistPortfolioWorks" PRIMARY KEY,
                    "SpecialistId" TEXT NOT NULL,
                    "ImageUrl" TEXT NOT NULL,
                    "Title" TEXT NULL,
                    "SortOrder" INTEGER NOT NULL,
                    "CreatedAt" TEXT NOT NULL,
                    CONSTRAINT "FK_SpecialistPortfolioWorks_Specialists_SpecialistId"
                        FOREIGN KEY ("SpecialistId") REFERENCES "Specialists" ("Id") ON DELETE CASCADE
                );
                """);

            db.Database.ExecuteSqlRaw(
                """CREATE INDEX "IX_SpecialistPortfolioWorks_SpecialistId" ON "SpecialistPortfolioWorks" ("SpecialistId");""");
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
                if (string.Equals(reader.GetString(1), column, StringComparison.OrdinalIgnoreCase))
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

    static bool TableExists(DataContext db, string table)
    {
        var connection = db.Database.GetDbConnection();
        bool wasOpen = connection.State == System.Data.ConnectionState.Open;
        if (!wasOpen)
            connection.Open();

        try
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=$name;";
            var param = cmd.CreateParameter();
            param.ParameterName = "$name";
            param.Value = table;
            cmd.Parameters.Add(param);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }
        finally
        {
            if (!wasOpen)
                connection.Close();
        }
    }
}
