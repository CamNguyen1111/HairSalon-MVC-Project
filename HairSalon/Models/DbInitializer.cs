using HairSalon.Models;

namespace HairSalon
{
    public static class DbInitializer
    {
        public static void Seed(HairSalonDbContext context)
        {
            context.Database.EnsureCreated();

            // 1. Khởi tạo Quyền (Roles) nếu chưa có
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role { RoleName = "Admin"},
                    new Role { RoleName = "Staff"},
                    new Role { RoleName = "Customer"}
                );
                context.SaveChanges();
            }

            // 2. Khởi tạo Tài khoản Admin mặc định nếu chưa có
            if (!context.Users.Any(u => u.Username == "admin"))
            {
                var adminRole = context.Roles.FirstOrDefault(r => r.RoleName == "Admin");
                if (adminRole != null)
                {
                    context.Users.Add(new User
                    {
                        Username = "admin",
                        PasswordHash = "123456", // Mật khẩu mặc định
                        FullName = "Quản Trị Viên",
                        Phone = "0900000000",
                        RoleId = adminRole.RoleId,
                        IsActive = true
                    });
                    context.SaveChanges();
                }
            }

            // 3. Khởi tạo Dịch vụ mẫu nếu bảng trống
            if (!context.Services.Any())
            {
                context.Services.AddRange(
                    new Service { ServiceName = "Cắt tóc nam cơ bản", Price = 80000, DurationMinutes = 30, Description = "Bao gồm xả tóc và tạo kiểu" },
                    new Service { ServiceName = "Uốn tóc bồng bềnh", Price = 300000, DurationMinutes = 60, Description = "Sử dụng thuốc uốn cao cấp" },
                    new Service { ServiceName = "Nhuộm màu thời trang", Price = 350000, DurationMinutes = 90, Description = "Tư vấn màu hợp tone da" }
                );
                context.SaveChanges();
            }
        }
    }
}