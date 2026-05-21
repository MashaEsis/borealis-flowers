using Microsoft.Data.Sqlite;

namespace borealis_flowers.api.Infrastructure;

/// <summary>
/// Приводит строку SQLite к абсолютному пути файла относительно корня приложения (ContentRoot).
/// Иначе относительный Data Source зависит от текущей рабочей директории и легко получить «другую» БД или ошибки при миграциях.
/// </summary>
public static class SqliteConnectionResolver
{
    public static string Resolve(string connectionString, string contentRootPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentRootPath);

        var builder = new SqliteConnectionStringBuilder(connectionString);
        if (string.IsNullOrWhiteSpace(builder.DataSource))
            throw new InvalidOperationException("Строка подключения SQLite не содержит Data Source.");

        if (!Path.IsPathRooted(builder.DataSource))
            builder.DataSource = Path.GetFullPath(Path.Combine(contentRootPath, builder.DataSource));

        return builder.ConnectionString;
    }
}
