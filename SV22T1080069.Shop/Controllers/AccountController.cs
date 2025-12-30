using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SV22T1080069.BusinessLayers;
using SV22T1080069.DomainModels;
using SV22T1080069.Shop.Models;

namespace SV22T1080069.Shop.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        #region Login / Logout / Register

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
        {
            ViewBag.UserName = username;
            ViewBag.ReturnUrl = returnUrl;
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("Error", "Nhập tên đăng nhập và mật khẩu.");
                return View();
            }
            // 1. Thử đăng nhập Employee (nhân viên/admin)
            var userAccount = await UserAccountService.EmployeeUserAccountDB
                                                      .AuthenticateAsync(username, password);

            // 2. Nếu không phải Employee, thử đăng nhập Customer
            if (userAccount == null)
            {
                userAccount = await UserAccountService.CustomerUserAccountDB
                                                      .AuthenticateCustomerAsync(username, password);
            }
            if (userAccount == null)
            {
                ModelState.AddModelError("Error", "Đăng nhập thất bại.");
                return View();
            }
            // Tạo dữ liệu người dùng cho cookie
            WebUserData userData = new WebUserData()
            {
                UserId = userAccount.UserID,
                UserName = userAccount.UserName,
                DisplayName = userAccount.FullName,
                Email = userAccount.Email,
                Photo = userAccount.Photo,
                Roles = userAccount.RoleNames.Split(',').ToList()
            };
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                userData.CreatePrincipal()
            );
            // Xác định có phải customer hay không (dựa trên RoleNames)
            /*bool isCustomer = userAccount.RoleNames != null
                              && userAccount.RoleNames.Split(',')
                                     .Contains(WebUserRoles.Customer);

            // Nếu là khách hàng thì lưu CustomerID (UserID) vào Session
            if (isCustomer)
            {
                if (int.TryParse(userAccount.UserID, out var customerId))
                {
                    HttpContext.Session.SetInt32("CustomerID", customerId);
                }
            }
            else
            {
                // Nhân viên thì xóa CustomerID nếu có
                HttpContext.Session.Remove("CustomerID");
            }*/
            // Sau khi userAccount != null và SignInAsync đã chạy
            if (int.TryParse(userAccount.UserID, out var customerId))
            {
                HttpContext.Session.SetInt32("CustomerID", customerId);
            }

            // Điều hướng sau đăng nhập
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        ///Dùng để đăng ký tài khoản khách hàng
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            ViewBag.Provinces = await SelectListHelper.Provinces();
            return View();
        }
        /// <summary>
        /// Dùng để đăng ký tài khoản khách hàng
        /// </summary>
        /// <param name="fullName"></param>
        /// <param name="email"></param>
        /// <param name="phone"></param>
        /// <param name="address"></param>
        /// <param name="province"></param>
        /// <param name="password"></param>
        /// <param name="confirmPassword"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(
            string fullName,
            string email,
            string phone,
            string address,
            string province,
            string password,
            string confirmPassword)
        {
            // Gán lại để giữ giá trị khi lỗi
            ViewBag.FullName = fullName;
            ViewBag.Email = email;
            ViewBag.Phone = phone;
            ViewBag.Address = address;
            ViewBag.Province = province;
            ViewBag.Provinces = await SelectListHelper.Provinces(province);

            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("Error", "Vui lòng nhập đầy đủ thông tin bắt buộc.");
                return View();
            }

            if (password != confirmPassword)
            {
                ModelState.AddModelError("Error", "Mật khẩu xác nhận không khớp.");
                return View();
            }

            // Kiểm tra email đã tồn tại
            var existing = await UserAccountService.CustomerUserAccountDB
                                                   .GetCustomerByEmailAsync(email);
            if (existing != null)
            {
                ModelState.AddModelError("Error", "Email này đã được sử dụng.");
                return View();
            }

            var success = await UserAccountService.CustomerUserAccountDB.CreateAsync(
                fullName: fullName,
                email: email,
                phone: phone,
                address: address,
                province: province,
                password: password);

            if (!success)
            {
                ModelState.AddModelError("Error", "Đăng ký thất bại. Vui lòng thử lại.");
                return View();
            }
            TempData["Success"] = "Đăng ký thành công, vui lòng đăng nhập.";
            return RedirectToAction("Login", "Account");
        }
        /// <summary>
        /// Đăng xuất
        /// </summary>
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            //await HttpContext.SignOutAsync();
            await HttpContext.SignOutAsync(
       CookieAuthenticationDefaults.AuthenticationScheme); // quan trọng
            return RedirectToAction("Login", "Account");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        #endregion

        #region Đổi mật khẩu

        // Không dùng view riêng, đổi mật khẩu ngay trong tab của Profile
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(oldPassword) ||
                string.IsNullOrWhiteSpace(newPassword))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin.";
                TempData["ActiveTab"] = "password";
                return RedirectToAction("Profile");
            }

            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "Mật khẩu mới không khớp.";
                TempData["ActiveTab"] = "password";
                return RedirectToAction("Profile");
            }

            var user = User.GetUserData();
            if (user == null)
                return RedirectToAction("Login");

            // Xác định là customer hay employee dựa trên Roles
            bool isCustomer = user.Roles != null && user.Roles.Contains(WebUserRoles.Customer);

            bool success;
            if (isCustomer)
            {
                // Lấy customer theo email để biết CustomerID
                var customer = await UserAccountService.CustomerUserAccountDB
                                                       .GetCustomerByEmailAsync(user.Email!);
                if (customer == null)
                {
                    TempData["ProfileError"] = "Không tìm thấy khách hàng.";
                    TempData["ActiveTab"] = "password";
                    return RedirectToAction("Profile");
                }

                success = await UserAccountService.CustomerUserAccountDB
                                                  .ChangePassword(customer.CustomerID.ToString(),
                                                                  oldPassword, newPassword);
            }
            else
            {
                // Nhân viên
                var userId = user.UserId;
                success = await UserAccountService.EmployeeUserAccountDB
                                                  .ChangePassword(userId!, oldPassword, newPassword);
            }

            if (!success)
            {
                TempData["ProfileError"] = "Mật khẩu cũ không đúng.";
            }
            else
            {
                TempData["ProfileSuccess"] = "Đổi mật khẩu thành công!";
            }

            TempData["ActiveTab"] = "password";
            return RedirectToAction("Profile");
        }

        #endregion

        #region Hồ sơ khách hàng

        [Authorize(Roles = WebUserRoles.Customer)]
        public async Task<IActionResult> Profile(string? activeTab = null)
        {
            // nếu không truyền gì thì mặc định tab info
            ViewBag.ActiveTab = activeTab ?? (TempData["ActiveTab"] as string ?? "info");

            var user = User.GetUserData();
            if (user == null)
                return RedirectToAction("Login");

            // Lấy customer theo Email
            var customer = await UserAccountService.CustomerUserAccountDB
                                                   .GetCustomerByEmailAsync(user.Email!);
            if (customer == null)
                return RedirectToAction("Login");

            // Lấy tất cả đơn rồi lọc theo CustomerID
            var allOrders = await OrderDataService.OrderDB.ListAsync(
                page: 1, pageSize: 0, status: 0,
                fromTime: null, toTime: null, searchValue: ""
            );
            var ordersOfCustomer = allOrders
                .Where(o => o.CustomerID == customer.CustomerID)
                .ToList();

            var model = new CustomerProfileViewModel
            {
                CustomerID = customer.CustomerID,
                CustomerName = customer.CustomerName,
                ContactName = customer.ContactName,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                Province = customer.Province,
                //Photo = user.Photo ?? ""
                Photo = customer.Photo ?? ""   // dùng từ DB, không lấy từ user.Photo nữa
            };

            foreach (var o in ordersOfCustomer)
            {
                var items = (await OrderDataService.OrderDB.ListDetailsAsync(o.OrderID)).ToList();

                model.Orders.Add(new OrderProfileItemViewModel
                {
                    Order = o,
                    Items = items
                });
            }

            ViewBag.Provinces = await SelectListHelper.Provinces(customer.Province);
            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = WebUserRoles.Customer)]
        public async Task<IActionResult> UpdateProfile(CustomerProfileViewModel model)
        {
            var user = User.GetUserData();
            if (user == null)
                return RedirectToAction("Login");

            var customer = await UserAccountService.CustomerUserAccountDB
                                                   .GetCustomerByEmailAsync(user.Email!);
            if (customer == null)
                return RedirectToAction("Login");

            string photoFileName = customer.Photo ?? "";

            /* if (model.UploadPhoto != null && model.UploadPhoto.Length > 0)
             {
                 string fileName = $"{DateTime.Now.Ticks}_{model.UploadPhoto.FileName}";
                 string uploadsFolder = Path.Combine(ApplicationContext.WWWRootPath, @"images\customers");
                 Directory.CreateDirectory(uploadsFolder);
                 string filePath = Path.Combine(uploadsFolder, fileName);

                 using (var stream = new FileStream(filePath, FileMode.Create))
                 {
                     await model.UploadPhoto.CopyToAsync(stream);
                 }

                 photoFileName = fileName;
             }*/
            if (model.UploadPhoto != null && model.UploadPhoto.Length > 0)
            {
                var ext = Path.GetExtension(model.UploadPhoto.FileName);
                var fileName = $"{DateTime.Now.Ticks}{ext}";

                string uploadsFolder = ApplicationContext.CustomerImagePhysicalPath;
                Directory.CreateDirectory(uploadsFolder);

                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.UploadPhoto.CopyToAsync(stream);
                }

                photoFileName = fileName; // lưu vào DB
            }
            var success = await UserAccountService.CustomerUserAccountDB
                .UpdateProfileAsync(customer.CustomerID,
                                    model.CustomerName,
                                    model.ContactName,
                                    model.Phone,
                                    model.Address,
                                    model.Province,
                                    photoFileName);
            if (success)
            {
                var updated = await UserAccountService.CustomerUserAccountDB
                                                      .GetCustomerByEmailAsync(customer.Email);

                var oldUser = User.GetUserData();

                var newUserData = new WebUserData
                {
                    UserId = updated.CustomerID.ToString(),   // hoặc đổi WebUserData.UserId sang int
                    UserName = updated.Email,
                    DisplayName = updated.CustomerName,
                    Email = updated.Email,
                    Photo = updated.Photo,                    // ảnh mới trong DB
                    Roles = oldUser.Roles
                };

                await HttpContext.SignOutAsync();
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    newUserData.CreatePrincipal());
            }

            TempData[success ? "ProfileSuccess" : "ProfileError"] =
                success ? "Cập nhật thông tin thành công!" : "Cập nhật thông tin thất bại.";
            TempData["ActiveTab"] = "info";

            return RedirectToAction("Profile");
        }
        #endregion
        #region Hỗ trợ lịch sử đơn hàng
        // Ví dụ: chi tiết đơn hàng, khi bấm Quay lại sẽ quay về tab orders
        [Authorize(Roles = WebUserRoles.Customer)]
        public async Task<IActionResult> OrderDetail(int id)
        {
            var user = User.GetUserData();
            if (user == null)
                return RedirectToAction("Login");

            // lấy đơn + kiểm tra thuộc customer hiện tại (tùy bạn đã viết OrderDB.GetAsync hay chưa)
            var order = await OrderDataService.OrderDB.GetAsync(id);
            if (order == null)
                return RedirectToAction("Profile", new { activeTab = "orders" });

            // TODO: build view model chi tiết đơn hàng, ở đây chỉ minh họa
            return View(order);
        }

        #endregion
    }
}
