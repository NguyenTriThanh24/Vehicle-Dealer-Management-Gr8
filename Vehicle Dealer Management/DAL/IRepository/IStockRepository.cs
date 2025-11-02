using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.DAL.IRepository
{
    public interface IStockRepository : IRepository<Stock>
    {
        Task<IEnumerable<Stock>> GetStocksByOwnerAsync(string ownerType, int ownerId);
        Task<IEnumerable<Stock>> GetStocksByVehicleIdAsync(int vehicleId);
        Task<Stock?> GetStockByOwnerAndVehicleAsync(string ownerType, int ownerId, int vehicleId, string colorCode);
        Task<IEnumerable<Stock>> GetAvailableStocksByVehicleIdAsync(int vehicleId, string ownerType);
        Task<decimal> GetTotalStockQtyAsync(int vehicleId, string ownerType);
    }
}

