using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.BLL.IService
{
    public interface IPricePolicyService
    {
        Task<PricePolicy?> GetPricePolicyByIdAsync(int id);
        Task<PricePolicy?> GetActivePricePolicyAsync(int vehicleId, int? dealerId = null);
        Task<IEnumerable<PricePolicy>> GetPricePoliciesByVehicleIdAsync(int vehicleId);
        Task<IEnumerable<PricePolicy>> GetActivePricePoliciesAsync(int? dealerId = null);
        Task<PricePolicy> CreatePricePolicyAsync(PricePolicy pricePolicy);
        Task UpdatePricePolicyAsync(PricePolicy pricePolicy);
        Task DeletePricePolicyAsync(int id);
        Task<bool> PricePolicyExistsAsync(int id);
    }
}

