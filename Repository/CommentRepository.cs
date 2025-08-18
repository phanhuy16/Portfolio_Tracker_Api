using Microsoft.EntityFrameworkCore;
using server.Data;
using server.DTOs.Comment;
using server.Interfaces;
using server.Models;

namespace server.Repository
{
    public class CommentRepository : ICommentRepository
    {
        private readonly ApplicationDbContext _context;
        public CommentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Comment> CreateAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<Comment?> DeleteAsync(int id)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(x => x.Id == id);
            if (comment == null)
            {
                return null!;
            }
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<List<Comment>> GetAllAsync()
        {
            try
            {
                return await _context.Comments
                    .Include(x => x.AppUser)
                    .Include(x => x.Stock)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // Log the exception (ex) here if needed
                throw new InvalidOperationException("An error occurred while retrieving comments.", ex);
            }
        }

        public async Task<Comment?> GetByIdAsync(int id)
        {
            try
            {
                var comment = await _context.Comments
                        .Include(x => x.AppUser)
                        .Include(x => x.Stock)
                        .FirstOrDefaultAsync(x => x.Id == id);
                return comment;
            }
            catch (Exception ex)
            {
                // Log the exception (ex) here if needed
                throw new InvalidOperationException($"An error occurred while retrieving the comment with ID {id}.", ex);
            }
        }

        public Task<List<Comment>> GetCommentsByStockIdAsync(int stockId)
        {
            try
            {
                return _context.Comments
                    .Where(c => c.StockId == stockId)
                    .Include(c => c.AppUser)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // Log the exception (ex) here if needed
                throw new InvalidOperationException($"An error occurred while retrieving comments for stock ID {stockId}.", ex);
            }
        }

        public async Task<Comment?> UpdateAsync(int id, UpdateCommentDto commentDto)
        {
            var existingComment = _context.Comments.FirstOrDefault(x => x.Id == id);
            if (existingComment == null) return null!;

            existingComment.Title = commentDto.Title;
            existingComment.Content = commentDto.Content;

            await _context.SaveChangesAsync();
            return existingComment;
        }
    }
}
