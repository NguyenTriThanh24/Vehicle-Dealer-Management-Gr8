using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Data;
using Vehicle_Dealer_Management.DAL.IRepository;
using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.DAL.Repositories
{
    public class PricePolicyRepository : Repository<PricePolicy>, IPricePolicyRepository
    {
        private readonly ApplicationDbContext _context;

        public PricePolicyRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PricePolicy?> GetActivePricePolicyAsync(int vehicleId, int? dealerId = null)
        {
            var query = _context.PricePolicies
                .Where(p => p.VehicleId == vehicleId &&
                           p.ValidFrom <= DateTime.UtcNow &&
                           (p.ValidTo == null || p.ValidTo >= DateTime.UtcNow));

            if (dealerId.HasValue)
            {
                // Prefer dealer-specific, fallback to global (null dealerId)
                query = query.Where(p => p.DealerId == dealerId || p.DealerId == null)
                    .OrderByDescending(p => p.DealerId); // Dealer-specific first
            }
            else
            {
                query = query.Where(p => p.DealerId == null)
                    .OrderByDescending(p => p.ValidFrom);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PricePolicy>> GetPricePoliciesByVehicleIdAsync(int vehicleId)
        {
            return await _context.PricePolicies
                .Where(p => p.VehicleId == vehicleId)
                .OrderByDescending(p => p.ValidFrom)
                .ToListAsync();
        }

        public async Task<IEnumerable<PricePolicy>> GetActivePricePoliciesAsync(int? dealerId = null)
        {
            var query = _context.PricePolicies
                .Where(p => p.ValidFrom <= DateTime.UtcNow &&
                           (p.ValidTo == null || p.ValidTo >= DateTime.UtcNow));

            if (dealerId.HasValue)
            {
                query = query.Where(p => p.DealerId == dealerId || p.DealerId == null);
            }
            else
            {
                query = query.Where(p => p.DealerId == null);
            }

            return await query.ToListAsync();
        }
    }
}

