namespace Vehicle_Dealer_Management.BLL.Constants
{
    public static class RoleConstants
    {
        public const string CUSTOMER = "CUSTOMER";
        public const string DEALER_STAFF = "DEALER_STAFF";
        public const string DEALER_MANAGER = "DEALER_MANAGER";
        public const string EVM_STAFF = "EVM_STAFF";
        public const string EVM_ADMIN = "EVM_ADMIN";

        /// <summary>
        /// Returns the dashboard page path for a given role
        /// </summary>
        public static string GetDashboardPath(string roleCode)
        {
            return roleCode switch
            {
                CUSTOMER => "/Customer/Dashboard",
                DEALER_STAFF => "/Dealer/Dashboard",
                DEALER_MANAGER => "/DealerManager/Dashboard",
                EVM_STAFF => "/EVM/Dashboard",
                EVM_ADMIN => "/Admin/Dashboard",
                _ => "/Auth/Home"
            };
        }

        /// <summary>
        /// Check if a role is an admin role (EVM_ADMIN)
        /// </summary>
        public static bool IsAdmin(string roleCode)
        {
            return roleCode == EVM_ADMIN;
        }

        /// <summary>
        /// Check if a role is an EVM role (EVM_STAFF or EVM_ADMIN)
        /// </summary>
        public static bool IsEvmRole(string roleCode)
        {
            return roleCode == EVM_STAFF || roleCode == EVM_ADMIN;
        }

        /// <summary>
        /// Check if a role is a dealer role (DEALER_STAFF or DEALER_MANAGER)
        /// </summary>
        public static bool IsDealerRole(string roleCode)
        {
            return roleCode == DEALER_STAFF || roleCode == DEALER_MANAGER;
        }
    }
}

