
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.DTOs.Auth;
using server.Interfaces;
using server.Models;

namespace server.Controllers
{
    [Route("api/client/auth")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;

        public AccountController(UserManager<AppUser> userManager,
            ITokenService tokenService,
            IEmailService emailService,
            SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto register)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var user = new AppUser
                {
                    UserName = register.Username,
                    Email = register.Email,
                    FirstName = register.FirstName,
                    LastName = register.LastName,
                    DateJoined = DateTime.Now,
                    LastLogin = DateTime.Now,
                };

                var createResult = await _userManager.CreateAsync(user, register.Password);

                if (createResult.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, "User");
                    if (roleResult.Succeeded)
                    {
                        // Send welcome email
                        await _emailService.SendWelcomeEmailAsync(user.Email!, user.FirstName, user.LastName);

                        return Ok(new UserDto
                        {
                            Username = user.UserName!,
                            Email = user.Email!,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Token = _tokenService.CreateToken(user)
                        });
                    }
                    else
                    {
                        return StatusCode(500, roleResult.Errors);
                    }
                }
                else
                {
                    return StatusCode(500, createResult.Errors);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto login)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.Users
                .FirstOrDefaultAsync(x => x.UserName == login.Username.ToLower());

            if (user == null) return Unauthorized("Invalid username!");

            var result = await _signInManager.CheckPasswordSignInAsync(user, login.Password, false);

            if (!result.Succeeded) return Unauthorized("Username not found and/or password incorrect");

            user.LastLogin = DateTime.Now;
            await _userManager.UpdateAsync(user);

            // Send welcome email if this is the first login
            //await _emailService.SendWelcomeEmailAsync(user.Email!, user.FirstName, user.LastName);

            return Ok(new UserDto
            {
                Username = user.UserName!,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = _tokenService.CreateToken(user)
            });
        }
    }
}
