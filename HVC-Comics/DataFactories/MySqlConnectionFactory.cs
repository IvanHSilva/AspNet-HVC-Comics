using MySqlConnector;

namespace HVC_Comics.Data;

public class MySqlConnectionFactory(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

    public MySqlConnection CreateConnection()
    {
        var connectionString =
            _configuration.GetConnectionString("MySQL");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "A connection string 'MySQL' não foi configurada.");
        }

        return new MySqlConnection(connectionString);
    }
}
