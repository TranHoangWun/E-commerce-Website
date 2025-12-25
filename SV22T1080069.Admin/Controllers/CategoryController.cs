using Microsoft.AspNetCore.Mvc;
using SV22T1080069.Admin.Models;
using SV22T1080069.BusinessLayers;
using SV22T1080069.DomainModels;

namespace SV22T1080069.Admin.Controllers
{
    public class CategoryController : Controller
    {
        public const int PAGE_SIZE = 20;
        private const string CATAGORY_SEARCH_CONDITION = "CatagorySearchCondition";
        //public async Task<IActionResult> Index(int page = 1, string searchValue = "")
        //{
        //    var Data = await CommonDataService.CategoryDB.ListAsync(page, PAGE_SIZE, searchValue);
        //    var rowCount = await CommonDataService.CategoryDB.CountAsync(searchValue);
        //    var model = new PaginationSearchResult<Category>()
        //    {
        //        Page = page,
        //        PageSize = PAGE_SIZE,
        //        SearchValue = searchValue,
        //        RowCount = rowCount,
        //        Data = Data
        //    };
        //    return View(model);
        //}
        public IActionResult Index()
        {
            var condition = ApplicationContext.GetSessionData<PaginationSearchCondition>(CATAGORY_SEARCH_CONDITION);
            if (condition == null)
            {
                condition = new PaginationSearchCondition()
                {
                    Page = 1,
                    PageSize = PAGE_SIZE,
                    SearchValue = ""
                };
            }
            return View(condition);
        }
        public async Task<IActionResult> Search(PaginationSearchCondition condition)
        {
            var data = await CommonDataService.CategoryDB.ListAsync(condition.Page, condition.PageSize, condition.SearchValue);
            var rowCount = await CommonDataService.CategoryDB.CountAsync(condition.SearchValue);
            var model = new PaginationSearchResult<Category>()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue,
                RowCount = rowCount,
                Data = data
            };
            // Lưu lại điều kiện tìm kiếm vào trong session
            ApplicationContext.SetSessionData(CATAGORY_SEARCH_CONDITION, condition);
            return View(model);
        }
        public IActionResult Create()
        {
            @ViewBag.Title = "Thêm loại hàng";
            var model = new Category()
            {
                CategoryID = 0
            };
            return View("Edit", model);
        }
        public async Task<IActionResult> Edit(int id = 0)
        {
            @ViewBag.Title = "Chỉnh sửa loại hàng";
            var model = await CommonDataService.CategoryDB.GetAsync(id);
            if (model == null)
            {
                return RedirectToAction("Index");
            }
            return View(model);
        }
        /// <summary>
        /// // Lưu thông tin loại hàng (Thêm mới hoặc Cập nhật)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SaveData(Category data)
        {
           try
            {
                ViewBag.title = data.CategoryID == 0 ? "Thêm loại hàng" : "Cập nhật loại hàng";
                if (string.IsNullOrWhiteSpace(data.CategoryName))
                    ModelState.AddModelError(nameof(data.CategoryName), "Tên loại hàng không được để trống");
                if (string.IsNullOrWhiteSpace(data.Description))
                    ModelState.AddModelError(nameof(data.Description), "Mô tả không được để trống");
                if (!ModelState.IsValid)                                    // kiểm tra nếu có lỗi
                {
                    return View("Edit", data);
                }
                if (data.CategoryID == 0)   
                {
                    await CommonDataService.CategoryDB.AddAsync(data);
                }
                else
                {
                    await CommonDataService.CategoryDB.UpdateAsync(data);
                }
                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("Edit", data);
            }
        }
        public async Task<IActionResult> Delete(int id = 0)
        {
            if (Request.Method == "POST")
            {
                // Xóa dữ liệu
                await CommonDataService.CategoryDB.DeleteAsync(id);
                return RedirectToAction("Index");
            }
            else
            {
                // Hiển thị thông tin để xác nhận việc xóa
                var model = await CommonDataService.CategoryDB.GetAsync(id);
                if (model == null)
                {
                    return RedirectToAction("Index");
                }
                return View(model);
            }
        }
    }
}
