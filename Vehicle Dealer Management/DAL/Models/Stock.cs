using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vehicle_Dealer_Management.DAL.Models
{
    public class Stock
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string OwnerType { get; set; } = string.Empty; // EVM hoặc DEALER

        [Required]
        public int OwnerId { get; set; } // ID của EVM (0) hoặc Dealer

        [Required]
        public int VehicleId { get; set; }

        [Required]
        [StringLength(20)]
        public string ColorCode { get; set; } = string.Empty; // Mã màu (ví dụ: "RED", "BLUE", "BLACK")

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Qty { get; set; } = 0; // Số lượng tồn kho (≥ 0)

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        [ForeignKey("VehicleId")]
        public virtual Vehicle? Vehicle { get; set; }
    }
}

