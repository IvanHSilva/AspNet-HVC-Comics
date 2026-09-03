using HVC_Comics.Data;
using Microsoft.AspNetCore.Mvc;

namespace HVC_Comics.Controllers;

public class DBConnectionController(
    SqlServerConnectionFactory factory) : Controller
{
    private readonly SqlServerConnectionFactory _factory = factory;

    public IActionResult Database()
    {
        try
        {
            using var connection = _factory.CreateConnection();

            connection.Open();

            return Content(
                "Conexão com o SQL Server realizada com sucesso!");
        }
        catch (Exception ex)
        {
            return Content(
                $"Erro ao conectar ao SQL Server: {ex.Message}");
        }
    }
}
