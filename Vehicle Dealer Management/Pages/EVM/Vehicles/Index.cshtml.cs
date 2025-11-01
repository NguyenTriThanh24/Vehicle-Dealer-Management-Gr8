using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Data;

namespace Vehicle_Dealer_Management.Pages.EVM.Vehicles
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<VehicleViewModel> Vehicles { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Login");
            }

            var vehicles = await _context.Vehicles.ToListAsync();

            foreach (var vehicle in vehicles)
            {
                var price = await _context.PricePolicies
                    .Where(p => p.VehicleId == vehicle.Id && p.DealerId == null)
                    .OrderByDescending(p => p.ValidFrom)
                    .FirstOrDefaultAsync();

                Vehicles.Add(new VehicleViewModel
                {
                    Id = vehicle.Id,
                    Name = vehicle.ModelName,
                    Variant = vehicle.VariantName,
                    ImageUrl = vehicle.ImageUrl,
                    Status = vehicle.Status,
                    Msrp = price?.Msrp ?? 0
                });
            }

            return Page();
        }

        public class VehicleViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Variant { get; set; } = "";
            public string ImageUrl { get; set; } = "";
            public string Status { get; set; } = "";
            public decimal Msrp { get; set; }
        }
    }
}

