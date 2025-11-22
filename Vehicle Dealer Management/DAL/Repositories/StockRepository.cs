using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Data;
using Vehicle_Dealer_Management.DAL.IRepository;
using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.DAL.Repositories
{
    public class StockRepository : Repository<Stock>, IStockRepository
    {
        private readonly ApplicationDbContext _context;

        public StockRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Stock>> GetStocksByOwnerAsync(string ownerType, int ownerId)
        {
            return await _context.Stocks
                .Where(s => s.OwnerType == ownerType && s.OwnerId == ownerId)
                .Include(s => s.Vehicle)
                .ToListAsync();
        }

        public async Task<IEnumerable<Stock>> GetStocksByVehicleIdAsync(int vehicleId)
        {
            return await _context.Stocks
                .Where(s => s.VehicleId == vehicleId)
                .Include(s => s.Vehicle)
                .ToListAsync();
        }

        public async Task<Stock?> GetStockByOwnerAndVehicleAsync(string ownerType, int ownerId, int vehicleId, string colorCode)
        {
            return await _context.Stocks
                .FirstOrDefaultAsync(s => s.OwnerType == ownerType &&
                                         s.OwnerId == ownerId &&
                                         s.VehicleId == vehicleId &&
                                         s.ColorCode == colorCode);
        }

        public async Task<IEnumerable<Stock>> GetAvailableStocksByVehicleIdAsync(int vehicleId, string ownerType)
        {
            return await _context.Stocks
                .Where(s => s.VehicleId == vehicleId &&
                           s.OwnerType == ownerType &&
                           s.Qty > 0)
                .Include(s => s.Vehicle)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalStockQtyAsync(int vehicleId, string ownerType)
        {
            return await _context.Stocks
                .Where(s => s.VehicleId == vehicleId && s.OwnerType == ownerType)
                .SumAsync(s => s.Qty);
        }
    }
}

