namespace Vehicle_Dealer_Management.DTO
{
    public class SaleDTO
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int VehicleId { get; set; }
        public decimal SalePrice { get; set; }
        public DateTime SaleDate { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        
        // Related entities
        public string? CustomerName { get; set; }
        public string? VehicleName { get; set; }
    }
}

