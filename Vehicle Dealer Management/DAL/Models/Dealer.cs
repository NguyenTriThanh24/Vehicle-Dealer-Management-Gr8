using System.ComponentModel.DataAnnotations;

namespace Vehicle_Dealer_Management.DAL.Models
{
    public class Dealer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty; // Mã đại lý (UNIQUE)

        [StringLength(50)]
        public string? TaxCode { get; set; } // Mã số thuế

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "ACTIVE"; // ACTIVE, INACTIVE, SUSPENDED

        public bool IsActive { get; set; } = true; // Giữ lại để tương thích

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }
    }
}

