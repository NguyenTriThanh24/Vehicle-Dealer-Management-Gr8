namespace Vehicle_Dealer_Management.DTO
{
    public class VehicleDTO
    {
        public int Id { get; set; }
        public string ModelName { get; set; } = "";
        public string VariantName { get; set; } = "";
        public string? SpecJson { get; set; }
        public string? ImageUrl { get; set; }
        public string Status { get; set; } = "";
        public decimal? Msrp { get; set; }
        public decimal? WholesalePrice { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}

