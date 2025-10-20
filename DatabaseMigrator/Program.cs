using DbUp;

var conn = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? "Host=auth-db;Port=5432;Database=authdb;Username=authuser;Password=authpassword";

// Nulstil kun i Development ved behov (kommentér ind/ud):
var resetInDev = Environment.GetEnvironmentVariable("RESET_SCHEMA") == "true";
if (resetInDev)
{
    using var npg = new Npgsql.NpgsqlConnection(conn);
    await npg.OpenAsync();
    var resetSql = """
        DROP SCHEMA IF EXISTS public CASCADE;
        CREATE SCHEMA public;
        GRANT ALL ON SCHEMA public TO public;
    """;
    using var cmd = new Npgsql.NpgsqlCommand(resetSql, npg);
    await cmd.ExecuteNonQueryAsync();
}

var upgrader = DeployChanges.To
    .PostgresqlDatabase(conn)
    .WithScriptsFromFileSystem("/app/sql") // mappes til ./db/migrations
    .LogToConsole()
    .Build();

var result = upgrader.PerformUpgrade();
if (!result.Successful)
{
    Console.Error.WriteLine(result.Error);
    Environment.ExitCode = -1;
}
else
{
    Console.WriteLine("Migrations completed.");
}