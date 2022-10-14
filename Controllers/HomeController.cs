using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DoctorCeo.Models;
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
            string nameId = "";
            string name = "";
            string email = "-";
            string provider = "facebook";
            string utcDate = DateTime.UtcNow.ToLocalTime().ToString("dd/MM/yyyy HH-mm-ss");
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
                if (claim.Type.Contains("claims/nameidentifier"))
                {
                    nameId = claim.Value;
                }
                else if (claim.Type.Contains("claims/emailaddress"))
                {
                    email = claim.Value;
                }
                else if (claim.Type.Contains("claims/name"))
                {
                    name = claim.Value;
                }
            }
            var result = UpsertTableEntity(name, email, nameId, provider, utcDate);
            Console.WriteLine("provider: ", provider + nameId + " nome: " + name + " email: " + email+"date: ",utcDate);
            return View();
        }
        else
        {
            Console.WriteLine("não logado");
            return View();
        }
    }

    public async Task<string> UpsertTableEntity(string name, string email, string nameId, string provider, string utcDate)
    {
        string message = string.Empty;
        var ConnStr = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AzureStorageConfig")["ConnString"];
        TableServiceClient tableServiceClient = new TableServiceClient(ConnStr);
        TableClient tableClient = tableServiceClient.GetTableClient("sitevisitors");
        UserEntity sitevisitor = new UserEntity()
        {
            Name = name,
            Email = email,
            NameId = nameId,
            SigninProvider = provider,
            LastSigninDate = utcDate,            
        };
        Azure.Response response = await tableClient.UpsertEntityAsync(sitevisitor);
        // tableClient.UpdateEntity(sitevisitor, Azure.ETag.All, TableUpdateMode.Replace);
        // Azure.Response response = await tableClient.AddEntityAsync<UserEntity>(sitevisitor);
        message = response.Status.ToString();
        return message;
    }

    public ActionResult<DateTimeOffset?> GetLastAccessTime(TableClient table, string partitionKey, string rowKey)
    {
        //Please refer to https://docs.microsoft.com/en-us/rest/api/storageservices/querying-tables-and-entities for more details about query syntax.
        var queryResult = table.Query<UserEntity>(filter: $"PartitionKey eq '{partitionKey}' and RowKey eq '{rowKey}'").Single();

        DateTimeOffset? res = queryResult.Timestamp;
        return res;
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
