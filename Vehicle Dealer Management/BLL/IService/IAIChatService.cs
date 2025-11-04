namespace Vehicle_Dealer_Management.BLL.IService
{
    public interface IAIChatService
    {
        Task<string> GetChatResponseAsync(string userMessage, int userId);
    }
}

