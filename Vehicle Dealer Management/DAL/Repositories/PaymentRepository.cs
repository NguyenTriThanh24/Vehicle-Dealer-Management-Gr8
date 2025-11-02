using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Data;
using Vehicle_Dealer_Management.DAL.IRepository;
using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.DAL.Repositories
{
    public class PaymentRepository : Repository<Payment>, IPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Payment>> GetPaymentsBySalesDocumentIdAsync(int salesDocumentId)
        {
            return await _context.Payments
                .Where(p => p.SalesDocumentId == salesDocumentId)
                .OrderByDescending(p => p.PaidAt)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalPaidAmountAsync(int salesDocumentId)
        {
            return await _context.Payments
                .Where(p => p.SalesDocumentId == salesDocumentId)
                .SumAsync(p => p.Amount);
        }
    }
}

