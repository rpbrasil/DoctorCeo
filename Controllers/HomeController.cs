using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DoctorCeo.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Azure.Data.Tables;

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
            string nameId = "teste9999";
            string name = "testeNome testeSobreNome";
            string email = "teste@email.com";
            string provider = "facedIn";
            DateTime utcDate = DateTime.UtcNow;
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
            var result = InsertTableEntity(name, email, nameId, provider, utcDate);
            Console.WriteLine("provider: ", provider + nameId + " nome: " + name + " email: " + email+"date: ",utcDate);
            return View();
        }
        else
        {
            Console.WriteLine("não logado");
            return View();
        }
    }

    public async Task<string> InsertTableEntity(string name, string email, string nameId, string provider, DateTime utcDate)
    {
        string message = string.Empty;
        var ConnStr = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AzureStorageConfig")["ConnString"];
        // New instance of the TableClient class
        TableServiceClient tableServiceClient = new TableServiceClient(ConnStr);
        // New instance of TableClient class referencing the server-side table
        TableClient tableClient = tableServiceClient.GetTableClient("sitevisitors");
        UserEntity sitevisitor = new UserEntity()
        {
            Name = name,
            Email = email,
            NameId = nameId,
            SigninProvider = provider,
            LastSigninDate = utcDate
        };

        Azure.Response response = await tableClient.AddEntityAsync<UserEntity>(sitevisitor);
        message = response.Status.ToString();
        return message;
    }
    // Para mais metodos neste tema: https://github.com/Azure-Samples/msdocs-azure-data-tables-sdk-dotnet/blob/main/2-completed-app/AzureTablesDemoApplicaton/Services/TablesService.cs
    public void InsertTableEntity(UserEntity model)
    {
        TableEntity entity = new TableEntity();
        entity.PartitionKey = model.PartitionKey;
        entity.RowKey = $"{model.RowKey} {model.Timestamp}";

        // The other values are added like a items to a dictionary
        entity["Name"] = model.Name;
        entity["Email"] = model.Email;
        entity["NameId"] = model.NameId;
        entity["LastSigninDate"] = model.LastSigninDate;
        entity["SigninProvider"] = model.SigninProvider;
        entity["Etag"] = model.ETag;

        // _tableClient.AddEntity(entity);
    }


    public void UpsertTableEntity(UserEntity model)
    {
        TableEntity entity = new TableEntity();
        entity.PartitionKey = model.PartitionKey;
        entity.RowKey = $"{model.RowKey} {model.Timestamp}";

        // The other values are added like a items to a dictionary
        entity["Name"] = model.Name;
        entity["Email"] = model.Email;
        entity["NameId"] = model.NameId;
        entity["LastSigninDate"] = model.LastSigninDate;
        entity["SigninProvider"] = model.SigninProvider;
        entity["Etag"] = model.ETag;

         //_tableClient.UpsertEntity(entity);
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
