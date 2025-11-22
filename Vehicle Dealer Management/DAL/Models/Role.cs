using System.ComponentModel.DataAnnotations;

namespace Vehicle_Dealer_Management.DAL.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty; // CUSTOMER, DEALER_STAFF, DEALER_MANAGER, EVM_STAFF, EVM_ADMIN

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool IsOperational { get; set; } = true;
    }
}

