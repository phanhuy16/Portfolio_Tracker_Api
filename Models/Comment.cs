using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    [Table("Comments")]
    public class Comment
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int StockId { get; set; }
        public string UserId { get; set; } = string.Empty;

        public AppUser AppUser { get; set; } = null!;
        public Stock Stock { get; set; } = null!;
    }
}
