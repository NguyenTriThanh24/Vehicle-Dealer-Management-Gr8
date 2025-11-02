using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Data;
using Vehicle_Dealer_Management.DAL.IRepository;
using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.DAL.Repositories
{
    public class DeliveryRepository : Repository<Delivery>, IDeliveryRepository
    {
        private readonly ApplicationDbContext _context;

        public DeliveryRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Delivery?> GetDeliveryBySalesDocumentIdAsync(int salesDocumentId)
        {
            return await _context.Deliveries
                .Include(d => d.SalesDocument)
                .FirstOrDefaultAsync(d => d.SalesDocumentId == salesDocumentId);
        }

        public async Task<IEnumerable<Delivery>> GetDeliveriesByStatusAsync(string status)
        {
            return await _context.Deliveries
                .Where(d => d.Status == status)
                .Include(d => d.SalesDocument)
                .OrderBy(d => d.ScheduledDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Delivery>> GetDeliveriesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Deliveries
                .Where(d => d.ScheduledDate >= startDate && d.ScheduledDate <= endDate)
                .Include(d => d.SalesDocument)
                .OrderBy(d => d.ScheduledDate)
                .ToListAsync();
        }
    }
}

