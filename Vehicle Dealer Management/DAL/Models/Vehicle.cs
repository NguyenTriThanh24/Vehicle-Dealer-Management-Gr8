using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vehicle_Dealer_Management.DAL.Models
{
    public class Vehicle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string ModelName { get; set; } = string.Empty; // Tên mẫu xe (ví dụ: "Model S", "Model 3")

        [Required]
        [StringLength(100)]
        public string VariantName { get; set; } = string.Empty; // Phiên bản (ví dụ: "Standard", "Premium", "Performance")

        [Column(TypeName = "nvarchar(max)")]
        public string? SpecJson { get; set; } // JSON chứa thông số kỹ thuật

        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty; // URL ảnh xe

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "AVAILABLE"; // AVAILABLE, DISCONTINUED, COMING_SOON

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        public virtual ICollection<Sale>? Sales { get; set; }
    }
}

