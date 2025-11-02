using Vehicle_Dealer_Management.DAL.Models;
using Vehicle_Dealer_Management.DAL.IRepository;
using Vehicle_Dealer_Management.BLL.IService;

namespace Vehicle_Dealer_Management.BLL.Services
{
    public class PricePolicyService : IPricePolicyService
    {
        private readonly IPricePolicyRepository _pricePolicyRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IDealerRepository _dealerRepository;

        public PricePolicyService(
            IPricePolicyRepository pricePolicyRepository,
            IVehicleRepository vehicleRepository,
            IDealerRepository dealerRepository)
        {
            _pricePolicyRepository = pricePolicyRepository;
            _vehicleRepository = vehicleRepository;
            _dealerRepository = dealerRepository;
        }

        public async Task<PricePolicy?> GetPricePolicyByIdAsync(int id)
        {
            return await _pricePolicyRepository.GetByIdAsync(id);
        }

        public async Task<PricePolicy?> GetActivePricePolicyAsync(int vehicleId, int? dealerId = null)
        {
            return await _pricePolicyRepository.GetActivePricePolicyAsync(vehicleId, dealerId);
        }

        public async Task<IEnumerable<PricePolicy>> GetPricePoliciesByVehicleIdAsync(int vehicleId)
        {
            return await _pricePolicyRepository.GetPricePoliciesByVehicleIdAsync(vehicleId);
        }

        public async Task<IEnumerable<PricePolicy>> GetActivePricePoliciesAsync(int? dealerId = null)
        {
            return await _pricePolicyRepository.GetActivePricePoliciesAsync(dealerId);
        }

        public async Task<PricePolicy> CreatePricePolicyAsync(PricePolicy pricePolicy)
        {
            if (pricePolicy == null)
            {
                throw new ArgumentNullException(nameof(pricePolicy));
            }

            // Validate vehicle exists
            var vehicle = await _vehicleRepository.GetByIdAsync(pricePolicy.VehicleId);
            if (vehicle == null)
            {
                throw new KeyNotFoundException($"Vehicle with ID {pricePolicy.VehicleId} not found");
            }

            // Validate dealer exists if dealerId is provided
            if (pricePolicy.DealerId.HasValue)
            {
                var dealer = await _dealerRepository.GetByIdAsync(pricePolicy.DealerId.Value);
                if (dealer == null)
                {
                    throw new KeyNotFoundException($"Dealer with ID {pricePolicy.DealerId.Value} not found");
                }
            }

            // Business logic: Validate dates
            if (pricePolicy.ValidFrom > pricePolicy.ValidTo && pricePolicy.ValidTo.HasValue)
            {
                throw new ArgumentException("ValidFrom cannot be after ValidTo", nameof(pricePolicy));
            }

            // Business logic: Validate prices
            if (pricePolicy.Msrp <= 0)
            {
                throw new ArgumentException("MSRP must be greater than 0", nameof(pricePolicy));
            }

            if (pricePolicy.WholesalePrice < 0 || pricePolicy.WholesalePrice > pricePolicy.Msrp)
            {
                throw new ArgumentException("Wholesale price must be between 0 and MSRP", nameof(pricePolicy));
            }

            return await _pricePolicyRepository.AddAsync(pricePolicy);
        }

        public async Task UpdatePricePolicyAsync(PricePolicy pricePolicy)
        {
            if (pricePolicy == null)
            {
                throw new ArgumentNullException(nameof(pricePolicy));
            }

            if (!await _pricePolicyRepository.ExistsAsync(pricePolicy.Id))
            {
                throw new KeyNotFoundException($"PricePolicy with ID {pricePolicy.Id} not found");
            }

            // Validate dates
            if (pricePolicy.ValidFrom > pricePolicy.ValidTo && pricePolicy.ValidTo.HasValue)
            {
                throw new ArgumentException("ValidFrom cannot be after ValidTo", nameof(pricePolicy));
            }

            await _pricePolicyRepository.UpdateAsync(pricePolicy);
        }

        public async Task DeletePricePolicyAsync(int id)
        {
            var pricePolicy = await _pricePolicyRepository.GetByIdAsync(id);
            if (pricePolicy == null)
            {
                throw new KeyNotFoundException($"PricePolicy with ID {id} not found");
            }

            await _pricePolicyRepository.DeleteAsync(id);
        }

        public async Task<bool> PricePolicyExistsAsync(int id)
        {
            return await _pricePolicyRepository.ExistsAsync(id);
        }
    }
}

