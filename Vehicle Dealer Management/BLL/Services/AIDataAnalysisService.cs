using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Data;
using System.Text.Json;

namespace Vehicle_Dealer_Management.BLL.Services
{
    public class AIDataAnalysisService
    {
        private readonly ApplicationDbContext _context;

        public AIDataAnalysisService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AnalysisData> GetAnalysisDataAsync()
        {
            var now = DateTime.UtcNow;
            var lastMonth = now.AddMonths(-1);
            var last3Months = now.AddMonths(-3);
            var last6Months = now.AddMonths(-6);

            // Sales data
            var salesThisMonth = await _context.SalesDocuments
                .Where(sd => sd.Type == "ORDER" && 
                            sd.CreatedAt >= new DateTime(now.Year, now.Month, 1) &&
                            sd.Status == "DELIVERED")
                .Include(sd => sd.Lines)
                .ThenInclude(l => l.Vehicle)
                .ToListAsync();

            var salesLastMonth = await _context.SalesDocuments
                .Where(sd => sd.Type == "ORDER" && 
                            sd.CreatedAt >= new DateTime(lastMonth.Year, lastMonth.Month, 1) &&
                            sd.CreatedAt < new DateTime(now.Year, now.Month, 1) &&
                            sd.Status == "DELIVERED")
                .Include(sd => sd.Lines)
                .ThenInclude(l => l.Vehicle)
                .ToListAsync();

            var salesLast3Months = await _context.SalesDocuments
                .Where(sd => sd.Type == "ORDER" && 
                            sd.CreatedAt >= last3Months &&
                            sd.Status == "DELIVERED")
                .Include(sd => sd.Lines)
                .ThenInclude(l => l.Vehicle)
                .Include(sd => sd.Dealer)
                .ToListAsync();

            // Inventory data
            var totalInventory = await _context.Stocks
                .Include(s => s.Vehicle)
                .Where(s => s.OwnerType == "EVM")
                .ToListAsync();

            var dealerInventory = await _context.Stocks
                .Include(s => s.Vehicle)
                .Include(s => s.Vehicle)
                .Where(s => s.OwnerType == "DEALER")
                .ToListAsync();

            // Dealer orders
            var dealerOrders = await _context.DealerOrders
                .Include(order => order.Dealer)
                .Where(order => order.Status == "SUBMITTED" || order.Status == "APPROVED")
                .ToListAsync();

            // Promotions
            var activePromotions = await _context.Promotions
                .Where(p => p.ValidFrom <= now && (p.ValidTo == null || p.ValidTo >= now))
                .ToListAsync();

            // Dealers
            var dealers = await _context.Dealers
                .Where(d => d.IsActive)
                .ToListAsync();

            // Vehicles
            var vehicles = await _context.Vehicles
                .Where(v => v.Status == "AVAILABLE")
                .ToListAsync();

            // Price policies
            var pricePolicies = await _context.PricePolicies
                .Include(pp => pp.Vehicle)
                .Where(pp => pp.ValidFrom <= now && (pp.ValidTo == null || pp.ValidTo >= now))
                .ToListAsync();

            return new AnalysisData
            {
                SalesThisMonth = salesThisMonth,
                SalesLastMonth = salesLastMonth,
                SalesLast3Months = salesLast3Months,
                TotalInventory = totalInventory,
                DealerInventory = dealerInventory,
                DealerOrders = dealerOrders,
                ActivePromotions = activePromotions,
                ActiveDealers = dealers,
                AvailableVehicles = vehicles,
                PricePolicies = pricePolicies,
                CurrentDate = now
            };
        }

