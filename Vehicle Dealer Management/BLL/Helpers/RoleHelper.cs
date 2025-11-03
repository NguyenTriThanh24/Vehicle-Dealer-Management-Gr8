using Vehicle_Dealer_Management.BLL.Constants;

namespace Vehicle_Dealer_Management.BLL.Helpers
{
    public static class RoleHelper
    {
        public static string GetDashboardPath(string? roleCode)
        {
            if (string.IsNullOrEmpty(roleCode))
                return "/Auth/Home";

            return RoleConstants.GetDashboardPath(roleCode);
        }
    }
}

