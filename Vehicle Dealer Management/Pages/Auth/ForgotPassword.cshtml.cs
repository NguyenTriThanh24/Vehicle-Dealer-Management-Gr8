using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Data;
using System.Security.Cryptography;
using System.Text;

namespace Vehicle_Dealer_Management.Pages.Auth
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ForgotPasswordModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string? Email { get; set; }

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ResetToken { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Email))
            {
                ErrorMessage = "Vui lòng nhập email.";
                return Page();
            }

            // Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == Email);
            
            if (user == null)
            {
                ErrorMessage = "Email không tồn tại trong hệ thống.";
                ResetToken = null;
                return Page();
            }

            // Generate random code (6 digits) for display only
            var random = new Random();
            ResetToken = random.Next(100000, 999999).ToString();

            SuccessMessage = $"Mã đặt lại mật khẩu đã được tạo. Vui lòng copy mã bên dưới.";
            
            return Page();
        }

        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}

