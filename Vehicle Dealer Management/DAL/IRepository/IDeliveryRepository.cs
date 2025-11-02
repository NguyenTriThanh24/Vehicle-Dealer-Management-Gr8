using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.DAL.IRepository
{
    public interface IDeliveryRepository : IRepository<Delivery>
    {
        Task<Delivery?> GetDeliveryBySalesDocumentIdAsync(int salesDocumentId);
        Task<IEnumerable<Delivery>> GetDeliveriesByStatusAsync(string status);
        Task<IEnumerable<Delivery>> GetDeliveriesByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}

