using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace server.Models
{
    public class AppUser : IdentityUser
    {
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        public DateTime DateJoined { get; set; } = DateTime.Now;

        public DateTime LastLogin { get; set; } = DateTime.Now;

        [StringLength(20)]
        public string Currency { get; set; } = "USD";

        public bool EmailNotifications { get; set; } = true;

        // Navigation properties
        public List<Portfolio> Portfolios { get; set; } = new List<Portfolio>();
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
        public List<Watchlist> Watchlists { get; set; } = new List<Watchlist>();
        public List<Alert> Alerts { get; set; } = new List<Alert>();
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}
