using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vehicle_Dealer_Management.DAL.Models
{
    public class Feedback
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int DealerId { get; set; }

        [Required]
        [StringLength(20)]
        public string Type { get; set; } = "FEEDBACK"; // FEEDBACK hoặc COMPLAINT

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "NEW"; // NEW, IN_PROGRESS, RESOLVED, REJECTED

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Content { get; set; } = string.Empty; // Nội dung phản hồi/khiếu nại

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }

        // Navigation properties
        [ForeignKey("CustomerId")]
        public virtual CustomerProfile? Customer { get; set; }

        [ForeignKey("DealerId")]
        public virtual Dealer? Dealer { get; set; }
    }
}

