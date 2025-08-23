using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using server.DTOs.Comment;
using server.Extensions;
using server.Interfaces;
using server.Mappers;
using server.Models;

namespace server.Controllers
{
    [Route("api/cliennt/comment")]
    [ApiController]
    [Authorize]
    public class CommentController : ControllerBase
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IStockRepository _stockRepository;
        private readonly UserManager<AppUser> _userManager;

        public CommentController(
            ICommentRepository commentRepository,
            IStockRepository stockRepository,
            UserManager<AppUser> userManager)
        {
            _userManager = userManager;
            _commentRepository = commentRepository;
            _stockRepository = stockRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllC()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var comments = await _commentRepository.GetAllAsync();
            var commentDto = comments.Select(s => s.ToCommentDto());

            return Ok(commentDto);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var comment = await _commentRepository.GetByIdAsync(id);

            if (comment == null)
            {
                return NotFound();
            }

            return Ok(comment.ToCommentDto());
        }

        [HttpGet("stock/{stockId:int}")]
        public async Task<IActionResult> GetCommentByStock(int stockId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var comments = await _commentRepository.GetCommentsByStockIdAsync(stockId);
            var commentDtos = comments.Select(c => c.ToCommentDto()).ToList();

            return Ok(commentDtos);
        }

        [HttpPost("{stockId:int}")]
        public async Task<IActionResult> Create([FromBody] CreateCommentDto commentDto, [FromRoute] int stockId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await _stockRepository.IsStockExistsAsync(stockId))
            {
                return BadRequest($"Stock with ID {stockId} not found.");
            }

            var username = User.GetUsername();
            var user = await _userManager.FindByNameAsync(username);

            var commentModel = commentDto.ToCommentFromCreate(stockId);
            commentModel.UserId = user!.Id;

            await _commentRepository.CreateAsync(commentModel);

            return CreatedAtAction(nameof(GetById), new { id = commentModel.Id }, commentModel.ToCommentDto());
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute]int id, [FromBody] UpdateCommentDto commentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var commnet = await _commentRepository.UpdateAsync(id , commentDto);
           
            if (commnet == null)
            {
                return NotFound($"Comment with ID {id} not found.");
            }

            return Ok(commnet.ToCommentDto());
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var username = User.GetUsername();
            var user = await _userManager.FindByNameAsync(username);

            var comment = await _commentRepository.GetByIdAsync(id);

            if (comment == null)
            {
                return NotFound($"Comment with ID {id} not found.");
            }

            if (comment.UserId != user!.Id)
            {
                return Forbid("You are not allowed to delete this comment.");
            }

            await _commentRepository.DeleteAsync(id);
            return Ok(new
            {
                Message = $"Comment with ID {id} has been deleted successfully."
            });
        }
    }
}
