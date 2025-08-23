using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using server.DTOs.Transaction;
using server.Extensions;
using server.Interfaces;
using server.Mappers;
using server.Models;

namespace server.Controllers
{
    [Route("api/client/transaction")]
    [ApiController]
    [Authorize]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IStockRepository _stockRepository;
        private readonly UserManager<AppUser> _userManager;

        public TransactionController(ITransactionRepository transactionRepository, 
            IStockRepository stockRepository, 
            UserManager<AppUser> userManager)
        {
            _transactionRepository = transactionRepository;
            _stockRepository = stockRepository;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserTransactions()
        {
            var username = User.GetUsername();

            if (string.IsNullOrEmpty(username))
                return Unauthorized("User is not authenticated.");

            var user = await _userManager.FindByNameAsync(username);

            var transactions = await _transactionRepository.GetUserTransactionsAsync(user!.Id);
            var transactionDtos = transactions.Select(t => t.ToTransactionDto()).ToList();
            return Ok(transactionDtos);
        }

        [HttpGet("stock/{stockId:int}")]
        public async Task<IActionResult> GetUserTransactionsByStock([FromRoute] int stockId)
        {
            var username = User.GetUsername();

            if (string.IsNullOrEmpty(username))
                return Unauthorized("User is not authenticated.");

            var user = await _userManager.FindByNameAsync(username);

            var transactions = await _transactionRepository.GetUserTransactionsByStockAsync(user!.Id, stockId);
            var transactionDtos = transactions.Select(t => t.ToTransactionDto()).ToList();

            return Ok(transactionDtos);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var transaction = await _transactionRepository.GetByIdAsync(id);

            if (transaction == null)
                return NotFound("Transaction not found.");

            var username = User.GetUsername();
            var appUser = await _userManager.FindByNameAsync(username);

            if (transaction.UserId != appUser!.Id)
            {
                return Forbid();
            }

            var transactionDto = transaction.ToTransactionDto();

            return Ok(transactionDto);
        }

        [HttpGet("date-range")]
        public async Task<IActionResult> GetTransactionsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var username = User.GetUsername();

            if (string.IsNullOrEmpty(username))
                return Unauthorized("User is not authenticated.");

            var user = await _userManager.FindByNameAsync(username);

            var transactions = await _transactionRepository.GetTransactionsByDateRangeAsync(user!.Id, startDate, endDate);
            var transactionDtos = transactions.Select(t => t.ToTransactionDto()).ToList();

            return Ok(transactionDtos);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDto transactionDto)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            if(!await _stockRepository.IsStockExistsAsync(transactionDto.StockId))
                return NotFound("Stock not found.");

            var username = User.GetUsername();

            if (string.IsNullOrEmpty(username))
                return Unauthorized("User is not authenticated.");

            var user = await _userManager.FindByNameAsync(username);

            var transactionModel = transactionDto.ToTransactionFromCreate();
            transactionModel.UserId = user!.Id;

            await _transactionRepository.CreateAsync(transactionModel);

            return CreatedAtAction(nameof(GetById), new { id = transactionModel.Id }, transactionModel.ToTransactionDto());
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateTransaction(int id, [FromBody] CreateTransactionDto transactionDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var username = User.GetUsername();

            if (string.IsNullOrEmpty(username))
                return Unauthorized("User is not authenticated.");

            var appUser = await _userManager.FindByNameAsync(username);

            var existingTransaction = await _transactionRepository.GetByIdAsync(id);

            if (existingTransaction == null)
            {
                return NotFound("Transaction not found.");
            }

            if (existingTransaction.UserId != appUser!.Id)
            {
                return Forbid();
            }

            var updatedTransaction = await _transactionRepository.UpdateAsync(id, transactionDto);

            return Ok(updatedTransaction);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var username = User.GetUsername();

            if (string.IsNullOrEmpty(username))
                return Unauthorized("User is not authenticated.");

            var appUser = await _userManager.FindByNameAsync(username);

            var existingTransaction = await _transactionRepository.GetByIdAsync(id);

            if (existingTransaction == null)
            {
                return NotFound("Transaction not found.");
            }

            if (existingTransaction.UserId != appUser!.Id)
            {
                return Forbid();
            }

             await _transactionRepository.DeleteAsync(id);

            return Ok(new
            {
                Message = "Transaction deleted successfully.",
            });
        }
    }
}
