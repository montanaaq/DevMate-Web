using DevMate.Shared.Constants;
using Npgsql;

namespace DevMate.Configuration;

public sealed class AppSettings(string url, string databaseConnectionString)
{
    public string Url { get; } = url;
    public string DatabaseConnectionString { get; } = databaseConnectionString;

    public static AppSettings FromEnvironment()
    {
        string value = "http://localhost:8080/";

        if (
            !Uri.TryCreate(value, UriKind.Absolute, out var url)
            || url.Scheme != Uri.UriSchemeHttp
            || url.AbsolutePath != "/"
            || url.UserInfo.Length != 0
            || url.Query.Length != 0
            || url.Fragment.Length != 0
        )
        {
            throw new ArgumentException(ErrorMessages.InvalidUrl);
        }

        if (
            !int.TryParse(GetRequiredVariable("POSTGRES_PORT"), out int port)
            || port is < 1 or > 65535
        )
        {
            throw new ArgumentException(ErrorMessages.InvalidDatabasePort);
        }

        var connectionString = new NpgsqlConnectionStringBuilder
        {
            Host = GetRequiredVariable("POSTGRES_HOST"),
            Port = port,
            Database = GetRequiredVariable("POSTGRES_DB"),
            Username = GetRequiredVariable("POSTGRES_USER"),
            Password = GetRequiredVariable("POSTGRES_PASSWORD"),
        };

        return new AppSettings(url.AbsoluteUri, connectionString.ConnectionString);
    }

    private static string GetRequiredVariable(string name)
    {
        string? value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                string.Format(ErrorMessages.MissingEnvironmentVariable, name)
            );
        }

        return value;
    }
}
