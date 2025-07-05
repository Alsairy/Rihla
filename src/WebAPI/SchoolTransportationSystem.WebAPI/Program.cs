using Microsoft.EntityFrameworkCore;
using Rihla.Infrastructure.Data;

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

var app = builder.Build();

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
