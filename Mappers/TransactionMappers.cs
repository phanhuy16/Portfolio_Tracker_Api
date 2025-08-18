using server.DTOs.Transaction;
using server.Models;

namespace server.Mappers
{
    public static class TransactionMappers
    {
        public static TransactionDto ToTransactionDto(this Transaction transaction)
        {
            return new TransactionDto
            {
                Id = transaction.Id,
                Symbol = transaction.Stock?.Symbol ?? "",
                CompanyName = transaction.Stock?.CompanyName ?? "",
                TransactionType = transaction.TransactionType.ToString(),
                Quantity = transaction.Quantity,
                Price = transaction.Price,
                Commission = transaction.Commission,
                TotalAmount = transaction.TotalAmount,
                TransactionDate = transaction.TransactionDate,
                Notes = transaction.Notes
            };
        }

        public static Transaction ToTransactionFromCreate(this CreateTransactionDto createTransactionDto)
        {
            return new Transaction
            {
                StockId = createTransactionDto.StockId,
                Quantity = createTransactionDto.Quantity,
                Price = createTransactionDto.Price,
                Commission = createTransactionDto.Commission,
                TransactionType = (TransactionType)createTransactionDto.TransactionType,
                TransactionDate = createTransactionDto.TransactionDate,
            };
        }
    }
}
