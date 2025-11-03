using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vehicle_Dealer_Management.DAL.Models
{
    public class Delivery
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SalesDocumentId { get; set; }

        [Required]
        public DateTime ScheduledDate { get; set; } // Ngày hẹn giao xe

        public DateTime? DeliveredDate { get; set; } // Ngày thực tế giao xe

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "SCHEDULED"; // SCHEDULED, IN_TRANSIT, DELIVERED, CANCELLED

        [StringLength(1000)]
        public string? HandoverNote { get; set; } // Ghi chú giao xe

        public bool CustomerConfirmed { get; set; } = false; // Khách hàng đã xác nhận nhận xe chưa

        public DateTime? CustomerConfirmedDate { get; set; } // Ngày khách hàng xác nhận nhận xe

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("SalesDocumentId")]
        public virtual SalesDocument? SalesDocument { get; set; }
    }
}

