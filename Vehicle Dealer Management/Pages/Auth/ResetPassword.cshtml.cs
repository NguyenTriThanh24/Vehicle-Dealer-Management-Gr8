using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Data;
using System.Security.Cryptography;
using System.Text;

namespace Vehicle_Dealer_Management.Pages.Auth
{
    public class ResetPasswordModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ResetPasswordModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string? Email { get; set; }

        [BindProperty]
        public string? Token { get; set; }

        [BindProperty]
        public string? Password { get; set; }

        [BindProperty]
        public string? ConfirmPassword { get; set; }

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public IActionResult OnGet(string? email, string? token)
        {
            Email = email;
            Token = token;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Email))
            {
                ErrorMessage = "Vui lòng nhập email.";
                return Page();
            }

            if (string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(ConfirmPassword))
            {
                ErrorMessage = "Vui lòng nhập mật khẩu và xác nhận mật khẩu.";
                return Page();
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Mật khẩu xác nhận không khớp.";
                return Page();
            }

            if (Password.Length < 6)
            {
                ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.";
                return Page();
            }

            // Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == Email);

            if (user == null)
            {
                ErrorMessage = "Email không tồn tại trong hệ thống.";
                return Page();
            }

            // Update password
            user.PasswordHash = HashPassword(Password);
            await _context.SaveChangesAsync();

            SuccessMessage = "Đặt lại mật khẩu thành công. Bạn có thể đăng nhập với mật khẩu mới.";
            Email = null;
            Token = null;
            Password = null;
            ConfirmPassword = null;

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

