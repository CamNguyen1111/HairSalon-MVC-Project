using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using HairSalon.Models;
using Microsoft.EntityFrameworkCore;

namespace HairSalon.Controllers
{
    public class AccountController : Controller
    {
        private readonly HairSalonDbContext _context;

        public AccountController(HairSalonDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Tìm user trong DB kèm theo Role
            var user = await _context.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == password);

            if (user != null)
            {
                // Tạo chứng minh thư (Claims) cho User
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Role, user.Role.RoleName) // Quyền: Admin, Staff, hoặc Customer
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Đăng nhập và lưu Cookie
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                // Điều hướng theo Role
                if (user.Role.RoleName == "Admin") return RedirectToAction("Index", "Appointments");
                if (user.Role.RoleName == "Staff") return RedirectToAction("Index", "Appointments"); // Tạm trỏ về lịch

                return RedirectToAction("Index", "Booking"); // Khách hàng
            }

            ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return Content("Bạn không có quyền truy cập trang này!");
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User model, string confirmPassword)
        {
            // Kiểm tra mật khẩu xác nhận
            if (model.PasswordHash != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu xác nhận không khớp.");
                return View(model);
            }

            // Kiểm tra trùng lặp Username hoặc Số điện thoại
            bool exists = await _context.Users.AnyAsync(u => u.Username == model.Username || u.Phone == model.Phone);
            if (exists)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc Số điện thoại đã được sử dụng.");
                return View(model);
            }

            // Gán quyền mặc định là Khách hàng (RoleId = 3)
            model.RoleId = 3;
            model.IsActive = true;

            _context.Users.Add(model);
            await _context.SaveChangesAsync();

            // Lưu thông báo thành công để hiển thị bên trang Login
            TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }
    }
}