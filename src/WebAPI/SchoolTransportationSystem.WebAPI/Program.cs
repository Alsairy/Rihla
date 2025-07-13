using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Globalization;
using System.Text;
using System.Reflection;
using SchoolTransportationSystem.Infrastructure.Data;
using SchoolTransportationSystem.Application.Interfaces;
using SchoolTransportationSystem.Application.Services;
using SchoolTransportationSystem.Core.Entities;
using SchoolTransportationSystem.Core.Enums;
using SchoolTransportationSystem.Core.ValueObjects;
using SchoolTransportationSystem.WebAPI.Hubs;
using SchoolTransportationSystem.WebAPI.Services;
using SchoolTransportationSystem.WebAPI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en"),
        new CultureInfo("ar")
    };

    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    options.RequestCultureProviders.Insert(0, new QueryStringRequestCultureProvider());
    options.RequestCultureProviders.Insert(1, new CookieRequestCultureProvider());
    options.RequestCultureProviders.Insert(2, new AcceptLanguageHeaderRequestCultureProvider());
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// Configure Swagger/OpenAPI with JWT Authentication
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Rihla API",
        Version = "v1",
        Description = "A comprehensive API for Rihla - School Transportation Management System",
        Contact = new OpenApiContact
        {
            Name = "Rihla Support",
            Email = "support@rihla.sa",
            Url = new Uri("https://rihla.sa")
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});
builder.Services.AddSignalR();


// Configure authentication with multiple schemes
var authBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "Rihla",
        ValidAudience = "RihlaUsers",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? "your-super-secret-key-that-is-at-least-32-characters-long"))
    };
});

var azureAdConfig = builder.Configuration.GetSection("ActiveDirectory:AzureAd");
var azureAdEnabled = azureAdConfig.GetValue<bool>("Enabled");

/*
if (azureAdEnabled && !string.IsNullOrEmpty(azureAdConfig["ClientId"]))
{
    authBuilder.AddOpenIdConnect("AzureAD", options =>
    {
        options.Authority = azureAdConfig["Authority"];
        options.ClientId = azureAdConfig["ClientId"];
        options.ClientSecret = azureAdConfig["ClientSecret"];
        options.ResponseType = "code";
        options.CallbackPath = azureAdConfig["CallbackPath"];
        options.SignedOutCallbackPath = azureAdConfig["SignedOutCallbackPath"];
        
        options.Scope.Clear();
        var scopes = azureAdConfig.GetSection("Scopes").Get<string[]>() ?? new[] { "openid", "profile", "email" };
        foreach (var scope in scopes)
        {
            options.Scope.Add(scope);
        }
        
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        
        options.Events = new OpenIdConnectEvents
        {
            OnAuthenticationFailed = context =>
            {
                context.Response.Redirect("/error");
                context.HandleResponse();
                return Task.CompletedTask;
            },
            OnTokenValidated = async context =>
            {
                var activeDirectoryService = context.HttpContext.RequestServices.GetRequiredService<IActiveDirectoryService>();
                var username = context.Principal?.Identity?.Name;
                
                if (!string.IsNullOrEmpty(username))
                {
                    await activeDirectoryService.SyncUserAsync(username);
                }
            }
        };
    });
}
*/

