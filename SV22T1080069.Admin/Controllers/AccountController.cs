using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1080069.BusinessLayers;

namespace SV22T1080069.Admin.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            ViewBag.UserName = username;
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("Error", "Nhập email và mật khẩu");
                return View();
            }

            //Kiểm tra thông tin đăng nhập
            var userAccount = await UserAccountService.EmployeeUserAccountDB.AuthenticateAsync(username, password);
            if (userAccount == null)
            {
                ModelState.AddModelError("Error", "Đăng nhập thất bại");
                return View();
            }

            //Tạo thông tin để ghi trong "giấy chứng nhận"
            WebUserData userData = new WebUserData()
            {
                UserId = userAccount.UserID,
                UserName = userAccount.UserName,
                DisplayName = userAccount.FullName,
                Email = userAccount.Email,
                Photo = userAccount.Photo,
                Roles = userAccount.RoleNames.Split(',').ToList()
            };

            //Thiết lập phiên đăng nhập 
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userData.CreatePrincipal());

            //Quay về trang chủ
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Đăng xuất
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
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
                return View();
            }

            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "Mật khẩu mới không khớp.";
                return View();
            }

            var userId = User.FindFirst("UserId")?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            var success = await UserAccountService.EmployeeUserAccountDB
                             .ChangePassword(userId, oldPassword, newPassword);

            if (!success)
            {
                TempData["Error"] = "Mật khẩu cũ không đúng.";
                return View();
            }

            // Đổi mật khẩu thành công → thông báo → quay về trang chủ
            TempData["Success"] = "Đổi mật khẩu thành công!";
            //return RedirectToAction("Index", "Home");
            return View();
        }



    }
}
