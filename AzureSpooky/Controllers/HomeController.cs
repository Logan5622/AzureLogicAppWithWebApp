using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AzureSpooky.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;

namespace AzureSpooky.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        static readonly HttpClient HttpClient = new HttpClient();
        private readonly BlobServiceClient _blobClient;

        public HomeController(ILogger<HomeController> logger, BlobServiceClient blobClient)
        {
            _logger = logger;
            _blobClient = blobClient;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task< IActionResult> Index(SpookyRequest spookyRequest, IFormFile file)
        {
            spookyRequest.Id = Guid.NewGuid().ToString();
            var jsonContent = JsonConvert.SerializeObject(spookyRequest);
            using(var content = new StringContent(jsonContent, Encoding.UTF8,"application/json"))
            {
                HttpResponseMessage httpResponseMessage = await HttpClient.
                    PostAsync("https://prod-26.southindia.logic.azure.com:443/workflows/c1c0c467531a4cfdbfc2315c7a719323/triggers/manual/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=TbIr34RrxbnbNaSduTyd6LI5d4KbZ0xa6eNALVu2e0w",
                    content);

            }

            if(file != null)
            {
                var fileName = spookyRequest.Id + Path.GetExtension(file.FileName);
                BlobContainerClient blobContainerClient = _blobClient.GetBlobContainerClient("logicappfiles");
                var blobClient = blobContainerClient.GetBlobClient(fileName);

                var httpHeaders = new BlobHttpHeaders()
                {
                    ContentType = file.ContentType
                };

                await blobClient.UploadAsync(file.OpenReadStream(), httpHeaders);
            }
            return RedirectToAction(nameof(Index));
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
}