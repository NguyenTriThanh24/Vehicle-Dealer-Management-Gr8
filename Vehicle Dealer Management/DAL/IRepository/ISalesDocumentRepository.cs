using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.DAL.IRepository
{
    public interface ISalesDocumentRepository : IRepository<SalesDocument>
    {
        Task<SalesDocument?> GetSalesDocumentWithDetailsAsync(int id);
        Task<IEnumerable<SalesDocument>> GetSalesDocumentsByDealerIdAsync(int dealerId, string? type = null, string? status = null);
        Task<IEnumerable<SalesDocument>> GetSalesDocumentsByCustomerIdAsync(int customerId, string? type = null);
        Task<IEnumerable<SalesDocument>> GetSalesDocumentsByDateRangeAsync(DateTime startDate, DateTime endDate, string? type = null);
        Task<bool> HasSalesDocumentLinesAsync(int vehicleId);
    }
}

