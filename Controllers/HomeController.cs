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
            string name = "";
            string email = "";
            string provider = "";

            var referer = HttpContext.Request.Headers.Referer;
            switch (referer)
            {
                case string l when l.Contains("linkedin"):
                    provider = "LinkedIn";
                    break;
                case string g when g.Contains("google"):
                    provider = "Google";
                    break;
                case string f when f.Contains("facebook"):
                    provider = "Facebook";
                    break;
                case string a when a.Contains("amazon"):
                    provider = "Amazon";
                    break;
                case string t when t.Contains("twitter"):
                    provider = "Twitter";
                    break;
            }
            HttpContext context = this.HttpContext;
            foreach (var claim in context.User.Claims)
            {
                if (claim.Type.Contains("name"))
                {
                    name = claim.Value;
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
            Console.WriteLine(provider + nameId + " nome: " + name + " email: " + email);
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
