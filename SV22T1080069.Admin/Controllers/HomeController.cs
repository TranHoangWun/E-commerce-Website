using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1080069.Admin.Models;
using SV22T1080069.DataLayers;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SV22T1080069.Admin.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Test()
        {
            string connectionString = "Server=LAPTOP-1AL3OBJH;Database=LiteCommerceDB;User Id=sa;Password=123;TrustServerCertificate=True;";
            var dal = new ProvinceDAL(connectionString);
            var data = await dal.ListAsync();
            return Json(data.ToList());
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
