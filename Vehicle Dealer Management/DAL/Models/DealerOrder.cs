using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vehicle_Dealer_Management.DAL.Models
{
    public class DealerOrder
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DealerId { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "DRAFT"; // DRAFT, SUBMITTED, APPROVED, REJECTED, FULFILLING, CLOSED

        [Column(TypeName = "nvarchar(max)")]
        public string ItemsJson { get; set; } = "[]"; // JSON array chứa các item: vehicle_id, color_code, qty

        [Required]
        public int CreatedBy { get; set; } // User ID (Dealer Staff)

        public int? ApprovedBy { get; set; } // User ID (EVM_STAFF hoặc EVM_ADMIN)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public DateTime? FulfilledAt { get; set; }

        // Navigation properties
        [ForeignKey("DealerId")]
        public virtual Dealer? Dealer { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual User? CreatedByUser { get; set; }

        [ForeignKey("ApprovedBy")]
        public virtual User? ApprovedByUser { get; set; }
    }
}

