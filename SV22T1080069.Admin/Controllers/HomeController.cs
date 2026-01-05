using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1080069.Admin.Models;
using SV22T1080069.BusinessLayers;
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

        //public IActionResult Index()
        //{
        //    return View();
        //}
        /// <summary>
        /// Dashboard cho trang quản trị 
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;

            var dal = new CustomerDAL(Configuration.ConnectionString);
            var totalCustomers = await dal.CountAllAsync();
            var totalOrders = await OrderDataService.OrderDB.CountAllAsync();
            var revenueThisMonth = await OrderDataService.OrderDB.GetRevenueInMonthAsync(today.Year, today.Month);
            var revenueToday = await OrderDataService.OrderDB.GetRevenueInDayAsync(today);
            var pendingOrders = await OrderDataService.OrderDB.CountPendingAsync();

            var chartRaw = await OrderDataService.OrderDB.GetRevenueChartAsync(
                fromDate: today.AddDays(-6),
                toDate: today
            );

            var model = new DashboardViewModel
            {
                TotalCustomers = totalCustomers,
                TotalOrders = totalOrders,
                RevenueThisMonth = revenueThisMonth,
                RevenueToday = revenueToday,
                PendingOrders = pendingOrders,
                RevenueChart = chartRaw
                    .Select(x => new RevenuePoint { Date = x.Date, Amount = x.Amount })
                    .ToList()
            };

            return View(model);
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
