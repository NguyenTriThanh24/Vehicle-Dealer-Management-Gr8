using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Data;
using Vehicle_Dealer_Management.BLL.Constants;

namespace Vehicle_Dealer_Management.Pages.Auth
{
    public class HomeModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public HomeModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int TotalVehicles { get; set; }
        public int TotalDealers { get; set; }
        public int TotalCustomers { get; set; }
        public string? FeaturedVehicleImage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // If user is logged in, redirect to their dashboard
            var userId = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

                if (user != null)
                {
                    var dashboardPath = RoleConstants.GetDashboardPath(user.Role.Code);
                    return RedirectToPage(dashboardPath);
                }
            }

            // Get statistics for homepage (public) - enhanced calculations với số liệu cao hơn
            var availableVehicles = await _context.Vehicles.CountAsync(v => v.Status == "AVAILABLE");
            var allVehicles = await _context.Vehicles.CountAsync();
            // Nâng cao số liệu: tối thiểu 50+ hoặc gấp 5 lần số xe thực tế
            TotalVehicles = Math.Max(50, Math.Max(availableVehicles * 5, (int)(allVehicles * 5)));

            var activeDealers = await _context.Dealers.CountAsync(d => d.Status == "ACTIVE");
            var allDealers = await _context.Dealers.CountAsync();
            // Nâng cao số liệu: tối thiểu 30+ hoặc gấp 5 lần số đại lý thực tế
            TotalDealers = Math.Max(30, Math.Max(activeDealers * 5, (int)(allDealers * 5)));

            var customerProfiles = await _context.CustomerProfiles.CountAsync();
            var customers = await _context.Customers.CountAsync();
            var totalCustomersReal = customerProfiles + customers;
            // Nâng cao số liệu: tối thiểu 1000+ hoặc gấp 10 lần số khách hàng thực tế
            TotalCustomers = Math.Max(1000, totalCustomersReal * 10);

            // Get featured vehicle image
            var featuredVehicle = await _context.Vehicles
                .Where(v => v.Status == "AVAILABLE" && !string.IsNullOrEmpty(v.ImageUrl))
                .OrderByDescending(v => v.Id)
                .FirstOrDefaultAsync();
            
            FeaturedVehicleImage = featuredVehicle?.ImageUrl;

            return Page();
        }
    }
}

