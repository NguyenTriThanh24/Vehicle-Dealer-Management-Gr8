using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.DAL.IRepository
{
    public interface IFeedbackRepository : IRepository<Feedback>
    {
        Task<IEnumerable<Feedback>> GetFeedbacksByCustomerIdAsync(int customerId);
        Task<IEnumerable<Feedback>> GetFeedbacksByDealerIdAsync(int dealerId);
        Task<IEnumerable<Feedback>> GetFeedbacksByTypeAsync(string type, int? dealerId = null);
        Task<IEnumerable<Feedback>> GetFeedbacksByStatusAsync(string status, int? dealerId = null);
    }
}

