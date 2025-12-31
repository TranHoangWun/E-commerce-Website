using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SV22T1080069.BusinessLayers;
using SV22T1080069.DomainModels;
using SV22T1080069.Shop.Models;

namespace SV22T1080069.Shop.Controllers
{
    [Authorize(Roles = WebUserRoles.Customer)]
    public class OrderController : Controller
    {
        /// <summary>
        /// Dùng để chuyển đổi từ CartItemDb sang CartItem để hiển thị trên View 
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        private CartItem MapToCartItem(CartItemDb db)
        {
            return new CartItem
            {
                CartItemID = db.CartItemID,
                ProductID = db.ProductID,
                ProductName = db.ProductName,
                Photo = db.Photo,
                UnitPrice = db.UnitPrice,
                Quantity = db.Quantity
            };
        }

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
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var user = User.GetUserData();
            if (user == null)
                return RedirectToAction("Login", "Account");

            // Lấy khách
            var customer = await UserAccountService.CustomerUserAccountDB
                                                   .GetCustomerByEmailAsync(user.Email!);
            if (customer == null)
                return RedirectToAction("Login", "Account");

            // Lấy giỏ hàng từ DB theo customer.CustomerID
            var cartDbItems = await CartDataService.ListCartAsync(customer.CustomerID);
            var cartItems = cartDbItems.Select(MapToCartItem).ToList();


            if (!cartItems.Any())
            {
                TempData["Message"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "Cart");
            }

            var model = new CheckoutViewModel
            {
                CustomerID = customer.CustomerID,
                CustomerName = customer.CustomerName,
                ContactName = customer.ContactName,
                Email = customer.Email,    // trên view để readonly
                Phone = customer.Phone,
                Address = customer.Address,
                Province = customer.Province,
                CartItems = cartItems
            };

            // Nếu bạn dùng ViewBag.Provinces cho dropdown, set ở đây
            ViewBag.Provinces = await SelectListHelper.Provinces(customer.Province);



            return View(model);   // dùng view HTML checkout mà bạn đã có
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var user = User.GetUserData();
            if (user == null)
                return RedirectToAction("Login", "Account");

            var customer = await UserAccountService.CustomerUserAccountDB
                                                   .GetCustomerByEmailAsync(user.Email!);
            if (customer == null)
                return RedirectToAction("Login", "Account");

            var cartDbItems = (await CartDataService.ListCartAsync(customer.CustomerID)).ToList();
            if (!cartDbItems.Any())
            {
                TempData["Message"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "Cart");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Provinces = await SelectListHelper.Provinces(customer.Province);
                model.CartItems = cartDbItems.Select(MapToCartItem).ToList();
                return View(model);
            }

            var order = new Order
            {
                CustomerID = customer.CustomerID,
                DeliveryAddress = model.Address,
                DeliveryProvince = model.Province,
                EmployeeID = null,
                Status = Constants.ORDER_INIT
                // Không set CustomerName/Phone/Email vì không có cột tương ứng
                //PaymentMethod = model.PaymentMethod
            };

            var orderId = await OrderDataService.CreateOrderFromCartAsync(order, cartDbItems);

            await CartDataService.ClearCartAsync(customer.CustomerID);

            return RedirectToAction("Details", new { id = orderId });
        }


    }
}
