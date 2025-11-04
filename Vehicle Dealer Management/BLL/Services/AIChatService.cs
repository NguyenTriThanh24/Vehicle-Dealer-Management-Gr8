using Vehicle_Dealer_Management.BLL.IService;
using Vehicle_Dealer_Management.DAL.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Vehicle_Dealer_Management.BLL.Services
{
    public class AIChatService : IAIChatService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AIChatService> _logger;
        private readonly AIDataAnalysisService _dataAnalysisService;

        private const string SYSTEM_PROMPT = @"Bạn là AI Dự báo & Phân tích nhu cầu xe điện.

Nhiệm vụ của bạn:
- Phân tích dữ liệu bán hàng, đơn đặt hàng, tồn kho, và kế hoạch khuyến mãi để đưa ra dự báo nhu cầu cho từng mẫu xe, từng khu vực, từng đại lý.
- Đưa ra khuyến nghị phân phối xe phù hợp giữa các đại lý nhằm tối ưu hàng tồn và doanh số.
- Phát hiện bất thường trong dữ liệu bán hàng hoặc tồn kho (ví dụ: doanh số giảm đột ngột, tồn kho vượt ngưỡng, v.v.).
- Gợi ý điều chỉnh kế hoạch sản xuất theo xu hướng thị trường, mùa vụ hoặc phản hồi từ khách hàng.

Nếu người dùng hỏi bất kỳ nội dung nào ngoài phạm vi xe, đại lý, bán hàng, phân phối, sản xuất, khuyến mãi hoặc dữ liệu liên quan, bạn phải trả lời: ""Xin lỗi, tôi không thể trả lời câu hỏi này. Tôi chỉ được thiết kế để hỗ trợ dự báo nhu cầu và lập kế hoạch phân phối xe.""

Trả lời bằng tiếng Việt, ngắn gọn, dễ hiểu và tập trung vào dữ liệu thực tế.";

        public AIChatService(ApplicationDbContext context, ILogger<AIChatService> logger, AIDataAnalysisService dataAnalysisService)
        {
            _context = context;
            _logger = logger;
            _dataAnalysisService = dataAnalysisService;
        }

        public async Task<string> GetChatResponseAsync(string userMessage, int userId)
        {
            try
            {
                // Kiểm tra nội dung có ngoài phạm vi không
                if (!IsValidQuestion(userMessage))
                {
                    return "Xin lỗi, tôi không thể trả lời câu hỏi này. Tôi chỉ được thiết kế để hỗ trợ dự báo nhu cầu và lập kế hoạch phân phối xe.";
                }

                // Lấy thông tin user để có context
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return "Xin lỗi, không tìm thấy thông tin người dùng.";
                }

                // Lấy dữ liệu phân tích
                var analysisData = await _dataAnalysisService.GetAnalysisDataAsync();
                var dataSummary = _dataAnalysisService.FormatAnalysisDataForAI(analysisData);

                // Xử lý câu hỏi
                var response = await ProcessAIRequestAsync(userMessage, user, dataSummary, analysisData);
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing AI chat request");
                return "Xin lỗi, đã có lỗi xảy ra khi xử lý yêu cầu của bạn. Vui lòng thử lại sau.";
            }
        }

        private bool IsValidQuestion(string message)
        {
            var lowerMessage = message.ToLower();
            
            // Từ khóa liên quan đến hệ thống
            var validKeywords = new[]
            {
                "xe", "đại lý", "bán hàng", "doanh số", "tồn kho", "phân phối",
                "sản xuất", "khuyến mãi", "đơn hàng", "dự báo", "nhu cầu",
                "kế hoạch", "phân tích", "báo cáo", "thống kê", "xu hướng",
                "vehicle", "dealer", "sales", "inventory", "order", "forecast",
                "demand", "plan", "analysis", "report", "trend", "promotion",
                "chào", "hello", "hi", "giúp", "help", "xin chào"
            };

            // Kiểm tra nếu có từ khóa không liên quan (trò chơi, đời sống, v.v.)
            var invalidKeywords = new[]
            {
                "game", "trò chơi", "đời sống", "giải trí", "phim", "nhạc",
                "thể thao", "bóng đá", "cooking", "nấu ăn", "du lịch", "travel"
            };

            // Nếu có từ khóa không hợp lệ, từ chối
            if (invalidKeywords.Any(kw => lowerMessage.Contains(kw)))
            {
                return false;
            }

            // Nếu có ít nhất một từ khóa hợp lệ, chấp nhận
            return validKeywords.Any(kw => lowerMessage.Contains(kw));
        }

        private async Task<string> ProcessAIRequestAsync(
            string userMessage, 
            DAL.Models.User user, 
            string dataSummary, 
            AIDataAnalysisService.AnalysisData analysisData)
        {
            var lowerMessage = userMessage.ToLower();
            
            // Chào hỏi
            if (lowerMessage.Contains("xin chào") || lowerMessage.Contains("hello") || lowerMessage.Contains("hi") || lowerMessage.Contains("chào"))
            {
                return $"Xin chào {user.FullName}! 👋\n\nTôi là AI Dự báo & Phân tích nhu cầu xe. Tôi có thể giúp bạn:\n\n" +
                       "📊 Phân tích dữ liệu bán hàng và tồn kho\n" +
                       "🔮 Dự báo nhu cầu theo mẫu xe, khu vực, đại lý\n" +
                       "📦 Đề xuất phân phối xe tối ưu\n" +
                       "⚠️ Phát hiện bất thường trong dữ liệu\n" +
                       "📈 Gợi ý điều chỉnh kế hoạch sản xuất\n\n" +
                       "Bạn muốn tôi phân tích điều gì?";
            }

            // Help
            if (lowerMessage.Contains("giúp") || lowerMessage.Contains("help") || lowerMessage.Contains("có thể"))
            {
                return "Tôi có thể giúp bạn:\n\n" +
                       "1️⃣ **Dự báo nhu cầu**: Phân tích xu hướng bán hàng để dự báo nhu cầu\n" +
                       "2️⃣ **Phân tích tồn kho**: Kiểm tra tồn kho và đề xuất điều phối\n" +
                       "3️⃣ **Phân tích doanh số**: So sánh doanh số theo thời gian, đại lý, mẫu xe\n" +
                       "4️⃣ **Phát hiện bất thường**: Cảnh báo về tồn kho thấp, doanh số giảm\n" +
                       "5️⃣ **Khuyến nghị phân phối**: Đề xuất phân phối xe từ kho tổng đến đại lý\n\n" +
                       "Hãy hỏi tôi về bất kỳ chủ đề nào ở trên!";
            }

            // Tóm tắt dữ liệu
            if (lowerMessage.Contains("tóm tắt") || lowerMessage.Contains("tổng quan") || 
                lowerMessage.Contains("summary") || lowerMessage.Contains("overview") ||
                lowerMessage.Contains("dữ liệu hiện tại"))
            {
                return $"📊 **TÓM TẮT DỮ LIỆU HIỆN TẠI**\n\n{dataSummary}\n\n" +
                       "Bạn muốn tôi phân tích chi tiết về phần nào?";
            }

            // Phân tích doanh số
            if (lowerMessage.Contains("doanh số") || lowerMessage.Contains("bán hàng") || 
                lowerMessage.Contains("sales") || lowerMessage.Contains("xu hướng"))
            {
                return AnalyzeSalesTrends(analysisData);
            }

            // Phân tích tồn kho
            if (lowerMessage.Contains("tồn kho") || lowerMessage.Contains("inventory") || 
                lowerMessage.Contains("stock") || lowerMessage.Contains("kho"))
            {
                return AnalyzeInventory(analysisData);
            }

            // Dự báo nhu cầu
            if (lowerMessage.Contains("dự báo") || lowerMessage.Contains("forecast") || 
                lowerMessage.Contains("nhu cầu") || lowerMessage.Contains("demand"))
            {
                return ForecastDemand(analysisData);
            }

            // Phân phối
            if (lowerMessage.Contains("phân phối") || lowerMessage.Contains("distribution") || 
                lowerMessage.Contains("điều phối"))
            {
                return RecommendDistribution(analysisData);
            }

            // Phát hiện bất thường
            if (lowerMessage.Contains("bất thường") || lowerMessage.Contains("cảnh báo") || 
                lowerMessage.Contains("anomaly") || lowerMessage.Contains("warning"))
            {
                return DetectAnomalies(analysisData);
            }

            // Câu hỏi chung
            return GenerateGeneralResponse(userMessage, dataSummary, analysisData);
        }

        private string AnalyzeSalesTrends(AIDataAnalysisService.AnalysisData data)
        {
            var sb = new StringBuilder();
            sb.AppendLine("📊 **PHÂN TÍCH XU HƯỚNG BÁN HÀNG**\n");

            var thisMonthCount = data.SalesThisMonth.Count;
            var lastMonthCount = data.SalesLastMonth.Count;
            var change = lastMonthCount > 0 
                ? ((thisMonthCount - lastMonthCount) * 100.0 / lastMonthCount) 
                : 0;

            sb.AppendLine($"**Tháng này:** {thisMonthCount} đơn hàng đã giao");
            sb.AppendLine($"**Tháng trước:** {lastMonthCount} đơn hàng đã giao");
            
            if (change > 0)
                sb.AppendLine($"📈 **Tăng trưởng:** +{change:F1}%");
            else if (change < 0)
                sb.AppendLine($"📉 **Giảm:** {change:F1}%");
            else
                sb.AppendLine($"➡️ **Ổn định**");

            // Top vehicles
            var topVehicles = data.SalesLast3Months
                .SelectMany(sd => sd.Lines ?? new List<DAL.Models.SalesDocumentLine>())
                .GroupBy(l => l.VehicleId)
                .Select(g => new
                {
                    VehicleName = g.First().Vehicle?.ModelName ?? "N/A",
                    Count = g.Sum(l => l.Qty)
                })
                .OrderByDescending(x => x.Count)
                .Take(3)
                .ToList();

            if (topVehicles.Any())
            {
                sb.AppendLine("\n**Top 3 xe bán chạy (3 tháng gần đây):**");
                foreach (var v in topVehicles)
                {
                    sb.AppendLine($"🥇 {v.VehicleName}: {v.Count} xe");
                }
            }

            return sb.ToString();
        }

        private string AnalyzeInventory(AIDataAnalysisService.AnalysisData data)
        {
            var sb = new StringBuilder();
            sb.AppendLine("📦 **PHÂN TÍCH TỒN KHO**\n");

            var totalQty = data.TotalInventory.Sum(s => s.Qty);
            sb.AppendLine($"**Tổng tồn kho EVM:** {totalQty} xe");

            var lowStock = data.TotalInventory
                .Where(s => s.Qty < 5)
                .GroupBy(s => s.VehicleId)
                .Select(g => new
                {
                    VehicleName = g.First().Vehicle?.ModelName ?? "N/A",
                    TotalQty = g.Sum(s => s.Qty)
                })
                .ToList();

            if (lowStock.Any())
            {
                sb.AppendLine("\n⚠️ **CẢNH BÁO: Tồn kho thấp (< 5 xe):**");
                foreach (var ls in lowStock)
                {
                    sb.AppendLine($"• {ls.VehicleName}: {ls.TotalQty} xe");
                }
                sb.AppendLine("\n💡 **Khuyến nghị:** Cần bổ sung tồn kho hoặc điều phối từ đại lý khác.");
            }
            else
            {
                sb.AppendLine("\n✅ Tồn kho đang ở mức ổn định.");
            }

            return sb.ToString();
        }

        private string ForecastDemand(AIDataAnalysisService.AnalysisData data)
        {
            var sb = new StringBuilder();
            sb.AppendLine("🔮 **DỰ BÁO NHU CẦU**\n");

            // Tính toán trung bình 3 tháng gần đây
            var avgMonthlySales = data.SalesLast3Months.Count / 3.0;
            var projectedNextMonth = (int)(avgMonthlySales * 1.1); // Dự báo tăng 10%

            sb.AppendLine($"**Trung bình bán hàng/tháng (3 tháng gần đây):** {avgMonthlySales:F1} đơn");
            sb.AppendLine($"**Dự báo tháng tới:** {projectedNextMonth} đơn (+10% dự phòng)");

            // Phân tích theo vehicle
            var vehicleDemand = data.SalesLast3Months
                .SelectMany(sd => sd.Lines ?? new List<DAL.Models.SalesDocumentLine>())
                .GroupBy(l => l.VehicleId)
                .Select(g => new
                {
                    VehicleName = g.First().Vehicle?.ModelName ?? "N/A",
                    MonthlyAvg = g.Sum(l => l.Qty) / 3m
                })
                .OrderByDescending(x => x.MonthlyAvg)
                .Take(3)
                .ToList();

            if (vehicleDemand.Any())
            {
                sb.AppendLine("\n**Top 3 mẫu xe có nhu cầu cao:**");
                foreach (var vd in vehicleDemand)
                {
                    var forecast = (int)(vd.MonthlyAvg * 1.1m);
                    sb.AppendLine($"• {vd.VehicleName}: Dự báo {forecast} xe/tháng");
                }
            }

            return sb.ToString();
        }

        private string RecommendDistribution(AIDataAnalysisService.AnalysisData data)
        {
            var sb = new StringBuilder();
            sb.AppendLine("📋 **KHUYẾN NGHỊ PHÂN PHỐI**\n");

            var pendingOrders = data.DealerOrders.Where(order => order.Status == "SUBMITTED").ToList();
            
            if (pendingOrders.Any())
            {
                sb.AppendLine($"**Có {pendingOrders.Count} đơn đặt hàng đang chờ duyệt:**");
                foreach (var order in pendingOrders.Take(3))
                {
                    sb.AppendLine($"• Đại lý: {order.Dealer?.Name ?? "N/A"} - Trạng thái: Chờ duyệt");
                }
                sb.AppendLine("\n💡 **Khuyến nghị:** Xem xét và duyệt các đơn đặt hàng này để đảm bảo phân phối kịp thời.");
            }

            // Kiểm tra tồn kho vs nhu cầu
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
                sb.AppendLine("\n⚠️ **Cần điều phối từ kho EVM:**");
                foreach (var ls in lowStockVehicles)
                {
                    sb.AppendLine($"• {ls.VehicleName}: Tồn kho thấp ({ls.TotalQty} xe)");
                }
            }

            return sb.ToString();
        }

        private string DetectAnomalies(AIDataAnalysisService.AnalysisData data)
        {
            var sb = new StringBuilder();
            sb.AppendLine("⚠️ **PHÁT HIỆN BẤT THƯỜNG**\n");

            var anomalies = new List<string>();

            // Kiểm tra tồn kho thấp
            var lowStockCount = data.TotalInventory.Count(s => s.Qty < 5);
            if (lowStockCount > 0)
            {
                anomalies.Add($"📦 Tồn kho thấp: {lowStockCount} mẫu xe có tồn kho < 5 xe");
            }

            // Kiểm tra doanh số giảm
            var thisMonthCount = data.SalesThisMonth.Count;
            var lastMonthCount = data.SalesLastMonth.Count;
            if (lastMonthCount > 0 && thisMonthCount < lastMonthCount * 0.8)
            {
                var decrease = ((lastMonthCount - thisMonthCount) * 100.0 / lastMonthCount);
                anomalies.Add($"📉 Doanh số giảm đột ngột: {decrease:F1}% so với tháng trước");
            }

            // Kiểm tra đơn đặt hàng chờ duyệt lâu
            var oldPendingOrders = data.DealerOrders
                .Where(order => order.Status == "SUBMITTED" && 
                           (DateTime.UtcNow - order.CreatedAt).Days > 7)
                .Count();
            if (oldPendingOrders > 0)
            {
                anomalies.Add($"⏰ Có {oldPendingOrders} đơn đặt hàng chờ duyệt > 7 ngày");
            }

            if (anomalies.Any())
            {
                foreach (var anomaly in anomalies)
                {
                    sb.AppendLine($"• {anomaly}");
                }
            }
            else
            {
                sb.AppendLine("✅ Không phát hiện bất thường. Hệ thống đang hoạt động bình thường.");
            }

            return sb.ToString();
        }

        private string GenerateGeneralResponse(
            string userMessage, 
            string dataSummary, 
            AIDataAnalysisService.AnalysisData data)
        {
            // Response thông minh dựa trên context
            var response = $"Tôi hiểu bạn đang hỏi về: \"{userMessage}\"\n\n";
            
            // Thêm dữ liệu liên quan nếu có
            if (userMessage.ToLower().Contains("xe") || userMessage.ToLower().Contains("vehicle"))
            {
                var vehicleCount = data.AvailableVehicles.Count;
                response += $"Hiện tại hệ thống có {vehicleCount} mẫu xe có sẵn.\n\n";
            }

            if (userMessage.ToLower().Contains("đại lý") || userMessage.ToLower().Contains("dealer"))
            {
                var dealerCount = data.ActiveDealers.Count;
                response += $"Có {dealerCount} đại lý đang hoạt động.\n\n";
            }

            response += "Bạn có thể hỏi tôi cụ thể hơn về:\n";
            response += "• Dự báo nhu cầu\n";
            response += "• Phân tích tồn kho\n";
            response += "• Xu hướng bán hàng\n";
            response += "• Khuyến nghị phân phối\n";
            response += "• Phát hiện bất thường\n\n";
            response += "Hoặc gõ \"tóm tắt\" để xem tổng quan dữ liệu hiện tại.";

            return response;
        }
    }
}

