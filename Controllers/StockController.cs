using Microsoft.AspNetCore.Mvc;
using server.DTOs.Stocks;
using server.Helpers;
using server.Interfaces;
using server.Mappers;

namespace server.Controllers
{
    [Route("api/client/stock")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly IStockRepository _stockRepository;
        public StockController(IStockRepository stockRepository)
        {
            _stockRepository = stockRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] QueryObject query, [FromQuery] string? industry)
        {
            if (!string.IsNullOrEmpty(industry))
            {
                var stockByIndustry = await _stockRepository.GetStocksByIndustryAsync(industry);
                var stockDtos = stockByIndustry.Select(s => s.ToStockDto()).ToList();
                return Ok(stockDtos);
            }

            if (!string.IsNullOrEmpty(query.searchTerm))
            {
                var searchResults = await _stockRepository.GetAllAsync(query);
                var stockDtos = searchResults.Select(s => s.ToStockDto()).ToList();
                return Ok(stockDtos);
            }

            var stocks = await _stockRepository.GetAllAsync(query);
            var stockDtosList = stocks.Select(s => s.ToStockDto()).ToList();
            return Ok(stockDtosList);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var stockItem = await _stockRepository.GetByIdAsync(id);

            if (stockItem == null)
                return NotFound();

            return Ok(stockItem.ToStockDto());
        }

        [HttpGet("symbol/{symbol}")]
        public async Task<IActionResult> GetBySymbol([FromRoute] string symbol)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var stockItem = await _stockRepository.GetBySymbolAsync(symbol);

            if (stockItem == null)
                return NotFound($"Stock with symbol {symbol} not found");

            return Ok(stockItem.ToStockDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStockRequestDto stockDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var stockModel = stockDto.ToStockFromCreateDTO();
            await _stockRepository.CreateAsync(stockModel);

            return CreatedAtAction(nameof(GetById), new { id = stockModel.Id }, stockModel.ToStockDto());
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateStockRequestDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var stockItem = await _stockRepository.UpdateAsync(id, updateDto);
            if (stockItem == null)
                return NotFound();

            return Ok(stockItem.ToStockDto());
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var stockItem = await _stockRepository.DeleteAsync(id);

            if (stockItem == null)
                return NotFound();

            return Ok(new { Message = $"Stock with Id {id} has been deleted successfully", });
        }
    }
}
