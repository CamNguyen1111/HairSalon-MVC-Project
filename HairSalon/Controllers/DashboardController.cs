using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HairSalon.Models;

namespace HairSalon.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly HairSalonDbContext _context;

        public DashboardController(HairSalonDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // 1. Thống kê tổng quan
            ViewBag.TotalUsers = _context.Users.Count(u => u.RoleId == 3);
            ViewBag.TotalAppointments = _context.Appointments.Count();

            var completedAppointments = _context.Appointments
                .Include(a => a.Service)
                .Where(a => a.Status == "Completed")
                .ToList();

            ViewBag.TotalRevenue = completedAppointments.Sum(a => a.Service.Price);

            // 2. DỮ LIỆU BẢNG: Lấy 5 lịch hẹn mới nhất (thay vì chỉ hôm nay để luôn có dữ liệu)
            ViewBag.RecentAppointments = _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Service)
                .OrderByDescending(a => a.AppointmentDate)
                .Take(5)
                .ToList();

            // 3. DỮ LIỆU BIỂU ĐỒ: Thống kê doanh thu theo từng dịch vụ
            var chartData = completedAppointments
                .GroupBy(a => a.Service.ServiceName)
                .Select(g => new {
                    ServiceName = g.Key,
                    TotalRevenue = g.Sum(a => a.Service.Price)
                }).ToList();

            ViewBag.ChartLabels = chartData.Select(c => c.ServiceName).ToList();
            ViewBag.ChartValues = chartData.Select(c => c.TotalRevenue).ToList();

            return View();
        }
    }
}