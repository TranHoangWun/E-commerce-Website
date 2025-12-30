using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1080069.BusinessLayers;
using SV22T1080069.DomainModels;
using SV22T1080069.Shop.Models;

namespace SV22T1080069.Shop.Controllers
{
    [Authorize(Roles = WebUserRoles.Customer)]
    public class OrderController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var user = User.GetUserData();
            if (user == null)
                return RedirectToAction("Login", "Account");

            var customer = await UserAccountService.CustomerUserAccountDB
                                                   .GetCustomerByEmailAsync(user.Email!);
            if (customer == null)
                return RedirectToAction("Login", "Account");

            var order = await OrderDataService.OrderDB.GetAsync(id);
            if (order == null)
                return NotFound();

            if (order.CustomerID != customer.CustomerID)
                return Forbid();

            var items = (await OrderDataService.OrderDB.ListDetailsAsync(order.OrderID)).ToList();

            var model = new OrderProfileItemViewModel
            {
                Order = order,
                Items = items
            };

            return View(model);   // kiểu OrderProfileItemViewModel
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var user = User.GetUserData();
            if (user == null)
                return RedirectToAction("Login", "Account");

            var customer = await UserAccountService.CustomerUserAccountDB
                                                   .GetCustomerByEmailAsync(user.Email!);
            if (customer == null)
                return RedirectToAction("Login", "Account");

            var order = await OrderDataService.OrderDB.GetAsync(id);
            if (order == null)
                return NotFound();

            if (order.CustomerID != customer.CustomerID)
                return Forbid();

            // Chỉ cho khách hàng hủy nếu đang Mới hoặc Đã chấp nhận
            if (order.Status == Constants.ORDER_INIT ||
                order.Status == Constants.ORDER_ACCEPTED)
            {
                await OrderDataService.OrderDB.CancelAsync(order.OrderID);
                TempData["Message"] = "Đơn hàng đã được hủy.";
            }


            return RedirectToAction("Details", new { id });
        }
    }
   }
