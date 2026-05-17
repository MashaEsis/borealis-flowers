using borealis_flowers.api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace borealis_flowers.api.Data;

/// <summary>dotnet-ef: поддерживается запуск из корня решения или из папки API.</summary>
public sealed class DataContextDesignTimeFactory : IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args)
    {
        string basePath = ResolveApiDirectory();

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        string raw = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection не найден в appsettings.json.");

        string resolved = SqliteConnectionResolver.Resolve(raw, basePath);

        DbContextOptionsBuilder<DataContext> optionsBuilder = new();
        optionsBuilder.UseSqlite(resolved);
        return new DataContext(optionsBuilder.Options);
    }

    static string ResolveApiDirectory()
    {
        string start = Directory.GetCurrentDirectory();

        DirectoryInfo current = new(start);

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "borealis-flowers.api.csproj")))
                return current.FullName;

            current = current.Parent;
        }

        return start;
    }
}
