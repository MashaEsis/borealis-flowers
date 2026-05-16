using borealis_flowers.api.Data;
using borealis_flowers.api.Features.Customers;
using borealis_flowers.api.Features.HistoryTimeslots;
using borealis_flowers.api.Features.Images;
using borealis_flowers.api.Features.Requests;
using borealis_flowers.api.Features.Services;
using borealis_flowers.api.Features.Specialists;
using borealis_flowers.api.Features.Specializations;
using borealis_flowers.api.Features.Statistics;
using borealis_flowers.api.Features.Timeslots;
using borealis_flowers.api.Infrastructure;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
/* Database Context */
builder.Services.AddDbContext<DataContext>();

/* Services */
builder.Services
    .AddServices()
    .AddCache()
    .AddAzureConfiguration(builder.Configuration)
    .AddConfigureOption(builder.Configuration)
    .AddImageProcessing();

/* Admin */
// // builder.Services.AddCoreAdmin();

builder.Services.AddEndpointsApiExplorer();

/* Swagger */
builder.Services.AddSwaggerGen();

/* Add Antiforgery */
builder.Services.AddAntiforgery();

var app = builder.Build();

/* Protect Admin Middleware */
app.UseMiddleware<CoreAdminProtectionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseHttpsRedirection();

//TODO: remove local ip and figure how to fix CORS
app.UseCors(policy => policy
    .WithOrigins("http://localhost:5000", "https://localhost:5001", "https://localhost:44349")
    .AllowAnyMethod()
    .AllowAnyHeader());

/* Add Antiforgery middleware */
app.UseAntiforgery();

/* Endpoints */
app.SpecialistsEndpointsRegistration();
app.ServicesEndpointsRegistration();
app.SpecializationsEndpointsRegistration();
app.CustomersEndpointsRegistration();
// // app.SchedulesEndpointsRegistration();
// // app.WorkingDaysEndpointsRegistration();
app.TimeslotsEndpointsRegistration();
app.ImagesEndpointsRegistration();
app.StatisticsNewEndpointsRegistration();
app.TimeslotsHistoryEndpointsRegistration();
app.RequestsEndpointsRegistration();

app.MapDefaultControllerRoute();

/* Seed test data for statistics in Development */
if (app.Environment.IsDevelopment())
{
    // await app.SeedStatisticsTestDataAsync();
    // await app.SeedIvanVisitHistoryAsync(); //TODO: Implement the same for your purpose
    // await app.SeedDefredusVisitHistoryAsync();
}
app.MapControllers();
app.Run();

