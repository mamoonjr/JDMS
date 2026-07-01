using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace JDMS.Infrastructure.Data;

public static class MySqlConnectionHelper
{
    private const string AivenDefaultDatabase = "defaultdb";

    public static string Resolve(IConfiguration configuration)
    {
        var sslCa = configuration["MYSQL_SSL_CA"];

        var fromParts = TryBuildFromEnvironmentVariables(configuration);
        if (fromParts != null)
            return Prepare(fromParts, sslCa);

        var fromNested = TryBuildFromNestedConnectionString(configuration);
        if (fromNested != null)
            return Prepare(fromNested, sslCa);

        var fromConfig = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(fromConfig))
            return Prepare(fromConfig, sslCa);

        throw new InvalidOperationException(
            "MySQL is not configured. On Render set either:\n" +
            "  ConnectionStrings__DefaultConnection=Server=HOST;Port=PORT;Database=defaultdb;User=avnadmin;Password=PASS;SslMode=Required;\n" +
            "or separate variables: MYSQL_HOST, MYSQL_PORT, MYSQL_DATABASE, MYSQL_USER, MYSQL_PASSWORD");
    }

    public static string Prepare(string? raw, string? sslCaPath = null)
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new InvalidOperationException("MySQL connection string is empty.");

        var input = raw.Trim();

        if (input.StartsWith("mysql://", StringComparison.OrdinalIgnoreCase))
            input = ConvertMySqlUri(input);

        MySqlConnectionStringBuilder builder;
        try
        {
            builder = new MySqlConnectionStringBuilder(input);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Invalid MySQL connection string. Example:\n" +
                "Server=HOST.a.aivencloud.com;Port=12345;Database=defaultdb;User=avnadmin;Password=PASS;SslMode=Required;", ex);
        }

        ApplyAivenDefaults(builder);

        if (string.IsNullOrWhiteSpace(builder.Server))
            throw new InvalidOperationException("Connection string missing Server= (Aiven Host).");

        if (string.IsNullOrWhiteSpace(builder.Database))
            throw new InvalidOperationException(
                "Connection string missing Database=. On Aiven the name is usually defaultdb.\n" +
                "Add Database=defaultdb to ConnectionStrings__DefaultConnection on Render.");

        if (string.IsNullOrWhiteSpace(builder.UserID))
            throw new InvalidOperationException("Connection string missing User= (e.g. avnadmin).");

        if (string.IsNullOrWhiteSpace(builder.Password))
            throw new InvalidOperationException("Connection string missing Password=.");

        if (builder.Port == 0)
            builder.Port = 3306;

        if (!input.Contains("SslMode", StringComparison.OrdinalIgnoreCase))
            builder.SslMode = MySqlSslMode.Required;

        var ca = sslCaPath?.Trim();
        if (!string.IsNullOrEmpty(ca) && File.Exists(ca))
            builder.SslCa = ca;

        builder.ConnectionTimeout = 30;
        builder.DefaultCommandTimeout = 120;
        builder.MaximumPoolSize = 50;

        return builder.ConnectionString;
    }

    public static string DescribeSafe(string connectionString)
    {
        var b = new MySqlConnectionStringBuilder(connectionString);
        return $"Server={b.Server};Port={b.Port};Database={b.Database};User={b.UserID};SslMode={b.SslMode}";
    }

    private static string? TryBuildFromNestedConnectionString(IConfiguration configuration)
    {
        var section = configuration.GetSection("ConnectionStrings:DefaultConnection");
        if (!section.Exists())
            return null;

        var server = section["Server"] ?? section["Host"];
        if (string.IsNullOrWhiteSpace(server))
            return null;

        var builder = new MySqlConnectionStringBuilder
        {
            Server = server.Trim(),
            Port = uint.TryParse(section["Port"], out var p) ? p : 3306,
            Database = FirstNonEmpty(section["Database"], section["Db"]) ?? AivenDefaultDatabase,
            UserID = FirstNonEmpty(section["User"], section["UserID"], section["Uid"]) ?? "avnadmin",
            Password = section["Password"] ?? section["Pwd"] ?? "",
            SslMode = MySqlSslMode.Required
        };

        if (string.IsNullOrWhiteSpace(builder.Password))
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection:Password is required.");

        return builder.ConnectionString;
    }

    private static string? TryBuildFromEnvironmentVariables(IConfiguration configuration)
    {
        var host = FirstNonEmpty(
            configuration["MYSQL_HOST"],
            configuration["MYSQL_SERVER"],
            Environment.GetEnvironmentVariable("MYSQL_HOST"),
            Environment.GetEnvironmentVariable("MYSQL_SERVER"));

        if (string.IsNullOrWhiteSpace(host))
            return null;

        var port = FirstNonEmpty(
            configuration["MYSQL_PORT"],
            Environment.GetEnvironmentVariable("MYSQL_PORT")) ?? "3306";

        var database = FirstNonEmpty(
            configuration["MYSQL_DATABASE"],
            configuration["MYSQL_DB"],
            Environment.GetEnvironmentVariable("MYSQL_DATABASE"),
            Environment.GetEnvironmentVariable("MYSQL_DB")) ?? AivenDefaultDatabase;

        var user = FirstNonEmpty(
            configuration["MYSQL_USER"],
            configuration["MYSQL_USERNAME"],
            Environment.GetEnvironmentVariable("MYSQL_USER"),
            Environment.GetEnvironmentVariable("MYSQL_USERNAME")) ?? "avnadmin";

        var password = FirstNonEmpty(
            configuration["MYSQL_PASSWORD"],
            Environment.GetEnvironmentVariable("MYSQL_PASSWORD"));

        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("MYSQL_PASSWORD (or Password in connection string) is required.");

        var builder = new MySqlConnectionStringBuilder
        {
            Server = host.Trim(),
            Port = uint.TryParse(port, out var p) ? p : 3306,
            Database = database.Trim(),
            UserID = user.Trim(),
            Password = password,
            SslMode = MySqlSslMode.Required,
            ConnectionTimeout = 30,
            DefaultCommandTimeout = 120,
            MaximumPoolSize = 50
        };

        return builder.ConnectionString;
    }

    private static void ApplyAivenDefaults(MySqlConnectionStringBuilder builder)
    {
        if (!string.IsNullOrWhiteSpace(builder.Database))
            return;

        if (builder.Server.Contains("aivencloud.com", StringComparison.OrdinalIgnoreCase))
            builder.Database = AivenDefaultDatabase;
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }

        return null;
    }

    private static string ConvertMySqlUri(string uri)
    {
        var parsed = new Uri(uri);
        var userInfo = parsed.UserInfo.Split(':', 2);
        var user = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
        var database = parsed.AbsolutePath.TrimStart('/');
        if (string.IsNullOrEmpty(database))
            database = AivenDefaultDatabase;

        var port = parsed.Port > 0 ? parsed.Port : 3306;
        var ssl = uri.Contains("ssl-mode=REQUIRED", StringComparison.OrdinalIgnoreCase)
            ? "SslMode=Required;"
            : "SslMode=Preferred;";

        return $"Server={parsed.Host};Port={port};Database={database};User={user};Password={password};{ssl}";
    }
}
