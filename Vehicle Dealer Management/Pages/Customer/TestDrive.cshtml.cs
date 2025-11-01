using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Data;

namespace Vehicle_Dealer_Management.Pages.Customer
{
    public class TestDriveModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public TestDriveModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<TestDriveViewModel> TestDrives { get; set; } = new();
        public List<DealerSimple> AllDealers { get; set; } = new();
        public List<VehicleSimple> AllVehicles { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Login");
            }

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null)
            {
                return RedirectToPage("/Login");
            }

            // Get all dealers and vehicles for booking form
            AllDealers = await _context.Dealers
                .Where(d => d.Status == "ACTIVE")
                .Select(d => new DealerSimple
                {
                    Id = d.Id,
                    Name = d.Name,
                    Address = d.Address
                })
                .ToListAsync();

            AllVehicles = await _context.Vehicles
                .Where(v => v.Status == "AVAILABLE")
                .Select(v => new VehicleSimple
                {
                    Id = v.Id,
                    Name = v.ModelName + " " + v.VariantName
                })
                .ToListAsync();

            var customerProfile = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customerProfile != null)
            {
                var testDrives = await _context.TestDrives
                    .Where(t => t.CustomerId == customerProfile.Id)
                    .Include(t => t.Vehicle)
                    .Include(t => t.Dealer)
                    .OrderByDescending(t => t.ScheduleTime)
                    .ToListAsync();

                TestDrives = testDrives.Select(t => new TestDriveViewModel
                {
                    Id = t.Id,
                    VehicleName = $"{t.Vehicle?.ModelName} {t.Vehicle?.VariantName}",
                    DealerName = t.Dealer?.Name ?? "N/A",
                    DealerAddress = t.Dealer?.Address ?? "N/A",
                    ScheduleTime = t.ScheduleTime,
                    Status = t.Status,
                    Note = t.Note
                }).ToList();
            }

            return Page();
        }

        public class TestDriveViewModel
        {
            public int Id { get; set; }
            public string VehicleName { get; set; } = "";
            public string DealerName { get; set; } = "";
            public string DealerAddress { get; set; } = "";
            public DateTime ScheduleTime { get; set; }
            public string Status { get; set; } = "";
            public string? Note { get; set; }
        }

        public class DealerSimple
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Address { get; set; } = "";
        }

        public class VehicleSimple
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
        }
    }
}

