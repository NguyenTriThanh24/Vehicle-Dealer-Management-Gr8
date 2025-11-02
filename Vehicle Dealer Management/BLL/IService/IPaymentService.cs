using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.BLL.IService
{
    public interface IPaymentService
    {
        Task<IEnumerable<Payment>> GetPaymentsBySalesDocumentIdAsync(int salesDocumentId);
        Task<decimal> GetTotalPaidAmountAsync(int salesDocumentId);
        Task<Payment> CreatePaymentAsync(int salesDocumentId, string method, decimal amount, string? metaJson = null);
        Task<bool> PaymentExistsAsync(int id);
    }
}

