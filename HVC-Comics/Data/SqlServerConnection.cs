using Microsoft.Data.SqlClient;

namespace HVC_Comics.Data;

public class SqlServerConnection(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

    public SqlConnection CreateConnection()
    {
        return new SqlConnection(
            _configuration.GetConnectionString("SQLConn"));
    }
}
