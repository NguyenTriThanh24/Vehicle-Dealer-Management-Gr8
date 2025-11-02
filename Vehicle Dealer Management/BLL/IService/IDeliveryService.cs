using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.BLL.IService
{
    public interface IDeliveryService
    {
        Task<Delivery?> GetDeliveryBySalesDocumentIdAsync(int salesDocumentId);
        Task<IEnumerable<Delivery>> GetDeliveriesByStatusAsync(string status);
        Task<IEnumerable<Delivery>> GetDeliveriesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Delivery> CreateOrUpdateDeliveryAsync(int salesDocumentId, DateTime scheduledDate, string? handoverNote = null);
        Task<Delivery> MarkDeliveryAsDeliveredAsync(int deliveryId, DateTime deliveredDate, string? handoverNote = null);
        Task<Delivery> UpdateDeliveryStatusAsync(int deliveryId, string status);
        Task<bool> DeliveryExistsAsync(int id);
    }
}

