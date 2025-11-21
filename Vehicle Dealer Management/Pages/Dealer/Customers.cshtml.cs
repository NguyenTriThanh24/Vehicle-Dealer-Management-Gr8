using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.BLL.IService;
using Vehicle_Dealer_Management.DAL.Data;
using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.Pages.Dealer
{
    public class CustomersModel : PageModel
    {
        private readonly ISalesDocumentService _salesDocumentService;
        private readonly ApplicationDbContext _context; // Cần cho CustomerProfile và complex queries

        public CustomersModel(
            ISalesDocumentService salesDocumentService,
            ApplicationDbContext context)
        {
            _salesDocumentService = salesDocumentService;
            _context = context;
        }

        public int TotalCustomers { get; set; }
        public int NewCustomers { get; set; }
        public int WithAccount { get; set; }
        public int WithPurchase { get; set; }

        public string? SearchQuery { get; set; }
        public string? FilterType { get; set; }

        public List<CustomerViewModel> Customers { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string? search, string? filter)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var dealerId = HttpContext.Session.GetString("DealerId");
            if (string.IsNullOrEmpty(dealerId))
            {
                return RedirectToPage("/Auth/Login");
            }

            // Set UserRole from Session for proper navigation
            ViewData["UserRole"] = HttpContext.Session.GetString("UserRole") ?? "DEALER_STAFF";
            ViewData["UserName"] = HttpContext.Session.GetString("UserName") ?? "User";

            // Store search and filter for UI
            SearchQuery = search;
            FilterType = filter;

            // Get all customers (for now show all, can be filtered by dealer later)
            var query = _context.CustomerProfiles.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(c => 
                    c.FullName.ToLower().Contains(searchLower) ||
                    (c.Email != null && c.Email.ToLower().Contains(searchLower)) ||
                    c.Phone.Contains(search)
                );
            }

            // Parse dealerId once at the beginning
            var dealerIdInt = int.Parse(dealerId);

            // Apply filter type
            if (!string.IsNullOrWhiteSpace(filter))
            {
                if (filter == "hasAccount")
                {
                    query = query.Where(c => c.UserId.HasValue);
                }
                else if (filter == "withPurchase")
                {
                    var orders = await _salesDocumentService.GetSalesDocumentsByDealerIdAsync(dealerIdInt, "ORDER", null);
                    var customerIdsWithOrders = orders.Select(o => o.CustomerId).Distinct().ToList();
                    query = query.Where(c => customerIdsWithOrders.Contains(c.Id));
                }
                else if (filter == "noPurchase")
                {
                    var orders = await _salesDocumentService.GetSalesDocumentsByDealerIdAsync(dealerIdInt, "ORDER", null);
                    var customerIdsWithOrders = orders.Select(o => o.CustomerId).Distinct().ToList();
                    query = query.Where(c => !customerIdsWithOrders.Contains(c.Id));
                }
            }

            // Get order counts for each customer using Service
            var allOrders = await _salesDocumentService.GetSalesDocumentsByDealerIdAsync(dealerIdInt, "ORDER", null);
            var orderCounts = allOrders
                .GroupBy(o => o.CustomerId)
                .ToDictionary(g => g.Key, g => g.Count());

            var customers = await query
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            // Calculate stats from ALL customers (not filtered)
            var allCustomers = await _context.CustomerProfiles.ToListAsync();
            TotalCustomers = allCustomers.Count;
            
            var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            NewCustomers = allCustomers.Count(c => c.CreatedDate >= monthStart);
            WithAccount = allCustomers.Count(c => c.UserId.HasValue);
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

            try
            {
                // Normalize email - convert empty string to null
                string? normalizedEmail = string.IsNullOrWhiteSpace(email) ? null : email.Trim();

                // Validate phone/email unique
                var existingPhone = await _context.CustomerProfiles
                    .FirstOrDefaultAsync(c => c.Phone == phone);
                if (existingPhone != null)
                {
                    TempData["Error"] = "Số điện thoại đã được sử dụng.";
                    return RedirectToPage();
                }

                if (!string.IsNullOrEmpty(normalizedEmail))
                {
                    var existingEmail = await _context.CustomerProfiles
                        .FirstOrDefaultAsync(c => c.Email != null && c.Email == normalizedEmail);
                    if (existingEmail != null)
                    {
                        TempData["Error"] = "Email đã được sử dụng.";
                        return RedirectToPage();
                    }
                }

                var customer = new CustomerProfile
                {
                    FullName = fullName?.Trim() ?? "",
                    Phone = phone?.Trim() ?? "",
                    Email = normalizedEmail,
                    Address = address?.Trim() ?? "",
                    IdentityNo = string.IsNullOrWhiteSpace(identityNo) ? null : identityNo.Trim(),
                    CreatedDate = DateTime.UtcNow
                };

                _context.CustomerProfiles.Add(customer);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thêm khách hàng thành công!";
            }
            catch (DbUpdateException ex)
            {
                // Handle unique constraint violations
                if (ex.InnerException?.Message.Contains("UNIQUE") == true || 
                    ex.InnerException?.Message.Contains("duplicate") == true)
                {
                    TempData["Error"] = "Số điện thoại hoặc email đã được sử dụng bởi khách hàng khác.";
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi lưu thông tin khách hàng. Vui lòng thử lại.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi thêm khách hàng. Vui lòng thử lại.";
                // Log exception for debugging
                System.Diagnostics.Debug.WriteLine($"Error creating customer: {ex.Message}");
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateCustomerAsync(int customerId, string fullName, string phone, string email, string address, string? identityNo)
        {
            // Set UserRole from Session
            ViewData["UserRole"] = HttpContext.Session.GetString("UserRole") ?? "DEALER_STAFF";
            ViewData["UserName"] = HttpContext.Session.GetString("UserName") ?? "User";

            var customer = await _context.CustomerProfiles.FindAsync(customerId);
            if (customer == null)
            {
                TempData["Error"] = "Không tìm thấy khách hàng này.";
                return RedirectToPage();
            }

            // Validate phone/email unique (excluding current customer)
            var existingPhone = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.Phone == phone && c.Id != customerId);
            if (existingPhone != null)
            {
                TempData["Error"] = "Số điện thoại đã được sử dụng bởi khách hàng khác.";
                return RedirectToPage();
            }

            var existingEmail = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => !string.IsNullOrEmpty(c.Email) && c.Email == email && c.Id != customerId);
            if (existingEmail != null)
            {
                TempData["Error"] = "Email đã được sử dụng bởi khách hàng khác.";
                return RedirectToPage();
            }

            // Update customer
            customer.FullName = fullName;
            customer.Phone = phone;
            customer.Email = email;
            customer.Address = address ?? "";
            customer.IdentityNo = identityNo;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật thông tin khách hàng thành công!";
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

