using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.BLL.Constants;
using Vehicle_Dealer_Management.BLL.IService;
using Vehicle_Dealer_Management.DAL.Data;
using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.BLL.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly ApplicationDbContext _context;

        public AuthorizationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsAdminAsync(int userId)
        {
            return await HasRoleAsync(userId, RoleConstants.EVM_ADMIN);
        }

        public async Task<bool> HasRoleAsync(int userId, string roleCode)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user?.Role?.Code == roleCode;
        }

        public async Task<bool> HasAnyRoleAsync(int userId, params string[] roleCodes)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Role == null)
                return false;

            return roleCodes.Contains(user.Role.Code);
        }

        public async Task<string?> GetUserRoleAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user?.Role?.Code;
        }

        public async Task<bool> RequireAdminAsync(int userId)
        {
            var isAdmin = await IsAdminAsync(userId);
            if (!isAdmin)
            {
                throw new UnauthorizedAccessException("Yêu cầu quyền quản trị viên.");
            }
            return true;
        }
    }
}

