using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ExpenseManagementAPI.Repositories;
using ExpenseManagementAPI.Data;
using ExpenseManagementAPI.Middleware;
using ExpenseManagementAPI.Services;
using ExpenseManagementAPI.Helpers;
using ExpenseManagementAPI.Seeders;
using DotNetEnv;

// Load the correct .env based on environment

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    Env.Load(".env.development");
}
else
{
    Env.Load(".env.production");
}


// ========================================
// CONFIGURATIONs
// ========================================
var configuration = builder.Configuration;

// ========================================
// CORS CONFIGURATION
// ========================================
// Read CORS origins from environment variable
var corsOrigins = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS")?
    .Split(',', StringSplitOptions.RemoveEmptyEntries) ?? new[] { "*" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});


// ========================================
// JWT AUTHENTICATION CONFIGURATION
// ========================================
var jwtSettings = configuration.GetSection("JwtSettings");
var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
    ?? throw new InvalidOperationException("JWT_SECRET_KEY not set");
var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") 
    ?? throw new InvalidOperationException("JWT_ISSUER not set");
var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") 
    ?? throw new InvalidOperationException("JWT_AUDIENCE not set");

var keyBytes = Convert.FromBase64String(secretKey);
var securityKey = new SymmetricSecurityKey(keyBytes);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = securityKey,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// ========================================
// DEPENDENCY INJECTION - DATABASE HELPER
// ========================================
// ========================================
// DEPENDENCY INJECTION - DATABASE HELPER
// ========================================
var dbServer = Environment.GetEnvironmentVariable("DB_SERVER")
    ?? throw new InvalidOperationException("DB_SERVER not set");

var dbName = Environment.GetEnvironmentVariable("DB_NAME")
    ?? throw new InvalidOperationException("DB_NAME not set");

var dbUser = Environment.GetEnvironmentVariable("DB_USER")
    ?? throw new InvalidOperationException("DB_USER not set");

var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD")
    ?? throw new InvalidOperationException("DB_PASSWORD not set");

var connectionString =
    $"Server={dbServer};" +
    $"Database={dbName};" +
    $"User Id={dbUser};" +
    $"Password={dbPassword};" +
    $"Encrypt=True;" +
    $"TrustServerCertificate=True;" +
    $"MultipleActiveResultSets=True;" +
    $"Connection Timeout=30;";

builder.Services.AddSingleton<DatabaseHelper>(
    new DatabaseHelper(connectionString)
);

// ========================================
// DEPENDENCY INJECTION - REPOSITORIES
// ========================================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();
builder.Services.AddScoped<IContactMessageRepository, ContactMessageRepository>();
builder.Services.AddScoped<IUserSettingsRepository, UserSettingsRepository>();

// ========================================
// DEPENDENCY INJECTION - SERVICES
// ========================================
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddScoped<IPdfReportService, PdfReportService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<PasswordHasher>();
builder.Services.AddScoped<IContactMessageService, ContactMessageService>();
builder.Services.AddScoped<IUserSettingsService, UserSettingsService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<AdminSeeder>();

// ========================================
// CONTROLLERS & JSON OPTIONS
// ========================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// ========================================
// SWAGGER/OpenAPI
// ========================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Expense Management API",
        Version = "v1.0",
        Description = "RESTful API for Expense Management System with JWT Authentication"
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid JWT token."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ========================================
// BUILD APPLICATION
// ========================================
var app = builder.Build();

// ========================================
// MIDDLEWARE PIPELINE
// ========================================

// Custom Error Handling Middleware (MUST BE FIRST)
app.UseMiddleware<ErrorHandlingMiddleware>();

// Development-specific middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Expense Management API v1");
        c.RoutePrefix = string.Empty;
    });
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigins");

// ASP.NET Core Built-in Authentication (Primary authentication)
app.UseAuthentication();

// OPTIONAL: Custom Authentication Middleware (for logging/extra validation)
// Uncomment the line below if you want to use custom authentication middleware
// app.UseCustomAuthentication();

app.UseAuthorization();
app.MapControllers();

// --- Seed Admin User ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var seeder = services.GetRequiredService<AdminSeeder>();
    await seeder.SeedAsync();
}


// ========================================
// ROOT ENDPOINT
// ========================================

TimeZoneInfo indianZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");


app.MapGet("/", () => new
{
    Application = "Expense Management API",
    Version = "1.0.0",
    Status = "Running",
    Timestamp = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianZone),
    Documentation = "/swagger"
});

// ========================================
// HEALTH CHECK ENDPOINT
// ========================================
app.MapGet("/health", () => Results.Ok(new
{
    Status = "Healthy",
  Timestamp = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianZone)

}));

// Get the port from environment variable (Render assigns it)
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000"; 
app.Urls.Add($"http://0.0.0.0:{port}");
// ========================================
// RUN APPLICATION
// ========================================

app.Run();