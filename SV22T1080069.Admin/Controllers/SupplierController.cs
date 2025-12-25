using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1080069.Admin.Models;
using SV22T1080069.BusinessLayers;
using SV22T1080069.DataLayers;
using SV22T1080069.DomainModels;
using System.Runtime.CompilerServices;

namespace SV22T1080069.Admin.Controllers
{
    [Authorize]
    public class SupplierController : Controller     // kế thừa lớp 
    {
        /*public IActionResult Index()                // kiểu dữ liệu trả về, hàm tên Index 
        {
            return View();                          // trả về giao diện
        }*/
        /// <summary>
        /// tìm kiếm và hiển thị danh sách nhà cung cấp dưới dạng phân trang
        /// </summary>
        /// <param name="page"></param>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        private const int PAGESIZE = 20;
        private const string SUPPLIER_SEARCH_CONDITION = "SupplierSearchCondition";
        //public async Task<IActionResult> Index(int page = 1, string searchValue = "")
        public IActionResult Index()
        {
            var condition = ApplicationContext.GetSessionData<PaginationSearchCondition>("SupplierSearchCondition");
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
            var data = await CommonDataService.SupplierDB.ListAsync(condition.Page, condition.PageSize, condition.SearchValue);
            var rowCount = await CommonDataService.SupplierDB.CountAsync(condition.SearchValue);
            var model = new PaginationSearchResult<Supplier>()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue,
                RowCount = rowCount,
                Data = data
            };
            // Lưu lại điều kiện tìm kiếm vào trong session
            ApplicationContext.SetSessionData(SUPPLIER_SEARCH_CONDITION, condition);

            return View(model);
        }
        /// <summary>
        /// Thêm mới nhà cung cấp
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewBag.Title = "Thêm nhà cung cấp";
            var model = new Supplier()
            {
                SupplierID = 0
            };
            return View("Edit", model);
        }
        /// <summary>
        /// Chỉnh sửa thông tin nhà cung cấp
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật thông tin nhà cung cấp";
            var model = await CommonDataService.SupplierDB.GetAsync(id);        // Lấy dữ liệu nhà cung cấp từ CSDL
            if (model == null)
                return RedirectToAction("Index");
            return View(model);
        }
        /// <summary>
        /// Save dữ liệu nhà cung cấp (thêm mới hoặc cập nhật)
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public async Task<IActionResult> SaveData(Supplier data)
        {
            ViewBag.Title = data.SupplierID == 0 ? "Bổ sung nhà cung cấp" : "Cập nhật thông tin nhà cung cấp";
            // Kiểm tra tính hợp lệ của dữ liệu
            if (string.IsNullOrWhiteSpace(data.SupplierName))
                ModelState.AddModelError(nameof(data.SupplierName), "Tên nhà cung cấp không được để trống");
            if (string.IsNullOrWhiteSpace(data.ContactName))   
                ModelState.AddModelError(nameof(data.ContactName), "Tên người liên hệ không được để trống");
            if (string.IsNullOrWhiteSpace(data.Phone))
                ModelState.AddModelError(nameof(data.Phone), "Số điện thoại không được để trống");
            if (string.IsNullOrWhiteSpace(data.Email))
                ModelState.AddModelError(nameof(data.Email), "Email không được để trống");
            if (string.IsNullOrWhiteSpace(data.Address))
                ModelState.AddModelError(nameof(data.Address), "Địa chỉ không được để trống");
            if (string.IsNullOrWhiteSpace(data.Province))
                ModelState.AddModelError(nameof(data.Province), "Tỉnh/Thành phố không được để trống");
            if (!ModelState.IsValid)
                return View("Edit", data);

            if (data.SupplierID == 0)
            {
                await CommonDataService.SupplierDB.AddAsync(data);
            }
            else
            {
                await CommonDataService.SupplierDB.UpdateAsync(data);
            }
            return RedirectToAction("Index");
        }
        /// <summary>
        /// Xóa nhà cung cấp
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int id = 0)
        {
            if (Request.Method == "POST")
            {
                await CommonDataService.SupplierDB.DeleteAsync(id);
                return RedirectToAction("Index");
            }
            else
            {
                var model = await CommonDataService.SupplierDB.GetAsync(id);
                if (model == null)
                {
                    return RedirectToAction("Index");
                }
                ViewBag.Title = "Xóa nhà cung cấp";
                return View(model);
            }
            //return View();
        }
    }
}
/*
           //string connectionString = "Server=LAPTOP-1AL3OBJH;Database=LiteCommerceDB;User Id=sa;Password=123;TrustServerCertificate=True;";
           //var dal = new SuppliedDAL(connectionString);
           //var data = await dal.ListAsync();
           //return View(data.ToList());
           var data = await CommonDataService.SupplierDB.ListAsync(page, 20, searchValue);
           var rowCount = await CommonDataService.SupplierDB.CountAsync(searchValue);
           var model = new Models.PaginationSearchResult<DomainModels.Supplier>()
           {
               Page = page,
               PageSize = 20,
               SearchValue = searchValue,
               RowCount = rowCount,
               Data = data
           };
           return View(model); */