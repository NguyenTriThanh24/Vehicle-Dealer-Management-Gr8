using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vehicle_Dealer_Management.BLL.IService;
using Vehicle_Dealer_Management.DAL.Data;

namespace Vehicle_Dealer_Management.Pages.Admin
{
    [IgnoreAntiforgeryToken]
    public class AIChatModel : AdminPageModel
    {
        private readonly IAIChatService _aiChatService;

        public AIChatModel(
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            IAIChatService aiChatService)
            : base(context, authorizationService)
        {
            _aiChatService = aiChatService;
        }

        public async Task<IActionResult> OnPostChatAsync([FromBody] ChatRequest request)
        {
            var authResult = await CheckAuthorizationAsync();
            if (authResult != null)
            {
                return new JsonResult(new { error = "Unauthorized" }) { StatusCode = 401 };
            }

            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return new JsonResult(new { error = "Message is required" }) { StatusCode = 400 };
            }

            try
            {
                var response = await _aiChatService.GetChatResponseAsync(request.Message, CurrentUserId);
                return new JsonResult(new { response = response });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { error = "An error occurred processing your request" }) { StatusCode = 500 };
            }
        }

        public class ChatRequest
        {
            public string Message { get; set; } = string.Empty;
        }
    }
}

