using Vehicle_Dealer_Management.DAL.Models;
using Vehicle_Dealer_Management.DAL.IRepository;
using Vehicle_Dealer_Management.BLL.IService;

namespace Vehicle_Dealer_Management.BLL.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _feedbackRepository;

        public FeedbackService(IFeedbackRepository feedbackRepository)
        {
            _feedbackRepository = feedbackRepository;
        }

        public async Task<IEnumerable<Feedback>> GetFeedbacksByCustomerIdAsync(int customerId)
        {
            return await _feedbackRepository.GetFeedbacksByCustomerIdAsync(customerId);
        }

        public async Task<IEnumerable<Feedback>> GetFeedbacksByDealerIdAsync(int dealerId)
        {
            return await _feedbackRepository.GetFeedbacksByDealerIdAsync(dealerId);
        }

        public async Task<IEnumerable<Feedback>> GetFeedbacksByTypeAsync(string type, int? dealerId = null)
        {
            return await _feedbackRepository.GetFeedbacksByTypeAsync(type, dealerId);
        }

        public async Task<IEnumerable<Feedback>> GetFeedbacksByStatusAsync(string status, int? dealerId = null)
        {
            return await _feedbackRepository.GetFeedbacksByStatusAsync(status, dealerId);
        }

        public async Task<Feedback?> GetFeedbackByIdAsync(int id)
        {
            return await _feedbackRepository.GetByIdAsync(id);
        }

        public async Task<Feedback> CreateFeedbackAsync(Feedback feedback)
        {
            if (feedback == null)
            {
                throw new ArgumentNullException(nameof(feedback));
            }

            // Business logic: Validate feedback
            if (string.IsNullOrWhiteSpace(feedback.Content))
            {
                throw new ArgumentException("Feedback content is required", nameof(feedback));
            }

            if (string.IsNullOrWhiteSpace(feedback.Type))
            {
                feedback.Type = "FEEDBACK";
            }

            if (string.IsNullOrWhiteSpace(feedback.Status))
            {
                feedback.Status = "NEW";
            }

            feedback.CreatedAt = DateTime.UtcNow;

            return await _feedbackRepository.AddAsync(feedback);
        }

        public async Task UpdateFeedbackStatusAsync(int id, string status)
        {
            var feedback = await _feedbackRepository.GetByIdAsync(id);
            if (feedback == null)
            {
                throw new KeyNotFoundException($"Feedback with ID {id} not found");
            }

            feedback.Status = status;
            feedback.UpdatedAt = DateTime.UtcNow;

            if (status == "RESOLVED" && feedback.ResolvedAt == null)
            {
                feedback.ResolvedAt = DateTime.UtcNow;
            }

            await _feedbackRepository.UpdateAsync(feedback);
        }

        public async Task<bool> FeedbackExistsAsync(int id)
        {
            return await _feedbackRepository.ExistsAsync(id);
        }
    }
}

