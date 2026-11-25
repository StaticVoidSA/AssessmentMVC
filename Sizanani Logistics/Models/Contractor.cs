using System.ComponentModel.DataAnnotations;

namespace Sizanani_Logistics.Models
{
    public class Contractor
    {
        public int ContractorId { get; set; }
        [Required]
        public string ContractorName { get; set; } = "";
        [Required]
        public string Email { get; set; } = "";
        [Required]
        public string PhoneNumber { get; set; } = "";
    }
}
