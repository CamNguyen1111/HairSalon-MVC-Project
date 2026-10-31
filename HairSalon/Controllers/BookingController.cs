using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using HairSalon.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;


namespace HairSalon.Controllers
{
    [Authorize(Roles = "Customer")]
    public class BookingController : Controller
    {
        private readonly HairSalonDbContext _context;

        public BookingController(HairSalonDbContext context)
        {
            _context = context;
        }

        // Hiển thị Form đặt lịch
        [HttpGet]
        public IActionResult Index()
        {
            // Đổ danh sách Dịch vụ và Thợ (giả sử RoleId = 2 là Thợ) ra View
            ViewBag.Services = new SelectList(_context.Services, "ServiceId", "ServiceName");
            ViewBag.Staffs = new SelectList(_context.Users.Where(u => u.RoleId == 2), "UserId", "FullName");
            return View();
        }

        // Xử lý gửi Form đặt lịch
        [HttpPost]
        public IActionResult Index(Appointment model)
        {
            // NGHIỆP VỤ BẮT BUỘC TUẦN 5: Kiểm tra trùng lịch
            // Điều kiện: Cùng thợ, cùng ngày, cùng giờ và lịch đó chưa bị Hủy
            bool isConflict = _context.Appointments.Any(a =>
                a.StaffId == model.StaffId &&
                a.AppointmentDate == model.AppointmentDate &&
                a.StartTime == model.StartTime &&
                a.Status != "Canceled");

            if (isConflict)
            {
                // Trả về thông báo lỗi nếu trùng lịch
                ModelState.AddModelError("", "Thợ cắt tóc này đã có khách đặt vào thời gian bạn chọn. Vui lòng chọn giờ hoặc thợ khác!");

                // Load lại danh sách để user chọn lại
                ViewBag.Services = new SelectList(_context.Services, "ServiceId", "ServiceName");
                ViewBag.Staffs = new SelectList(_context.Users.Where(u => u.RoleId == 2), "UserId", "FullName");
                return View(model);
            }

            // Nếu hợp lệ, tiến hành lưu Booking
            model.Status = "Pending"; // Trạng thái mặc định là Chờ xác nhận
            model.CreatedAt = DateTime.Now;

            // Giả định Khách hàng có UserId = 3 đang đăng nhập (khi tích hợp Authentication ở Tuần 7 sẽ lấy từ hệ thống)
            // Đọc ID người dùng từ Cookie đăng nhập
            model.CustomerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            _context.Appointments.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            return View();
        }
        // Xem lịch sử cá nhân
        [Authorize]
        public IActionResult History()
        {
            // Lấy ID khách hàng đang đăng nhập
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var history = _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Staff) // Include thông tin thợ
                .Where(a => a.CustomerId == userId)
                .OrderByDescending(a => a.AppointmentDate).ThenByDescending(a => a.StartTime)
                .ToList();

            return View(history);
        }

        // Chức năng Hủy lịch
        [Authorize]
        [HttpPost]
        public IActionResult Cancel(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var appointment = _context.Appointments.FirstOrDefault(a => a.AppointmentId == id && a.CustomerId == userId);

            // Chỉ cho phép hủy nếu trạng thái đang là Pending (Mới tạo)
            if (appointment != null && appointment.Status == "Pending")
            {
                appointment.Status = "Canceled";
                _context.SaveChanges();
            }
            return RedirectToAction("History");
        }
    }
}