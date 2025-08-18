using System.ComponentModel.DataAnnotations;

namespace server.DTOs.Auth
{
    public class AppUserDto
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string UserName { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        public DateTime DateJoined { get; set; } = DateTime.Now;

        public DateTime LastLogin { get; set; } = DateTime.Now;

        [StringLength(20)]
        public string Currency { get; set; } = "USD";

        public bool EmailNotifications { get; set; } = true;
    }
}
