using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vehicle_Dealer_Management.BLL.Constants;
using Vehicle_Dealer_Management.BLL.IService;
using Vehicle_Dealer_Management.DAL.Data;

namespace Vehicle_Dealer_Management.Pages.Admin
{
    public abstract class AdminPageModel : PageModel
    {
        protected readonly ApplicationDbContext _context;
        protected readonly IAuthorizationService _authorizationService;
        protected int CurrentUserId { get; private set; }
        protected string CurrentUserRole { get; private set; } = "";

        protected AdminPageModel(
            ApplicationDbContext context,
            IAuthorizationService authorizationService)
        {
            _context = context;
            _authorizationService = authorizationService;
        }

        /// <summary>
        /// Check if user is authenticated and has admin role
        /// Returns null if authorized, otherwise returns redirect result
        /// </summary>
        protected async Task<IActionResult?> CheckAuthorizationAsync()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToPage("/Auth/Login");
            }

            if (!int.TryParse(userIdString, out var userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            CurrentUserId = userId;

            var userRole = await _authorizationService.GetUserRoleAsync(userId);
            
            if (string.IsNullOrEmpty(userRole))
            {
                return RedirectToPage("/Auth/Login");
            }

            CurrentUserRole = userRole;

            // Check if user is admin
            if (!await _authorizationService.IsAdminAsync(userId))
            {
                // Redirect to their dashboard instead of admin pages
                var dashboardPath = RoleConstants.GetDashboardPath(userRole);
                return RedirectToPage(dashboardPath);
            }

            return null; // Authorized
        }

        /// <summary>
        /// Sets ViewData for user role and name for the layout
        /// </summary>
        protected void SetViewData()
        {
            ViewData["UserRole"] = CurrentUserRole;
            ViewData["UserName"] = HttpContext.Session.GetString("UserName") ?? "Admin";
        }
    }
}

