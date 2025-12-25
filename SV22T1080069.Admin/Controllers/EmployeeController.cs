using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1080069.Admin.Models;
using SV22T1080069.BusinessLayers;
using SV22T1080069.DomainModels;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SV22T1080069.Admin.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.Administrator}")]
    public class EmployeeController : Controller
    {
        public const int PAGE_SIZE = 9;
        private const string EMPLOYEE_SEARCH_CONDITION = "EmployeeSearchCondition";
        //public async Task<IActionResult> Index(int page = 1, string searchValue = "")
        //{
        //    var Data = await CommonDataService.EmployeeDB.ListAsync(page, PAGE_SIZE, searchValue);
        //    var rowCount = await CommonDataService.EmployeeDB.CountAsync(searchValue);
        //    var model = new PaginationSearchResult<Employee>()
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
            var condition = ApplicationContext.GetSessionData<PaginationSearchCondition>(EMPLOYEE_SEARCH_CONDITION);
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
            var data = await CommonDataService.EmployeeDB.ListAsync(condition.Page, condition.PageSize, condition.SearchValue);
            var rowCount = await CommonDataService.EmployeeDB.CountAsync(condition.SearchValue);
            var model = new PaginationSearchResult<Employee>()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue,
                RowCount = rowCount,
                Data = data
            };
            // Lưu lại điều kiện tìm kiếm vào trong session
            ApplicationContext.SetSessionData(EMPLOYEE_SEARCH_CONDITION, condition);

            return View(model);
        }
        public async Task<IActionResult> Delete(int id = 0)
        {
            if (Request.Method == "POST")
            {
                await CommonDataService.EmployeeDB.DeleteAsync(id);
                return RedirectToAction("Index");
            }
            else
            {
                var model = await CommonDataService.EmployeeDB.GetAsync(id);
                if (model == null)
                {
                    return RedirectToAction("Index");
                }
                return View(model);
            }
        }
        public IActionResult Create()
        {
            @ViewBag.Title = "Thêm Nhân Viên";
            var model = new EmployeeEditModel()
            {
                EmployeeId = 0,
                Photo = "nophoto.png"
            };

            return View("Edit", model);
        }
        public async Task<IActionResult> Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật thông tin nhân viên";
            var employee = await CommonDataService.EmployeeDB.GetAsync(id);
            if (employee == null)
                return RedirectToAction("Index");

            var model = new EmployeeEditModel()
            {
                EmployeeId = employee.EmployeeId,
                FullName = employee.FullName,
                BirthDate = employee.BirthDate,
                Address = employee.Address,
                Email = employee.Email,
                Phone = employee.Phone,
                Photo = employee.Photo,
                IsWorking = employee.IsWorking
            };
            return View(model);
        }
        /// <summary>
        /// / Xử lý lưu dữ liệu từ form (Create, Edit)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SaveData(EmployeeEditModel model)
        {
            //if (data.BirthDate == default(DateTime))
            //{
            //    ModelState.AddModelError("BirthDate", "Vui lòng chọn ngày sinh.");
            //    return View("Edit", data); // hoặc View("Create", data) 
            //}
            //if (data.EmployeeId == 0)
            //{
            //    await CommonDataService.EmployeeDB.AddAsync(data);
            //}
            //else
            //{
            //    await CommonDataService.EmployeeDB.UpdateAsync(data);
            //}
            //return RedirectToAction("Index");
            //TODO: Kiểm tra dữ liệu đầu vào

            //Nếu có ảnh thì upload ảnh lên và lấy tên file ảnh mới upload cho Photo
            try
            {
                ViewBag.Title = model.EmployeeId == 0 ? "Bổ sung nhân viên mới" : "Cập nhật thông tin nhân viên";
                // Kiếm tra tính đúng đắn của dữ liệu nhập vào
                if (string.IsNullOrWhiteSpace(model.FullName))
                    ModelState.AddModelError(nameof(model.FullName), "Tên nhân viên không được để trống");
                //if (string.IsNullOrWhiteSpace(model.BirthDate))
                 //   ModelState.AddModelError(nameof(model.BirthDate), "Ngày sinh không được để trống");
                if (string.IsNullOrWhiteSpace(model.Phone))
                    ModelState.AddModelError(nameof(model.Phone), "Điện thoại không được để trống");
                if (string.IsNullOrWhiteSpace(model.Email))
                    ModelState.AddModelError(nameof(model.Email), "Email không được để trống");
                if (string.IsNullOrWhiteSpace(model.Address))
                    ModelState.AddModelError(nameof(model.Address), "Địa chỉ không được để trống");
                
                if (model.UploadPhoto != null)
                {
                    string fileName = $"{DateTime.Now.Ticks}_{model.UploadPhoto.FileName}";
                    string filePath = Path.Combine(ApplicationContext.WWWRootPath, @"images\employees", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.UploadPhoto.CopyToAsync(stream);
                    }
                    model.Photo = fileName;
                }
                // thông báo lỗi và yêu cầu nhập lại dữ liệu nếu có trường hợp dữ liệu không đúng
                if (!ModelState.IsValid)                                    // kiểm tra nếu có lỗi
                {
                    return View("Edit", model);
                }
                Employee data = new Employee()
                {
                    EmployeeId = model.EmployeeId,
                    FullName = model.FullName,
                    BirthDate = model.BirthDate,
                    Address = model.Address,
                    Email = model.Email,
                    Phone = model.Phone,
                    Photo = model.Photo,
                    IsWorking = model.IsWorking
                };

                if (data.EmployeeId == 0)
                {
                    await CommonDataService.EmployeeDB.AddAsync(data);
                }
                else
                {
                    await CommonDataService.EmployeeDB.UpdateAsync(data);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Error", ex.Message);
                return View("Edit", model);
            }
        }
    }
}
