using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vehicle_Dealer_Management.DAL.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SalesDocumentId { get; set; }

        [Required]
        [StringLength(20)]
        public string Method { get; set; } = "CASH"; // CASH hoặc FINANCE

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; } = 0; // > 0

        [Column(TypeName = "nvarchar(max)")]
        public string? MetaJson { get; set; } // JSON chứa thông tin bổ sung: số thẻ, ngân hàng, v.v.

        [Required]
        public DateTime PaidAt { get; set; } = DateTime.UtcNow; // Thời điểm thanh toán

        // Navigation properties
        [ForeignKey("SalesDocumentId")]
        public virtual SalesDocument? SalesDocument { get; set; }
    }
}

