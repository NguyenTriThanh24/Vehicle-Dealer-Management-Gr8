using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.DAL.IRepository
{
    public interface IPricePolicyRepository : IRepository<PricePolicy>
    {
        Task<PricePolicy?> GetActivePricePolicyAsync(int vehicleId, int? dealerId = null);
        Task<IEnumerable<PricePolicy>> GetPricePoliciesByVehicleIdAsync(int vehicleId);
        Task<IEnumerable<PricePolicy>> GetActivePricePoliciesAsync(int? dealerId = null);
    }
}

