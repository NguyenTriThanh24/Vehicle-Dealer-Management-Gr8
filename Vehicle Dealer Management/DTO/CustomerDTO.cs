namespace Vehicle_Dealer_Management.DTO
{
    public class CustomerDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? IdCardNumber { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}

