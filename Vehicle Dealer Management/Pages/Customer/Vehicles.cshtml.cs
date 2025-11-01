using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Data;

namespace Vehicle_Dealer_Management.Pages.Customer
{
    public class VehiclesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public VehiclesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<VehicleViewModel> Vehicles { get; set; } = new();

        public async Task OnGetAsync()
        {
            var vehicles = await _context.Vehicles
                .Where(v => v.Status == "AVAILABLE")
                .ToListAsync();

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
                    Description = "Xe điện hiện đại, tiết kiệm năng lượng",
                    Price = price?.Msrp ?? 0
                });
            }
        }

        public class VehicleViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Variant { get; set; } = "";
            public string ImageUrl { get; set; } = "";
            public string Description { get; set; } = "";
            public decimal Price { get; set; }
        }
    }
}

