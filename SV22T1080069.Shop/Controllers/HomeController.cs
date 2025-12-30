using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1080069.BusinessLayers;
using SV22T1080069.Shop.Models;
using System.Diagnostics;

namespace SV22T1080069.Shop.Controllers
{
   
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}
        /// <summary>
        /// Trang chủ - Hiển thị 4 sản phẩm đắt tiền nhất
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            var top4Products = await ProductDataService.ProductDB.TopProductsByPriceAsync(4);
            return View(top4Products);
        }

        public ActionResult About()
        {
            return View();
        }
        public ActionResult Portfolio()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
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
