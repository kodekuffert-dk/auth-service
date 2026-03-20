using auth_service.Data;
using auth_service.Data.Implementations.Postgres;
using auth_service.Middleware;
using auth_service.Services;
using auth_service.Services.Implementations;
using Npgsql;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Health checks
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString) || connectionString == "CHANGE_THIS_IN_PRODUCTION")
{
    connectionString = "Host=auth-db;Port=5432;Database=authdb;Username=authuser;Password=authpassword";
}
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "database", timeout: TimeSpan.FromSeconds(3));

// Dependency injection for services
builder.Services.AddSingleton<IAuthService, AuthService>();

// Email service: Use mock in Development, real service in Production
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<IEmailService, MockEmailService>();
}
else
{
    builder.Services.AddScoped<IEmailService, EmailService>();
}

// Dependency injection for repositories and database connection
builder.Services.AddScoped<IDbConnection>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection") ?? "Host=auth-db;Port=5432;Database=authdb;Username=authuser;Password=authpassword";
    return new NpgsqlConnection(connectionString);
});
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();

app.UseMiddleware<SignatureValidationMiddleware>();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
