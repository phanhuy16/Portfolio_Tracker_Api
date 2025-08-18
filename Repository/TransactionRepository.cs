using Microsoft.EntityFrameworkCore;
using server.Data;
using server.DTOs.Transaction;
using server.Interfaces;
using server.Models;

namespace server.Repository
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public TransactionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Transaction> CreateAsync(Transaction transaction)
        {
            try
            {
                await _context.AddAsync(transaction);
                await _context.SaveChangesAsync();
                return transaction;
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                throw new Exception("An error occurred while creating the transaction.", ex);
            }
        }

        public async Task<Transaction?> DeleteAsync(int id)
        {
            try
            {
                var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.Id == id);

                if (transaction == null) return null!;

                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();

                return transaction;
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                throw new Exception("An error occurred while deleting the transaction.", ex);
            }
        }

        public async Task<Transaction?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Transactions
                  .Include(t => t.Stock)
                  .Include(t => t.AppUser)
                  .FirstOrDefaultAsync(t => t.Id == id);
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                throw new Exception("An error occurred while retrieving the transaction.", ex);
            }
        }

        public async Task<List<Transaction>> GetTransactionsByDateRangeAsync(string userId, DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _context.Transactions
                   .Where(t => t.UserId == userId && t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                   .Include(t => t.Stock)
                   .OrderByDescending(t => t.TransactionDate)
                   .ToListAsync();
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                throw new Exception("An error occurred while retrieving transactions by date range.", ex);
            }
        }

        public async Task<List<Transaction>> GetUserTransactionsAsync(string userId)
        {
            try
            {
                return await _context.Transactions
                   .Where(t => t.UserId == userId)
                   .Include(t => t.Stock)
                   .OrderByDescending(t => t.TransactionDate)
                   .ToListAsync();
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                throw new Exception("An error occurred while retrieving user transactions.", ex);
            }
        }

        public async Task<List<Transaction>> GetUserTransactionsByStockAsync(string userId, int stockId)
        {
            try
            {
                return await _context.Transactions
                    .Where(t => t.UserId == userId && t.StockId == stockId)
                    .Include(t => t.Stock)
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                throw new Exception("An error occurred while retrieving user transactions by stock.", ex);
            }
        }

        public async Task<Transaction?> UpdateAsync(int id, CreateTransactionDto transactionDto)
        {
            try
            {
                var existingTransaction = await _context.Transactions.FirstOrDefaultAsync(t => t.Id == id);

                if (existingTransaction == null)
                {
                    return null; // Transaction not found
                }

                // Update properties from DTO
                existingTransaction.StockId = transactionDto.StockId;
                existingTransaction.Quantity = transactionDto.Quantity;
                existingTransaction.Price = transactionDto.Price;
                existingTransaction.Commission = transactionDto.Commission;
                existingTransaction.Notes = transactionDto.Notes;
                existingTransaction.TransactionType = (TransactionType)transactionDto.TransactionType;

                await _context.SaveChangesAsync();
                return existingTransaction;
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                throw new Exception("An error occurred while updating the transaction.", ex);
            }
        }
    }
}
