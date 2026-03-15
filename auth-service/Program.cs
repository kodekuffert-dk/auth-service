using auth_service.Data;
using auth_service.Data.Implementations.Postgres;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Health checks
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString) || connectionString == "CHANGE_THIS_IN_PRODUCTION")
{
    connectionString = "Host=auth-db;Port=5432;Database=authdb;Username=authuser;Password=authpassword";
}
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "database", timeout: TimeSpan.FromSeconds(3));

// JWT authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "this_wont_work";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "auth_service";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "auth_service_users";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

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
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = key
    };
    // Tilføj logging for authentication events
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"[JWT] Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine($"[JWT] Token validated for: {context.Principal?.Identity?.Name}");
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine($"[JWT] Challenge triggered: {context.ErrorDescription}");
            return Task.CompletedTask;
        }
    };
});

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administrator", policy => policy.RequireRole("Administrator"));
    options.AddPolicy("Student", policy => policy.RequireRole("Student"));
});

// Dependency injection for AuthService
builder.Services.AddSingleton<auth_service.Services.AuthService>();

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

// Initialiser database
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<IDbConnection>();
//    DatabaseInitializer.Initialize(db);
//}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
