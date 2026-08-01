using Microsoft.Data.SqlClient;

namespace HVC_Comics.Data;

public class SqlServerConnectionFactory(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

    public SqlConnection CreateConnection()
    {
        var connectionString =
            _configuration.GetConnectionString("SQLConn");

        return new SqlConnection(connectionString);
    }
}