using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Data;
using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.Pages.Dealer
{
    public class CustomersModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CustomersModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int TotalCustomers { get; set; }
        public int NewCustomers { get; set; }
        public int WithAccount { get; set; }
        public int WithPurchase { get; set; }

        public List<CustomerViewModel> Customers { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var dealerId = HttpContext.Session.GetString("DealerId");
            if (string.IsNullOrEmpty(dealerId))
            {
                return RedirectToPage("/Login");
            }

            // Set UserRole from Session for proper navigation
            ViewData["UserRole"] = HttpContext.Session.GetString("UserRole") ?? "DEALER_STAFF";
            ViewData["UserName"] = HttpContext.Session.GetString("UserName") ?? "User";

            // Get all customers (for now show all, can be filtered by dealer later)
            var customers = await _context.CustomerProfiles
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            TotalCustomers = customers.Count;
            
            var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            NewCustomers = customers.Count(c => c.CreatedDate >= monthStart);
            WithAccount = customers.Count(c => c.UserId.HasValue);

            // Get order counts for each customer
            var dealerIdInt = int.Parse(dealerId);
            var orderCounts = await _context.SalesDocuments
                .Where(s => s.DealerId == dealerIdInt && s.Type == "ORDER")
                .GroupBy(s => s.CustomerId)
                .Select(g => new { CustomerId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CustomerId, x => x.Count);

            WithPurchase = orderCounts.Count;

            Customers = customers.Select(c => new CustomerViewModel
            {
                Id = c.Id,
                FullName = c.FullName,
                Phone = c.Phone,
                Email = c.Email,
                Address = c.Address,
                HasAccount = c.UserId.HasValue,
                OrderCount = orderCounts.ContainsKey(c.Id) ? orderCounts[c.Id] : 0,
                TotalSpent = 0 // Mock for now
            }).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string fullName, string phone, string email, string address, string? identityNo)
        {
            // Set UserRole from Session
            ViewData["UserRole"] = HttpContext.Session.GetString("UserRole") ?? "DEALER_STAFF";
            ViewData["UserName"] = HttpContext.Session.GetString("UserName") ?? "User";

            var customer = new CustomerProfile
            {
                FullName = fullName,
                Phone = phone,
                Email = email,
                Address = address ?? "",
                IdentityNo = identityNo,
                CreatedDate = DateTime.UtcNow
            };

            _context.CustomerProfiles.Add(customer);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public class CustomerViewModel
        {
            public int Id { get; set; }
            public string FullName { get; set; } = "";
            public string Phone { get; set; } = "";
            public string Email { get; set; } = "";
            public string Address { get; set; } = "";
            public bool HasAccount { get; set; }
            public int OrderCount { get; set; }
            public decimal TotalSpent { get; set; }
        }
    }
}

