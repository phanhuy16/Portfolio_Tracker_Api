using server.DTOs.Transaction;
using server.Models;

namespace server.Interfaces
{
    public interface ITransactionRepository
    {
        Task<List<Transaction>> GetUserTransactionsAsync(string userId);
        Task<List<Transaction>> GetUserTransactionsByStockAsync(string userId, int stockId);
        Task<Transaction?> GetByIdAsync(int id);
        Task<List<Transaction>> GetTransactionsByDateRangeAsync(string userId, DateTime startDate, DateTime endDate);
        Task<Transaction> CreateAsync(Transaction transaction);
        Task<Transaction?> UpdateAsync(int id, CreateTransactionDto transactionDto);
        Task<Transaction?> DeleteAsync(int id);
    }
}
