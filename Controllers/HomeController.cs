using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using DoctorCeo.Models;

namespace DoctorCeo.Controllers;
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }
    [HttpGet("~/")]
    public IActionResult Index()
    {
        if (User?.Identity?.IsAuthenticated ?? false)
        {
            string nameId = "";
            string nome = "";
            string email = "";
            HttpContext context = this.HttpContext;
            foreach (var claim in context.User.Claims)
            {
                if (claim.Type.Contains("name"))
                {
                    nome = claim.Value;
                }
                else if (claim.Type.Contains("emailaddress"))
                {
                    email = claim.Value;
                }
                else if (claim.Type.Contains("nameidentifier"))
                {
                    nameId = claim.Value;
                }
            }
        // http://<storage account>.table.core.windows.net/<table>
            Console.WriteLine(nameId+" nome: " + nome + " email: " + email);
            return View();
        }
        else
        {
            Console.WriteLine("não logado");
            return View();
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
