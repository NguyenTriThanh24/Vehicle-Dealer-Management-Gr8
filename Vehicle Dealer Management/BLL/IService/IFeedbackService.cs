using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.BLL.IService
{
    public interface IFeedbackService
    {
        Task<IEnumerable<Feedback>> GetFeedbacksByCustomerIdAsync(int customerId);
        Task<IEnumerable<Feedback>> GetFeedbacksByDealerIdAsync(int dealerId);
        Task<IEnumerable<Feedback>> GetFeedbacksByTypeAsync(string type, int? dealerId = null);
        Task<IEnumerable<Feedback>> GetFeedbacksByStatusAsync(string status, int? dealerId = null);
        Task<Feedback?> GetFeedbackByIdAsync(int id);
        Task<Feedback> CreateFeedbackAsync(Feedback feedback);
        Task UpdateFeedbackStatusAsync(int id, string status);
        Task<bool> FeedbackExistsAsync(int id);
    }
}

