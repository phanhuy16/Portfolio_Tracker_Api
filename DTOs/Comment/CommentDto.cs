using System.ComponentModel.DataAnnotations;

namespace server.DTOs.Comment
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public int StockId { get; set; }
        public string Symbol { get; set; } = string.Empty;
    }

    public class CreateCommentDto
    {
        [Required]
        [StringLength(100)]
        [MinLength(5, ErrorMessage = "Title must be 5 characters")]
        [MaxLength(280, ErrorMessage = "Title cannot be over 280 characters")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        [MinLength(5, ErrorMessage = "Content must be 5 characters")]
        [MaxLength(500, ErrorMessage = "Content cannot be over 500 characters")]
        public string Content { get; set; } = string.Empty;
    }

    public class UpdateCommentDto
    {
        [Required]
        [StringLength(100)]
        [MinLength(5, ErrorMessage = "Title must be 5 characters")]
        [MaxLength(280, ErrorMessage = "Title cannot be over 280 characters")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        [MinLength(5, ErrorMessage = "Content must be 5 characters")]
        [MaxLength(500, ErrorMessage = "Content cannot be over 500 characters")]
        public string Content { get; set; } = string.Empty;
    }
}
