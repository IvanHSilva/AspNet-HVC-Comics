using Npgsql;

namespace HVC_Comics.Data;

public class PostgreSqlConnectionFactory(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

    public NpgsqlConnection CreateConnection()
    {
        var connectionString =
            _configuration.GetConnectionString("PostgreSQL");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "A connection string 'PostgreSQL' não foi configurada.");
        }

        return new NpgsqlConnection(connectionString);
    }
}
