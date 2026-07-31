using HVC_Comics.Data;
using Microsoft.AspNetCore.Mvc;

namespace HVC_Comics.Controllers;

public class DBConnectionController : Controller
{
    private readonly SqlServerConnection _connection;

    public DBConnectionController(SqlServerConnection connection)
    {
        _connection = connection;
    }

    public IActionResult Database()
    {
        try
        {
            using var conn = _connection.CreateConnection();

            conn.Open();

            return Content("Conexão com o banco de dados realizada com sucesso!");
        }
        catch (Exception ex)
        {
            return Content(ex.Message);
        }
    }
}
