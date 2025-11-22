using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vehicle_Dealer_Management.DAL.Models
{
    public class TestDrive
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int DealerId { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [Required]
        public DateTime ScheduleTime { get; set; } // Thời gian hẹn lái thử (≥ now)

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "REQUESTED"; // REQUESTED, CONFIRMED, DONE, CANCELLED

        [StringLength(500)]
        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("CustomerId")]
        public virtual CustomerProfile? Customer { get; set; }

        [ForeignKey("DealerId")]
        public virtual Dealer? Dealer { get; set; }

        [ForeignKey("VehicleId")]
        public virtual Vehicle? Vehicle { get; set; }
    }
}