// Configure Authorization with Role-Based and Permission-Based Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("SystemAdmin", policy => policy.RequireRole("SuperAdmin", "TenantAdmin", "SystemAdmin"));
    options.AddPolicy("ManagerOrAbove", policy => policy.RequireRole("Admin", "Manager"));
    options.AddPolicy("DispatcherOrAbove", policy => policy.RequireRole("Admin", "Manager", "Dispatcher"));
    options.AddPolicy("DriverAccess", policy => policy.RequireRole("Admin", "Manager", "Dispatcher", "Driver"));
    options.AddPolicy("ParentAccess", policy => policy.RequireRole("Admin", "Manager", "Parent"));

    options.AddPolicy("Permission:ManageUsers", policy => 
        policy.Requirements.Add(new SchoolTransportationSystem.WebAPI.Attributes.PermissionRequirement("ManageUsers")));
    options.AddPolicy("Permission:ViewUsers", policy => 
        policy.Requirements.Add(new SchoolTransportationSystem.WebAPI.Attributes.PermissionRequirement("ViewUsers")));
    options.AddPolicy("Permission:ManageDrivers", policy => 
        policy.Requirements.Add(new SchoolTransportationSystem.WebAPI.Attributes.PermissionRequirement("ManageDrivers")));
    options.AddPolicy("Permission:ViewDrivers", policy => 
        policy.Requirements.Add(new SchoolTransportationSystem.WebAPI.Attributes.PermissionRequirement("ViewDrivers")));
    options.AddPolicy("Permission:ManageVehicles", policy => 
        policy.Requirements.Add(new SchoolTransportationSystem.WebAPI.Attributes.PermissionRequirement("ManageVehicles")));
    options.AddPolicy("Permission:ViewVehicles", policy => 
        policy.Requirements.Add(new SchoolTransportationSystem.WebAPI.Attributes.PermissionRequirement("ViewVehicles")));
    options.AddPolicy("Permission:ManageRoutes", policy => 
        policy.Requirements.Add(new SchoolTransportationSystem.WebAPI.Attributes.PermissionRequirement("ManageRoutes")));
    options.AddPolicy("Permission:ViewRoutes", policy => 
        policy.Requirements.Add(new SchoolTransportationSystem.WebAPI.Attributes.PermissionRequirement("ViewRoutes")));
    options.AddPolicy("Permission:ManageTrips", policy => 
        policy.Requirements.Add(new SchoolTransportationSystem.WebAPI.Attributes.PermissionRequirement("ManageTrips")));
    options.AddPolicy("Permission:ViewTrips", policy => 
        policy.Requirements.Add(new SchoolTransportationSystem.WebAPI.Attributes.PermissionRequirement("ViewTrips")));
    options.AddPolicy("Permission:ManageAttendance", policy => 
        policy.Requirements.Add(new SchoolTransportationSystem.WebAPI.Attributes.PermissionRequirement("ManageAttendance")));
    options.AddPolicy("Permission:ViewAttendance", policy => 
        policy.Requirements.Add(new SchoolTransportationSystem.WebAPI.Attributes.PermissionRequirement("ViewAttendance")));
    options.AddPolicy("Permission:ManagePayments", policy => 
        policy.Requirements.Add(new SchoolTransportationSystem.WebAPI.Attributes.PermissionRequirement("ManagePayments")));
    options.AddPolicy("Permission:ViewPayments", policy => 
        policy.Requirements.Add(new SchoolTransportationSystem.WebAPI.Attributes.PermissionRequirement("ViewPayments")));
    options.AddPolicy("Permission:ManageMaintenance", policy => 
        policy.Requirements.Add(new SchoolTransportationSystem.WebAPI.Attributes.PermissionRequirement("ManageMaintenance")));
    options.AddPolicy("Permission:ViewMaintenance", policy => 
        policy.Requirements.Add(new SchoolTransportationSystem.WebAPI.Attributes.PermissionRequirement("ViewMaintenance")));
    options.AddPolicy("Permission:ViewReports", policy => 
        policy.Requirements.Add(new SchoolTransportationSystem.WebAPI.Attributes.PermissionRequirement("ViewReports")));
    options.AddPolicy("Permission:ViewStudents", policy => 
        policy.Requirements.Add(new SchoolTransportationSystem.WebAPI.Attributes.PermissionRequirement("ViewStudents")));
    options.AddPolicy("Permission:ManageStudents", policy => 
        policy.Requirements.Add(new SchoolTransportationSystem.WebAPI.Attributes.PermissionRequirement("ManageStudents")));
});

builder.Services.AddScoped<IAuthorizationHandler, SchoolTransportationSystem.WebAPI.Attributes.PermissionRequirementHandler>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", policy =>
    {
        policy.WithOrigins("https://code-review-app-8uuyzbq4.devinapps.com", "http://localhost:5173", "http://localhost:8081", "http://localhost:3000", "https://code-review-audit-tunnel-avtdc1ra.devinapps.com", "https://code-review-app-tunnel-1brka0c7.devinapps.com", "https://code-review-app-tunnel-s90s9jto.devinapps.com", "https://code-review-app-tunnel-ye3vmhv2.devinapps.com", "https://code-review-app-tunnel-fl4m1cdf.devinapps.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });

    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins(
                "https://rihla.sa",
                "https://www.rihla.sa",
                "https://app.rihla.sa",
                "https://admin.rihla.sa"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Configure HTTPS redirection
builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
    options.HttpsPort = 443;
});

builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

// Configure Entity Framework with environment-aware database provider
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (builder.Environment.IsDevelopment())
    {
        options.UseSqlite(connectionString);
    }
    else
    {
        options.UseSqlServer(connectionString);
    }
});


builder.Services.AddScoped<IRouteService, RouteService>();
builder.Services.AddScoped<IDriverService, DriverService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ITripService, TripService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserContext, UserContext>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<SchoolTransportationSystem.Application.Interfaces.IAuditLogService, SchoolTransportationSystem.Application.Services.AuditLogService>();
builder.Services.AddScoped<SchoolTransportationSystem.Application.Services.MfaService>();
builder.Services.AddScoped<SchoolTransportationSystem.Application.Services.PasswordPolicyService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();


// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Rihla API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
        c.DisplayRequestDuration();
    });
    app.UseCors("Development");
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts(); // Add HSTS header in production
    app.UseCors("Production");
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add("Permissions-Policy", "geolocation=(self), microphone=(), camera=()");
    
    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers.Add("Content-Security-Policy", 
            "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; connect-src 'self' https:; font-src 'self'; object-src 'none'; media-src 'self'; frame-src 'none';");
    }
    
    await next();
});

app.UseRouting();

app.UseRequestLocalization();

app.UseAuthentication();
app.UseMiddleware<SchoolTransportationSystem.WebAPI.Middleware.PermissionAuthorizationMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");

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

app.MapPost("/seed-db", async (ApplicationDbContext context) =>
{
    try
    {
        await DatabaseSeeder.SeedAsync(context);
        return "Database seeded successfully!";
    }
    catch (Exception ex)
    {
        return $"Seeding error: {ex.Message}";
    }
});

app.Run();

public partial class Program { }
