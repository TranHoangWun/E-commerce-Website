using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1080069.Admin.Models;
using SV22T1080069.BusinessLayers;
using SV22T1080069.DomainModels;

namespace SV22T1080069.Admin.Controllers
{
    [Authorize]
    public class ShipperController : Controller
    {

        /*public IActionResult Index()
        {
            return View();
        }*/
        /// <summary>
        /// Index hiển thị danh sách nhà vận chuyển dưới dạng phân trang
        /// </summary>
        /// <param name="page"></param>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        private const int PAGESIZE = 20;
        private const string SHIPPER_SEARCH_CONDITION = "ShipperSearchCondition";
        // async Task<IActionResult> Index(int page = 0, string searchValue = "")
        public IActionResult Index()
        {
            var condition = ApplicationContext.GetSessionData<PaginationSearchCondition>("ShipperSearchCondition");
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
            var data = await CommonDataService.ShipperDB.ListAsync(condition.Page, condition.PageSize, condition.SearchValue);
            var rowCount = await CommonDataService.ShipperDB.CountAsync(condition.SearchValue);
            var model = new PaginationSearchResult<Shipper>()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue,
                RowCount = rowCount,
                Data = data
            };
            // Lưu lại điều kiện tìm kiếm vào trong session
            ApplicationContext.SetSessionData(SHIPPER_SEARCH_CONDITION, condition);

            return View(model);
        }
        /// <summary>
        /// Thêm mới người giao hàng 
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewBag.Title = "Thêm nhà vận chuyển";
            var model = new Shipper()
            {
                ShipperID = 0
            };
            return View("Edit", model);
        }
        /// <summary>
        /// Chỉnh sửa thông tin người giao hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật thông tin nhà vận chuyển";
            var model = await CommonDataService.ShipperDB.GetAsync(id);
            if (model == null)
                return RedirectToAction("Index");
            return View(model);
        }
        /// <summary>
        /// SavaData lưu thông tin người giao hàng (thêm mới hoặc cập nhật)
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> SaveData(Shipper data)
        {
            //TODO : kiểm tra tính hợp lệ của dữ liệu
            try
            {
                ViewBag.Title = data.ShipperID == 0 ? "Bổ sung nhà vận chuyển mới" : "Cập nhật thông tin nhà vận chuyển";
                if (string.IsNullOrWhiteSpace(data.ShipperName))
                    ModelState.AddModelError(nameof(data.ShipperName), "Tên người giao hàng không được để trống");
                if (string.IsNullOrWhiteSpace(data.Phone))
                    ModelState.AddModelError(nameof(data.Phone), "Điện thoại không được để trống");
                if (!ModelState.IsValid)                                    // kiểm tra nếu có lỗi
                {
                    return View("Edit", data);
                }
                if (data.ShipperID == 0)
                {
                    await CommonDataService.ShipperDB.AddAsync(data);
                }
                else
                {
                    await CommonDataService.ShipperDB.UpdateAsync(data);
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                {
                    ModelState.AddModelError("Error", ex.Message);
                    return View("Edit", data);
                }
            }
        }
        /// <summary>
        /// Xóa người giao hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int id = 0)
        {
            if (Request.Method == "POST")
            {
                await CommonDataService.ShipperDB.DeleteAsync(id);
                return RedirectToAction("Index");
            }
            else
            {
                var model = await CommonDataService.ShipperDB.GetAsync(id);
                if (model == null)
                {
                    return RedirectToAction("Index");
                }
                return View(model);                                             // truyền model để hiển thị thông tin người giao hàng cần xóa
            }
            return View();
        }
    }
}
////string connectionString = "Server=LAPTOP-1AL3OBJH;Database=LiteCommerceDB;User Id=sa;Password=123;TrustServerCertificate=True;";
////var dal = new DataLayers.ShipperDAL(connectionString);
////var data = await dal.ListAsync();
////return View(data.ToList());

//var data = await BusinessLayers.CommonDataService.ShipperDB.ListAsync(page, 20, searchValue);
//var rowCount = await BusinessLayers.CommonDataService.ShipperDB.CountAsync(searchValue);
//var model = new Models.PaginationSearchResult<DomainModels.Shipper>()
//{
//    Page = page,
//    PageSize = 20,
//    SearchValue = searchValue,
//    RowCount = rowCount,
//    Data = data
//};
//return View(model);