        public string FormatAnalysisDataForAI(AnalysisData data)
        {
            var summary = new System.Text.StringBuilder();
            
            summary.AppendLine("=== DỮ LIỆU PHÂN TÍCH HIỆN TẠI ===\n");

            // Sales summary
            var thisMonthCount = data.SalesThisMonth.Count;
            var lastMonthCount = data.SalesLastMonth.Count;
            var salesChange = lastMonthCount > 0 
                ? ((thisMonthCount - lastMonthCount) * 100.0 / lastMonthCount).ToString("F1") 
                : "N/A";

            summary.AppendLine($"📊 BÁN HÀNG:");
            summary.AppendLine($"- Tháng này: {thisMonthCount} đơn hàng đã giao");
            summary.AppendLine($"- Tháng trước: {lastMonthCount} đơn hàng đã giao");
            summary.AppendLine($"- Thay đổi: {salesChange}%");
            
            // Sales by vehicle
            var vehicleSales = data.SalesLast3Months
                .SelectMany(sd => sd.Lines ?? new List<DAL.Models.SalesDocumentLine>())
                .GroupBy(l => l.VehicleId)
                .Select(g => new
                {
                    VehicleId = g.Key,
                    VehicleName = g.First().Vehicle?.ModelName ?? "N/A",
                    Count = g.Sum(l => l.Qty)
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            if (vehicleSales.Any())
            {
                summary.AppendLine($"\n- Top 5 xe bán chạy (3 tháng gần đây):");
                foreach (var vs in vehicleSales)
                {
                    summary.AppendLine($"  • {vs.VehicleName}: {vs.Count} xe");
                }
            }

            // Sales by dealer
            var dealerSales = data.SalesLast3Months
                .GroupBy(sd => sd.DealerId)
                .Select(g => new
                {
                    DealerId = g.Key,
                    DealerName = g.First().Dealer?.Name ?? "N/A",
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            if (dealerSales.Any())
            {
                summary.AppendLine($"\n- Top 5 đại lý bán chạy (3 tháng gần đây):");
                foreach (var ds in dealerSales)
                {
                    summary.AppendLine($"  • {ds.DealerName}: {ds.Count} đơn hàng");
                }
            }

            // Inventory
            summary.AppendLine($"\n📦 TỒN KHO:");
            var totalQty = data.TotalInventory.Sum(s => s.Qty);
            summary.AppendLine($"- Tổng kho EVM: {totalQty} xe");
            
            var lowStockVehicles = data.TotalInventory
                .Where(s => s.Qty < 5)
                .GroupBy(s => s.VehicleId)
                .Select(g => new
                {
                    VehicleName = g.First().Vehicle?.ModelName ?? "N/A",
                    TotalQty = g.Sum(s => s.Qty)
                })
                .ToList();

            if (lowStockVehicles.Any())
            {
                summary.AppendLine($"- ⚠️ Cảnh báo tồn kho thấp (< 5 xe):");
                foreach (var ls in lowStockVehicles)
                {
                    summary.AppendLine($"  • {ls.VehicleName}: {ls.TotalQty} xe");
                }
            }

            // Dealer orders
            summary.AppendLine($"\n📋 ĐƠN ĐẶT HÀNG CỦA ĐẠI LÝ:");
            var pendingOrders = data.DealerOrders.Count(order => order.Status == "SUBMITTED");
            var approvedOrders = data.DealerOrders.Count(order => order.Status == "APPROVED");
            summary.AppendLine($"- Chờ duyệt: {pendingOrders} đơn");
            summary.AppendLine($"- Đã duyệt: {approvedOrders} đơn");

            // Promotions
            summary.AppendLine($"\n🎁 KHUYẾN MÃI:");
            summary.AppendLine($"- Đang áp dụng: {data.ActivePromotions.Count} chương trình");

            // Dealers
            summary.AppendLine($"\n🏪 ĐẠI LÝ:");
            summary.AppendLine($"- Tổng số đại lý hoạt động: {data.ActiveDealers.Count}");

            // Vehicles
            summary.AppendLine($"\n🚗 SẢN PHẨM:");
            summary.AppendLine($"- Số mẫu xe có sẵn: {data.AvailableVehicles.Count}");

            return summary.ToString();
        }

        public class AnalysisData
        {
            public List<DAL.Models.SalesDocument> SalesThisMonth { get; set; } = new();
            public List<DAL.Models.SalesDocument> SalesLastMonth { get; set; } = new();
            public List<DAL.Models.SalesDocument> SalesLast3Months { get; set; } = new();
            public List<DAL.Models.Stock> TotalInventory { get; set; } = new();
            public List<DAL.Models.Stock> DealerInventory { get; set; } = new();
            public List<DAL.Models.DealerOrder> DealerOrders { get; set; } = new();
            public List<DAL.Models.Promotion> ActivePromotions { get; set; } = new();
            public List<DAL.Models.Dealer> ActiveDealers { get; set; } = new();
            public List<DAL.Models.Vehicle> AvailableVehicles { get; set; } = new();
            public List<DAL.Models.PricePolicy> PricePolicies { get; set; } = new();
            public DateTime CurrentDate { get; set; }
        }
    }
}

