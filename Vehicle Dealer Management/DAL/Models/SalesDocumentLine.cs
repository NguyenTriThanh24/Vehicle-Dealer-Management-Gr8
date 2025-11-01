using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vehicle_Dealer_Management.DAL.Models
{
    public class SalesDocumentLine
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SalesDocumentId { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [Required]
        [StringLength(20)]
        public string ColorCode { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Qty { get; set; } = 1; // > 0

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } = 0; // Giá đơn vị tại thời điểm bán (≥ 0)

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountValue { get; set; } = 0; // Tổng giá trị giảm giá cho dòng này (≥ 0)

        // Navigation properties
        [ForeignKey("SalesDocumentId")]
        public virtual SalesDocument? SalesDocument { get; set; }

        [ForeignKey("VehicleId")]
        public virtual Vehicle? Vehicle { get; set; }
    }
}

