using Microsoft.EntityFrameworkCore;
using Rihla.Infrastructure.Data;
using Rihla.Application.Interfaces;
using Rihla.Application.Services;
using Rihla.Core.Entities;
using Rihla.Core.Enums;
using Rihla.Core.ValueObjects;
using SchoolTransportationSystem.WebAPI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=rihla.db"));

builder.Services.AddScoped<IRouteService, RouteService>();
builder.Services.AddScoped<IDriverService, DriverService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ITripService, TripService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DatabaseSeeder.SeedAsync(context);
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseRouting();
app.MapControllers();

// Test endpoints
app.MapGet("/", () => "Rihla API is running!");

app.MapGet("/test-db", async (ApplicationDbContext context) =>
{
    try
    {
        await context.Database.EnsureCreatedAsync();
        return "Database created successfully!";
    }
    catch (Exception ex)
    {
        return $"Database error: {ex.Message}";
    }
});

app.Run();
