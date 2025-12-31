using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Client;
using SV22T1080069.Admin.Models;
using SV22T1080069.BusinessLayers;
using SV22T1080069.DomainModels;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SV22T1080069.Admin.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}
        private const int PAGESIZE_ORDER = 20;
        private const string ORDER_SEARCH_CONDITION = "OrderSearchCodition";
        private int? GetCurrentEmployeeId()
        {
            var userData = User.GetUserData();
            if (userData == null || string.IsNullOrEmpty(userData.UserId))
                return null;

            return Convert.ToInt32(userData.UserId);
        }

        public IActionResult Index()
        {
            var condition = ApplicationContext.GetSessionData<OrderSearchCondition>(ORDER_SEARCH_CONDITION);
            if (condition == null)
            {
                condition = new OrderSearchCondition()
                {
                    Page = 1,
                    PageSize = PAGESIZE_ORDER,
                    SearchValue = "",
                    StatusID = 0,
                    FromDate = null,
                    ToDate = null,
                    DateRange = "" // để trống
                };
            }
            condition ??= new OrderSearchCondition()
            {
                Page = 1,
                PageSize = PAGESIZE_ORDER,
                SearchValue = "",
                StatusID = 0,
                DateRange = string.Format(
                    "{0:dd/MM/yyyy} - {1:dd/MM/yyyy}",
                    DateTime.Today.AddDays(-365),
                    DateTime.Today
                )
            };
            return View(condition);
        }

        public async Task<IActionResult> SearchOrders(OrderSearchCondition condition)
        {
            if (!string.IsNullOrEmpty(condition.DateRange))
            {
                var parts = condition.DateRange.Split(" - ");
                condition.FromDate = DateTime.ParseExact(parts[0], "dd/MM/yyyy", null);
                condition.ToDate = DateTime.ParseExact(parts[1], "dd/MM/yyyy", null);
            }
            else
            {
                condition.FromDate = null;
                condition.ToDate = null;
            }
            var data = await OrderDataService.OrderDB.ListAsync(
                condition.Page,
                condition.PageSize,
                condition.StatusID,
                condition.FromDate,
                condition.ToDate,
                condition.SearchValue
            );

            var rowCount = await OrderDataService.OrderDB.CountAsync(
                condition.StatusID,
                condition.FromDate,
                condition.ToDate,
                condition.SearchValue
            );

            var model = new PaginationSearchResult<Order>()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue,
                RowCount = rowCount,
                Data = data,
            };

            ApplicationContext.SetSessionData(ORDER_SEARCH_CONDITION, condition);

            return View(model);
        }

        private const string PRODUCT_SEARCH_FOR_SALE = "ProductSearchForSale";
        public const int PAGE_SIZE = 5;

        public IActionResult Create()
        {
            var condition = ApplicationContext.GetSessionData<ProductSearchCondition>(PRODUCT_SEARCH_FOR_SALE);
            if (condition == null)
            {
                condition = new ProductSearchCondition()
                {
                    Page = 1,
                    PageSize = PAGE_SIZE,
                    //SearchValue = string.Empty,
                    SearchValue = "",
                    CategoryID = 0,
                    SupplierID = 0,
                    ProductID = 0,
                    MinPrice = 0,
                    MaxPrice = 0
                };
                //ApplicationContext.SetSessionData(PRODUCT_SEARCH_FOR_SALE, condition);
            }
            return View(condition);
        }
        
        /// <summary>
        /// Tìm mặt hàng để đưa vào giỏ
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> SearchProducts(ProductSearchCondition condition)
        {
            if (condition == null)
            {
                return Content("Yêu cầu không hợp lệ!");
            }
            //var model = new ProductSearchResult()
            //{
            //    Page = condition.Page,
            //    PageSize = condition.PageSize,
            //    SearchValue = condition.SearchValue,
            //    CategoryID = condition.CategoryID,
            //    SupplierID = condition.SupplierID,
            //    ProductID = condition.ProductID,
            //    MinPrice = condition.MinPrice,
            //    MaxPrice = condition.MaxPrice,
            //    Data = await ProductDataService.ProductDB.ListAsync(condition.Page,
            //        condition.PageSize,
            //        condition.SearchValue,
            //        condition.CategoryID,
            //        condition.SupplierID,
            //        condition.ProductID,
            //        condition.MinPrice,
            //        condition.MaxPrice
            //    ),
            //    RowCount = await ProductDataService.ProductDB.CountAsync(
            //        condition.SearchValue,
            //        condition.CategoryID,
            //        condition.SupplierID,
            //        condition.ProductID,
            //        condition.MinPrice,
            //        condition.MaxPrice
            //    )
            //};
            var data = await ProductDataService.ProductDB.ListAsync(
                page: condition.Page,
                pageSize: condition.PageSize,
                searchValue: condition.SearchValue,
                categoryID: condition.CategoryID,
                supplierID: condition.SupplierID,
                minPrice: condition.MinPrice,
                maxPrice: condition.MaxPrice
            );

            var rowCount = await ProductDataService.ProductDB.CountAsync(
                searchValue: condition.SearchValue,
                categoryID: condition.CategoryID,
                supplierID: condition.SupplierID,
                minPrice: condition.MinPrice,
                maxPrice: condition.MaxPrice
            );

            var model = new PaginationSearchResult<Product>()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue,
                RowCount = rowCount,
                Data = data
            };
            ApplicationContext.SetSessionData(PRODUCT_SEARCH_FOR_SALE, condition);
            return View(model);
        }
        /// <summary>
        /// Lấy giỏ hàng 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult GetCart()
        {
            return View(GetSessionCart());
        }
        public IActionResult AddToCart(OrderDetail data)
        {
            if (data.Quantity <1)
                return Json(new ApiResult()
                {
                    Code = 0,
                    Message = "Số lượng phải lớn hơn hoặc bằng 1"
                });
            if (data.SalePrice <0)
                return Json(new ApiResult()
                {
                    Code = 0,
                    Message = "Giá bán phải lớn hơn hoặc bằng 0"
                });
            AddSesssionCart(data);
            return Json(new ApiResult()
            {
                Code = 1,
                Message = "Thêm mặt hàng vào giỏ thành công"
            });
            //AddSesssionCart(data);
            //return View("GetCart", GetSessionCart());
        }
        /// <summary>
        /// Xóa mặt hàng khỏi giỏ
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ///
        [HttpPost]
        public IActionResult RemoveFromCart(int id)
        {
            try
            {
                var cart = GetSessionCart(); // lấy giỏ hàng từ session
                int index = cart.FindIndex(m => m.ProductID == id); // tìm chỉ số mặt hàng trong giỏ
                if (index >= 0)
                {
                    cart.RemoveAt(index); // xóa mặt hàng khỏi giỏ
                    // Cập nhật lại session sau khi xóa
                    ApplicationContext.SetSessionData(CART, cart);
                    return Json(new ApiResult()
                    {
                        Code = 1,
                        Message = "Xóa mặt hàng khỏi giỏ thành công",
                    });
                }
                return Json(new ApiResult()
                {
                    Code = 0,
                    Message = "Mặt hàng không tồn tại trong giỏ",
                });
            }
            catch (Exception ex)
            {
                return Json(new ApiResult()
                {
                    Code = 0,
                    Message = "Lỗi xóa mặt hàng khỏi giỏ: " + ex.Message,
                });
            }
        }
        public async Task<IActionResult> Init(string customerID, string deliveryProvince, string deliveryAddress)
        {
            try
            {
                int orderID = 0;
                var cart = GetSessionCart();            // Lấy giỏ hàng từ session, nguyên tắc giỏ hàng không null
                if (cart.Count == 0)
                {
                    return Json(new ApiResult()
                    {
                        Code = 0,
                        Message = "Giỏ hàng rỗng, không thể khởi tạo đơn hàng"
                    });
                }
                if (string.IsNullOrWhiteSpace(customerID))
                {
                    return Json(new ApiResult()
                    {
                        Code = 0,
                        Message = "Vui lòng chọn khách hàng"
                    });
                }
                if (string.IsNullOrWhiteSpace(deliveryProvince))
                {
                    return Json(new ApiResult()
                    {
                        Code = 0,
                        Message = "Vui lòng chọn tỉnh/thành phố giao hàng"
                    });
                }
                if (string.IsNullOrWhiteSpace(deliveryAddress))
                {
                    return Json(new ApiResult()
                    {
                        Code = 0,
                        Message = "Vui lòng nhập địa chỉ giao hàng"
                    });
                }
                Order data = new Order()
                {
                    CustomerID = Convert.ToInt32(customerID),
                    DeliveryProvince = deliveryProvince,
                    DeliveryAddress = deliveryAddress,
                    EmployeeID = Convert.ToInt32(User.GetUserData()?.UserId),
                    Status = Constants.ORDER_INIT
                };
                orderID = await OrderDataService.OrderDB.AddAsync(data);
                foreach (var item in cart)
                {
                    await OrderDataService.OrderDB.SaveDetailAsync(orderID, item.ProductID, item.Quantity, item.SalePrice);
                }
                return Json(new ApiResult()
                {
                    Code = 1,
                    Message = "Khởi tạo đơn hàng thành công"
                });
            }
            catch (Exception ex)
            {
                return Json(new ApiResult()
                {
                    Code = 0,
                    Message = "Lỗi khởi tạo đơn hàng: " + ex.Message
                });
            }
        }
        /// <summary>
        /// Xóa giỏ hàng
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ClearCart()
        {
            try
            {
               var cart = GetSessionCart();
                cart.Clear();
                ApplicationContext.SetSessionData(CART, cart);
                return Json(new ApiResult()
                {
                    Code = 1,
                    Message = "Xóa giỏ hàng thành công"
                });
            }
            catch (Exception ex)
            {
                return Json(new ApiResult()
                {
                    Code = 0,
                    Message = "Lỗi xóa giỏ hàng: " + ex.Message
                });
            }
        }
        /// <summary>
        /// Giảm số lượng mặt hàng trong giỏ
        /// </summary>
        /// <param name="id"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult updateCartQuantity(int id, int quantity)
        {
            try
            {
                var cart = GetSessionCart();
                var existProduct = cart.Find(m => m.ProductID == id);
                if (existProduct != null)
                {
                    existProduct.Quantity = existProduct.Quantity + quantity;
                    ApplicationContext.SetSessionData(CART, cart);
                    return Json(new ApiResult()
                    {
                        Code = 1,
                        Message = "Cập nhật số lượng mặt hàng trong giỏ thành công"
                    });
                }
                return Json(new ApiResult()
                {
                    Code = 0,
                    Message = "Mặt hàng không tồn tại trong giỏ"
                });
            }
            catch (Exception ex)
            {
                return Json(new ApiResult()
                {
                    Code = 0,
                    Message = "Lỗi cập nhật số lượng mặt hàng trong giỏ: " + ex.Message
                });
            }
        }
        private const string CART = "CART";
        private List<OrderDetail> GetSessionCart()
        {
            var cart= ApplicationContext.GetSessionData<List<OrderDetail>>(CART);
            if (cart == null)
            {
                cart = new List<OrderDetail>();
            }
            return cart;
        }
        private void AddSesssionCart(OrderDetail data)
        {
            var cart = GetSessionCart();
            var existOrderDetails = cart.Find(m => m.ProductID == data.ProductID);
            if (existOrderDetails == null)
            {
                cart.Add(data);
            }
            else
            {
                existOrderDetails.Quantity += data.Quantity;
                existOrderDetails.SalePrice = data.SalePrice;
            }
            ApplicationContext.SetSessionData(CART, cart);
        }
        //public IActionResult Delete(int id = 0)
        //{
        //    return View();
        //}
        public async Task<IActionResult> Delete(int id = 0)
        {
            if (id <= 0)
                return RedirectToAction("Index");

            bool result = await OrderDataService.OrderDB.DeleteAsync(id);
            TempData[result ? "Message" : "Error"] =
                result ? "Xóa đơn hàng thành công"
                       : "Không thể xóa đơn hàng";

            return RedirectToAction("Index");
        }

        //public IActionResult Details(int id = 0)
        //{
        //    return View();
        //}
        public async Task<IActionResult> Details(int id = 0)
        {
            if (id <= 0)
            {
                return RedirectToAction("Index");
            }

            // Lấy thông tin đơn hàng
            var order = await OrderDataService.OrderDB.GetAsync(id);
            if (order == null)
            {
                return RedirectToAction("Index");
            }

            // Lấy danh sách chi tiết đơn hàng
            var details = await OrderDataService.OrderDB.ListDetailsAsync(id);

            // Gán mô hình cho View
            var model = new OrderDetailModel()
            {
                Order = order,
                Details = details
            };

            return View(model);
        }
        public async Task<IActionResult> Accept(int id = 0)
        {
            if (id <= 0)
                return RedirectToAction("Index");
            // Cập nhật nhân viên phụ trách đơn
            var order = await OrderDataService.OrderDB.GetAsync(id);
            if (order != null && !order.EmployeeID.HasValue)
            {
                var employeeId = GetCurrentEmployeeId();
                if (employeeId.HasValue)
                    await OrderDataService.OrderDB.UpdateEmployeeAsync(id, employeeId.Value);
            }

            bool result = await OrderDataService.OrderDB.AcceptAsync(id);
            TempData[result ? "Message" : "Error"] =
                result ? "Duyệt đơn hàng thành công" : "Không thể duyệt đơn hàng";

            return RedirectToAction("Details", new { id });
        }

        //public IActionResult EditDetail(int id = 0)
        //{
        //    return View();
        //}
        [HttpPost]
        public async Task<IActionResult> UpdateDetail(OrderDetail model)
        {
            bool result = await OrderDataService.OrderDB.SaveDetailAsync(
                model.OrderID,
                model.ProductID,
                model.Quantity,
                model.SalePrice
            );

            TempData[result ? "Message" : "Error"] =
                result ? "Cập nhật chi tiết đơn hàng thành công"
                       : "Không thể cập nhật chi tiết";

            return RedirectToAction("Details", new { id = model.OrderID });
        }

        [HttpGet]
        public async Task<IActionResult> UpdateDetail(int id, int productId)
        {
            var detail = await OrderDataService.OrderDB.GetDetailAsync(id, productId);
            if (detail == null)
                return Content("Chi tiết đơn hàng không tồn tại");

            return View(detail);
        }


        //public IActionResult DeleteDetail(int id, int ProductId)
        //{
        //    return View();
        //}
        [HttpPost]
        public async Task<IActionResult> DeleteDetail(int id, int productId)
        {
            bool result = await OrderDataService.OrderDB.DeleteDetailAsync(id, productId);

            TempData[result ? "Message" : "Error"] =
                result ? "Xóa mặt hàng khỏi đơn hàng thành công"
                       : "Không thể xóa mặt hàng";

            return RedirectToAction("Details", new { id });
        }


        //public IActionResult Shipping(int id = 0)
        //{
        //    return View();
        //}
        public async Task<IActionResult> ShippingAsync(int id = 0)
        {
            if (id <= 0)
                return Content("Yêu cầu không hợp lệ");

            ViewBag.OrderID = id;
            // Lấy danh sách shipper từ DB
            var shippers = await ShipperDataService.ShipperDB.ListAsync();

            ViewBag.Shippers = shippers
                .Select(s => new SelectListItem
                {
                    Value = s.ShipperID.ToString(),
                    Text = s.ShipperName
                })
                .ToList();
            return View();
        }
        //[HttpPost]
        //public async Task<IActionResult> Shipping(int orderID, int shipperID)
        //{
        //    bool result = await OrderDataService.OrderDB.ShipAsync(orderID, shipperID);

        //    TempData[result ? "Message" : "Error"] =
        //        result ? "Chuyển đơn hàng cho người giao thành công"
        //               : "Không thể chuyển giao đơn hàng";

        //    return RedirectToAction("Details", new { id = orderID });
        //}
        [HttpPost]
        public async Task<IActionResult> Shipping(int orderID, int shipperID)
        {
            if (orderID <= 0 || shipperID <= 0)
                return RedirectToAction("Details", new { id = orderID });
            var order = await OrderDataService.OrderDB.GetAsync(orderID);
            if (order != null && !order.EmployeeID.HasValue)
            {
                var employeeId = GetCurrentEmployeeId();
                if (employeeId.HasValue)
                    await OrderDataService.OrderDB.UpdateEmployeeAsync(orderID, employeeId.Value);
            }
            bool result = await OrderDataService.OrderDB.ShipAsync(orderID, shipperID);

            TempData[result ? "Message" : "Error"] =
                result ? "Chuyển đơn hàng cho người giao thành công"
                       : "Không thể chuyển giao đơn hàng";

            return RedirectToAction("Details", new { id = orderID });
        }
        public async Task<IActionResult> Finish(int id = 0)
        {
            if (id <= 0)
                return RedirectToAction("Index");
            var order = await OrderDataService.OrderDB.GetAsync(id);
            if (order != null && !order.EmployeeID.HasValue)
            {
                var employeeId = GetCurrentEmployeeId();
                if (employeeId.HasValue)
                    await OrderDataService.OrderDB.UpdateEmployeeAsync(id, employeeId.Value);
            }

            bool result = await OrderDataService.OrderDB.FinishAsync(id);
            TempData[result ? "Message" : "Error"] =
                result ? "Đơn hàng đã được hoàn tất"
                       : "Không thể hoàn tất đơn hàng";

            return RedirectToAction("Details", new { id });
        }
        public async Task<IActionResult> Cancel(int id = 0)
        {
            if (id <= 0)
                return RedirectToAction("Index");
            var order = await OrderDataService.OrderDB.GetAsync(id);
            if (order != null && !order.EmployeeID.HasValue)
            {
                var employeeId = GetCurrentEmployeeId();
                if (employeeId.HasValue)
                    await OrderDataService.OrderDB.UpdateEmployeeAsync(id, employeeId.Value);
            }

            bool result = await OrderDataService.OrderDB.CancelAsync(id);
            TempData[result ? "Message" : "Error"] =
                result ? "Đã hủy đơn hàng"
                       : "Không thể hủy đơn hàng";

            return RedirectToAction("Details", new { id });
        }
        public async Task<IActionResult> Reject(int id = 0)
        {
            if (id <= 0)
                return RedirectToAction("Index");
            var order = await OrderDataService.OrderDB.GetAsync(id);
            if (order != null && !order.EmployeeID.HasValue)
            {
                var employeeId = GetCurrentEmployeeId();
                if (employeeId.HasValue)
                    await OrderDataService.OrderDB.UpdateEmployeeAsync(id, employeeId.Value);
            }

            bool result = await OrderDataService.OrderDB.RejectAsync(id);
            TempData[result ? "Message" : "Error"] =
                result ? "Đã từ chối đơn hàng"
                       : "Không thể từ chối đơn hàng";

            return RedirectToAction("Details", new { id });
        }

    }
}
/*
  public int Sum(int id)      // tham số sau ?id=  | & | /id
        {
            return 14 * 2 + id;
        }
 */