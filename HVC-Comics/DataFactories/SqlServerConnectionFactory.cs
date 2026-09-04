using Microsoft.Data.SqlClient;

namespace HVC_Comics.Data;

public class SqlServerConnectionFactory(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

    public SqlConnection CreateConnection()
    {
        var connectionString =
            _configuration.GetConnectionString("SQLServer");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "A connection string 'SQLServer' não foi configurada.");
        }

        return new SqlConnection(connectionString);
    }
}
