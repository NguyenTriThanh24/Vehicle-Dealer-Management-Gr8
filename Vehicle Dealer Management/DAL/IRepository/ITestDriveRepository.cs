using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.DAL.IRepository
{
    public interface ITestDriveRepository : IRepository<TestDrive>
    {
        Task<IEnumerable<TestDrive>> GetTestDrivesByCustomerIdAsync(int customerId);
        Task<IEnumerable<TestDrive>> GetTestDrivesByDealerIdAsync(int dealerId);
        Task<IEnumerable<TestDrive>> GetTestDrivesByDealerAndDateAsync(int dealerId, DateTime date);
        Task<IEnumerable<TestDrive>> GetTestDrivesByStatusAsync(string status, int? dealerId = null);
    }
}

