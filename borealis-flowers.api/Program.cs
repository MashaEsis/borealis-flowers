using System.Text;
using borealis_flowers.api.Data;
using borealis_flowers.api.Data.Models;
using borealis_flowers.api.Features.AdminCatalog;
using borealis_flowers.api.Features.Auth;
using borealis_flowers.api.Features.Customers;
using borealis_flowers.api.Features.Directory;
using borealis_flowers.api.Features.FloristApplications;
using borealis_flowers.api.Features.Home;
using borealis_flowers.api.Features.HistoryTimeslots;
using borealis_flowers.api.Features.Images;
using borealis_flowers.api.Features.Orders;
using borealis_flowers.api.Features.PublicEvents;
using borealis_flowers.api.Features.Requests;
using borealis_flowers.api.Features.Services;
using borealis_flowers.api.Features.Specialists;
using borealis_flowers.api.Features.Specializations;
using borealis_flowers.api.Features.Statistics;
using borealis_flowers.api.Features.Timeslots;
using borealis_flowers.api.Features.Wallet;
using borealis_flowers.api.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string sqliteConnection = SqliteConnectionResolver.Resolve(
    builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
            "Задайте ConnectionStrings:DefaultConnection (см. appsettings.json)."),
    builder.Environment.ContentRootPath);

builder.Services.AddControllers();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
JwtOptions jwtOptions =
    builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Секция Jwt обязательна.");
if (string.IsNullOrEmpty(jwtOptions.Key) || jwtOptions.Key.Length < 32)
    throw new InvalidOperationException("Jwt:Key должен быть не короче 32 символов.");

builder.Services.AddSingleton<JwtTokenService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ClockSkew = TimeSpan.FromMinutes(2),
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddDbContext<DataContext>(options => options.UseSqlite(sqliteConnection));

builder.Services
    .AddServices()
    .AddCache()
    .AddAzureConfiguration(builder.Configuration)
    .AddConfigureOption(builder.Configuration)
    .AddImageProcessing();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Borealis Flowers API", Version = "v1" });
    c.AddSecurityDefinition(
        "bearer",
        new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT Authorization header: Bearer {token}",
        });
    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = [],
    });
});

builder.Services.AddAntiforgery();

WebApplication app = builder.Build();

EnsureSqliteDataDirectory(sqliteConnection);

using (IServiceScope scope = app.Services.CreateScope())
{
    DataContext db = scope.ServiceProvider.GetRequiredService<DataContext>();
    LegacySqliteMigrationBaseline.StampInitialMigrationIfLegacyDatabase(db);
    try
    {
        db.Database.ExecuteSqlRaw("""DELETE FROM "__EFMigrationsLock" WHERE "Id" = 1;""");
    }
    catch
    {
        // lock table may not exist yet
    }

    db.Database.Migrate();
    ServiceCatalogSchemaPatcher.Apply(db);
    PortfolioSchemaPatcher.Apply(db);
    BouquetOrderSchemaPatcher.Apply(db);
    WalletSchemaPatcher.Apply(db);
    await EnsureSeedAdminAsync(db);
    await DatabaseIntegrityPatcher.ApplyAsync(db);
    try
    {
        await BouquetCatalogSeeder.ApplyAsync(db, app.Environment.ContentRootPath);
    }
    catch (IOException ex)
    {
        app.Logger.LogWarning(ex, "Не удалось обновить фото каталога — файл занят другим процессом.");
    }
    await BouquetFloristSeeder.ApplyAsync(db);
}

app.UseMiddleware<CoreAdminProtectionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseCors(policy => policy
    .WithOrigins(
        "http://localhost:5000",
        "https://localhost:5001",
        "https://localhost:44349",
        "http://localhost:5298",
        "https://localhost:7027",
        "http://localhost:5185",
        "https://localhost:7094")
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.SpecialistsEndpointsRegistration();
app.ServicesEndpointsRegistration();
app.SpecializationsEndpointsRegistration();
app.CustomersEndpointsRegistration();
app.TimeslotsEndpointsRegistration();
app.ImagesEndpointsRegistration();
app.StatisticsNewEndpointsRegistration();
app.TimeslotsHistoryEndpointsRegistration();
app.RequestsEndpointsRegistration();
app.PublicEventsEndpointsRegistration();
app.HomeEndpointsRegistration();

app.AuthEndpointsRegistration();
app.FloristApplicationsEndpointsRegistration();
app.OrdersEndpointsRegistration();
app.WalletEndpointsRegistration();
app.StaffDirectoryEndpointsRegistration();
app.AdminCatalogEndpointsRegistration();

app.MapDefaultControllerRoute();

app.MapControllers();
await app.RunAsync();

static void EnsureSqliteDataDirectory(string resolvedSqliteConnectionString)
{
    var builderSql = new SqliteConnectionStringBuilder(resolvedSqliteConnectionString);
    string dataPath = builderSql.DataSource;
    string? directory = Path.GetDirectoryName(dataPath);
    if (!string.IsNullOrEmpty(directory))
        Directory.CreateDirectory(directory);
}

static async Task EnsureSeedAdminAsync(DataContext db)
{
    const string email = "admin@example.com";
    string normalized = email.ToLowerInvariant();
    const string password = "admin123";

    Customer? row =
        await db.Customers.FirstOrDefaultAsync(c =>
            c.Email != null && c.Email.ToLower() == normalized);

    string hash = BCrypt.Net.BCrypt.HashPassword(password);

    if (row is null)
    {
        await db.Customers.AddAsync(
            new Customer
            {
                Id = Guid.NewGuid(),
                Name = "Администратор",
                Email = normalized,
                PasswordHash = hash,
                IsAdmin = true,
                IsSpecialist = false,
                SpecialistId = null,
                FirstVisit = DateTime.UtcNow,
                LastVisit = DateTime.UtcNow,
            });
    }
    else
    {
        row.PasswordHash = hash;
        row.IsAdmin = true;
        row.IsSpecialist = false;
        row.SpecialistId = null;
        if (string.IsNullOrWhiteSpace(row.Name))
            row.Name = "Администратор";
    }

    await db.SaveChangesAsync();
}
