using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vehicle_Dealer_Management.DAL.Models
{
    public class SalesDocument
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Type { get; set; } = string.Empty; // QUOTE, ORDER, CONTRACT

        [Required]
        public int DealerId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty; // Xem state machine

        public int? PromotionId { get; set; }

        public DateTime? SignedAt { get; set; } // Ngày ký hợp đồng

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [Required]
        public int CreatedBy { get; set; } // User ID (Dealer Staff)

        // Navigation properties
        [ForeignKey("DealerId")]
        public virtual Dealer? Dealer { get; set; }

        [ForeignKey("CustomerId")]
        public virtual CustomerProfile? Customer { get; set; }

        [ForeignKey("PromotionId")]
        public virtual Promotion? Promotion { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual User? CreatedByUser { get; set; }

        public virtual ICollection<SalesDocumentLine>? Lines { get; set; }
        public virtual ICollection<Payment>? Payments { get; set; }
        public virtual Delivery? Delivery { get; set; }
    }
}

