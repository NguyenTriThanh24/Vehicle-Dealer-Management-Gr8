using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Data;
using Vehicle_Dealer_Management.DAL.Models;
using Vehicle_Dealer_Management.DAL.IRepository;

namespace Vehicle_Dealer_Management.DAL.Repositories
{
    public class VehicleRepository : Repository<Vehicle>, IVehicleRepository
    {
        public VehicleRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync()
        {
            return await _dbSet
                .Where(v => v.Status == "AVAILABLE")
                .ToListAsync();
        }

        public async Task<IEnumerable<Vehicle>> SearchVehiclesAsync(string searchTerm)
        {
            return await _dbSet
                .Where(v => v.ModelName.Contains(searchTerm) ||
                           v.VariantName.Contains(searchTerm))
                .ToListAsync();
        }

        public async Task<Vehicle?> GetVehicleWithSalesAsync(int id)
        {
            return await _dbSet
                .Include(v => v.Sales)
                .FirstOrDefaultAsync(v => v.Id == id);
        }
    }
}

