using server.DTOs.Comment;
using server.Models;

namespace server.Interfaces
{
    public interface ICommentRepository
    {
        Task<List<Comment>> GetAllAsync();
        Task<Comment?> GetByIdAsync(int id);
        Task<List<Comment>> GetCommentsByStockIdAsync(int stockId);
        Task<Comment> CreateAsync(Comment comment);
        Task<Comment?> UpdateAsync(int id, UpdateCommentDto commentDto);
        Task<Comment?> DeleteAsync(int id);
    }
}
