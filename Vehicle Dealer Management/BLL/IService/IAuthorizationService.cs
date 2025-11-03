namespace Vehicle_Dealer_Management.BLL.IService
{
    public interface IAuthorizationService
    {
        Task<bool> IsAdminAsync(int userId);
        Task<bool> HasRoleAsync(int userId, string roleCode);
        Task<bool> HasAnyRoleAsync(int userId, params string[] roleCodes);
        Task<string?> GetUserRoleAsync(int userId);
        Task<bool> RequireAdminAsync(int userId);
    }
}

