using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vehicle_Dealer_Management.DAL.Models
{
    public class Promotion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Scope { get; set; } = "GLOBAL"; // GLOBAL, DEALER, VEHICLE

        public int? DealerId { get; set; } // Required nếu scope = DEALER

        public int? VehicleId { get; set; } // Required nếu scope = VEHICLE

        [Column(TypeName = "nvarchar(max)")]
        public string RuleJson { get; set; } = "{}"; // JSON chứa quy tắc: giảm %, giảm số tiền, điều kiện

        [Required]
        public DateTime ValidFrom { get; set; }

        public DateTime? ValidTo { get; set; } // NULL nếu không có hạn

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("DealerId")]
        public virtual Dealer? Dealer { get; set; }

        [ForeignKey("VehicleId")]
        public virtual Vehicle? Vehicle { get; set; }
    }
}

