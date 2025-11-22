using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vehicle_Dealer_Management.DAL.Models
{
    public class CustomerProfile
    {
        [Key]
        public int Id { get; set; }

        public int? UserId { get; set; } // Link với account nếu khách đăng ký

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty; // UNIQUE

        [StringLength(100)]
        public string? Email { get; set; } // UNIQUE

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(20)]
        public string? IdentityNo { get; set; } // Số CMND/CCCD

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}

