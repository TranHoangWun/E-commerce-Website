using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1080069.Admin.Models;
using SV22T1080069.BusinessLayers;
using SV22T1080069.DataLayers;
using SV22T1080069.DomainModels;
using System.Buffers;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace SV22T1080069.Admin.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private const int PAGESIZE = 20;
        private const string CUSTOMER_SEARCH_CONDITION = "CustomerSearchCondition";
        /*public IActionResult Index()
        {
            return View();
        } */
        public IActionResult Index()                    //int page = 1, string searchValue = ""
        {
            // 2 lỏ
            //------------------------------
            // Nếu trong session có lưu điều kiện tìm kiếm thì sử dụng lại điều kiện đó, 
            // ngược lại thì tạo điều kiện tìm kiếm mặc định 
            var condition = ApplicationContext.GetSessionData<PaginationSearchCondition>("CustomerSearchCondition");
            if (condition == null)
            {
                condition = new PaginationSearchCondition()
                {
                    Page = 1,
                    PageSize = PAGESIZE,
                    SearchValue = ""
                };
               
            }
            return View(condition);
        }
        public async Task<IActionResult> Search(PaginationSearchCondition condition)
        {
            var data = await CommonDataService.CustomerDB.ListAsync(condition.Page, condition.PageSize, condition.SearchValue);
            var rowCount = await CommonDataService.CustomerDB.CountAsync(condition.SearchValue);
            var model = new PaginationSearchResult<Customer>()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue,
                RowCount = rowCount,
                Data = data
            };
            // Lưu lại điều kiện tìm kiếm vào trong session
            ApplicationContext.SetSessionData(CUSTOMER_SEARCH_CONDITION, condition);

            return View(model);
        }
        /// <summary>
        /// Thêm mới khách hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewBag.Title = "Thêm khách hàng";
            var model = new Customer()
            {
                CustomerID = 0
            };
            return View("Edit", model);
        }
        /// <summary>
        /// Chỉnh sửa thông tin khách hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật thông tin khách hàng";
            var model = await CommonDataService.CustomerDB.GetAsync(id);
            if (model == null)
                return RedirectToAction("Index");   
            return View(model);                                         
        }
        /// <summary>
        /// dùng để lưu dữ liệu khách hàng (thêm mới hoặc cập nhật)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<IActionResult> SaveData(Customer data)
        {
            try
            {
                ViewBag.Title = data.CustomerID == 0 ? "Bổ sung khách hàng mới" : "Cập nhật thông tin khách hàng";
                // Kiếm tra tính đúng đắn của dữ liệu nhập vào
                if (string.IsNullOrWhiteSpace(data.CustomerName))
                    ModelState.AddModelError(nameof(data.CustomerName), "Tên khách hàng không được để trống");
                if (string.IsNullOrWhiteSpace(data.ContactName))
                    ModelState.AddModelError(nameof(data.ContactName), "Tên giao dịch không được để trống");
                if (string.IsNullOrWhiteSpace(data.Phone))
                    ModelState.AddModelError(nameof(data.Phone), "Điện thoại không được để trống");
                if (string.IsNullOrWhiteSpace(data.Email))
                    ModelState.AddModelError(nameof(data.Email), "Email không được để trống");
                if (string.IsNullOrWhiteSpace(data.Address))
                    ModelState.AddModelError(nameof(data.Address), "Địa chỉ không được để trống");
                if (string.IsNullOrWhiteSpace(data.Province))
                    ModelState.AddModelError(nameof(data.Province), "Tỉnh/Thành phố không được để trống");

                // thông báo lỗi và yêu cầu nhập lại dữ liệu nếu có trường hợp dữ liệu không đúng
                if (!ModelState.IsValid)                                    // kiểm tra nếu có lỗi
                {
                    return View("Edit", data);
                }

                if (data.CustomerID == 0)
                {
                    await CommonDataService.CustomerDB.AddAsync(data);
                }
                else
                {
                    await CommonDataService.CustomerDB.UpdateAsync(data);
                }
                return RedirectToAction("Index");
            }
            catch(Exception ex) 
            {
                ModelState.AddModelError("Error", ex.Message);
                return View("Edit", data);
            }
        }
        public async Task<IActionResult> Delete(int id = 0)
        {
            //ViewBag.Title = "Xóa khách hàng";         không cần thiết vì chỉ dùng duy nhất 1 view 
            if (Request.Method == "POST")               // kiểm tra nếu phương thức gửi lên là POST
            {
                // Thực hiện xóa
                await CommonDataService.CustomerDB.DeleteAsync(id);
                return RedirectToAction("Index");
            }
            else
            {
                var model = await CommonDataService.CustomerDB.GetAsync(id);
                if (model == null)
                {
                    return RedirectToAction("Index");
                }
                return View(model);                     // trả về view xóa với dữ liệu là model
            }
            //return View();
        }
    }
}
/*
             * string connectionString = "Server=LAPTOP-1AL3OBJH;Database=LiteCommerceDB;User Id=sa;Password=123;TrustServerCertificate=True;";
            var dal = new CustomerDAL(connectionString);
            var data = await dal.ListAsync();
            return View(data.ToList());
             */
//-----
/*var data = await CommonDataService.CustomerDB.ListAsync(page, PAGESIZE, searchValue);
var rowCount = await CommonDataService.CustomerDB.CountAsync(searchValue);
var model = new PaginationSearchResult<Customer>()
{
    Page = page,
    PageSize = PAGESIZE,
    SearchValue = searchValue,
    RowCount = rowCount,
    Data = data
};
return View(model);*/