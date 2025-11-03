using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Data;
using Vehicle_Dealer_Management.DAL.Models;
using Vehicle_Dealer_Management.BLL.IService;

namespace Vehicle_Dealer_Management.Pages.Admin.Reports
{
    public class SalesByDealerModel : AdminPageModel
    {
        private readonly IDealerService _dealerService;

        public SalesByDealerModel(
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            IDealerService dealerService)
            : base(context, authorizationService)
        {
            _dealerService = dealerService;
        }

        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Region { get; set; }

        public List<DealerReportViewModel> DealerReports { get; set; } = new();
        public List<string> AllRegions { get; set; } = new() { "Tất cả", "Miền Bắc", "Miền Trung", "Miền Nam" };
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var authResult = await CheckAuthorizationAsync();
            if (authResult != null)
                return authResult;

            SetViewData();

            // Set default date range (current month if not specified)
            var now = DateTime.UtcNow;
            if (!FromDate.HasValue)
                FromDate = new DateTime(now.Year, now.Month, 1);
            if (!ToDate.HasValue)
                ToDate = now;

            // Get all dealers
            var allDealers = (await _dealerService.GetAllDealersAsync()).ToList();
            var dealerDict = allDealers.ToDictionary(d => d.Id, d => d);

            // Calculate date range for comparison (previous period)
            var periodDays = (ToDate.Value - FromDate.Value).Days + 1;
            var previousFromDate = FromDate.Value.AddDays(-periodDays);
            var previousToDate = FromDate.Value.AddDays(-1);

            // Get sales data for current period
            var currentPeriodOrders = await _context.SalesDocuments
                .Include(sd => sd.Dealer)
                .Include(sd => sd.Lines)
                .Where(sd => sd.Type == "ORDER" &&
                             (sd.Status == "PAID" || sd.Status == "DELIVERED") &&
                             sd.CreatedAt >= FromDate.Value &&
                             sd.CreatedAt <= ToDate.Value.AddDays(1).AddTicks(-1))
                .ToListAsync();

            // Get sales data for previous period (for growth calculation)
            var previousPeriodOrders = await _context.SalesDocuments
                .Include(sd => sd.Dealer)
                .Include(sd => sd.Lines)
                .Where(sd => sd.Type == "ORDER" &&
                             (sd.Status == "PAID" || sd.Status == "DELIVERED") &&
                             sd.CreatedAt >= previousFromDate &&
                             sd.CreatedAt <= previousToDate.AddDays(1).AddTicks(-1))
                .ToListAsync();

            // Group by dealer for current period
            var currentPeriodData = currentPeriodOrders
                .GroupBy(sd => sd.DealerId)
                .Select(g =>
                {
                    var firstOrder = g.First();
                    var dealer = dealerDict.ContainsKey(g.Key) ? dealerDict[g.Key] : null;
                    var sales = g.SelectMany(sd => sd.Lines ?? Enumerable.Empty<SalesDocumentLine>())
                        .Sum(line => (line.UnitPrice * line.Qty) - line.DiscountValue);
                    return new
                    {
                        DealerId = g.Key,
                        Dealer = dealer,
                        OrderCount = g.Count(),
                        Sales = sales
                    };
                })
                .ToList();

            // Group by dealer for previous period
            var previousPeriodData = previousPeriodOrders
                .GroupBy(sd => sd.DealerId)
                .ToDictionary(g => g.Key, g => g
                    .SelectMany(sd => sd.Lines ?? Enumerable.Empty<SalesDocumentLine>())
                    .Sum(line => (line.UnitPrice * line.Qty) - line.DiscountValue));

            // Build report data
            var dealerReportsList = new List<DealerReportViewModel>();
            
            foreach (var d in currentPeriodData)
            {
                var dealer = d.Dealer;
                if (dealer == null) continue;

                // Determine region from address
                var region = "N/A";
                if (dealer.Address != null)
                {
                    if (dealer.Address.Contains("Hà Nội") || dealer.Address.Contains("Hải Phòng") || dealer.Address.Contains("Bắc"))
                        region = "Miền Bắc";
                    else if (dealer.Address.Contains("Đà Nẵng") || dealer.Address.Contains("Huế") || dealer.Address.Contains("Trung"))
                        region = "Miền Trung";
                    else if (dealer.Address.Contains("TP.HCM") || dealer.Address.Contains("Cần Thơ") || dealer.Address.Contains("Nam"))
                        region = "Miền Nam";
                }

                // Calculate growth
                var previousSales = previousPeriodData.ContainsKey(d.DealerId) ? previousPeriodData[d.DealerId] : 0;
                var growth = previousSales > 0
                    ? (int)(((d.Sales - previousSales) / previousSales) * 100)
                    : (d.Sales > 0 ? 100 : 0);

                // Calculate target percent (mock - assume target is 20% higher than average)
                var dealersWithSales = currentPeriodData.Where(x => x.Sales > 0).ToList();
                int targetPercent = 0;
                if (dealersWithSales.Any())
                {
                    var avgSales = dealersWithSales.Average(x => (double)x.Sales);
                    var targetSales = (decimal)avgSales * 1.2m;
                    targetPercent = targetSales > 0 ? (int)((d.Sales / targetSales) * 100) : 0;
                }
                else
                {
                    // If no sales data, set to 0%
                    targetPercent = 0;
                }

                var report = new DealerReportViewModel
                {
                    Name = dealer.Name,
                    Region = region,
                    OrderCount = d.OrderCount,
                    Sales = d.Sales,
                    TargetPercent = targetPercent,
                    Growth = growth
                };

                // Filter by region if specified
                if (string.IsNullOrEmpty(Region) || Region == "Tất cả" || report.Region == Region)
                {
                    dealerReportsList.Add(report);
                }
            }

            DealerReports = dealerReportsList
                .OrderByDescending(d => d.Sales)
                .ToList();

            TotalSales = DealerReports.Sum(d => d.Sales);
            TotalOrders = DealerReports.Sum(d => d.OrderCount);

            return Page();
        }

        public class DealerReportViewModel
        {
            public string Name { get; set; } = "";
            public string Region { get; set; } = "";
            public int OrderCount { get; set; }
            public decimal Sales { get; set; }
            public int TargetPercent { get; set; }
            public int Growth { get; set; }
        }
    }
}

