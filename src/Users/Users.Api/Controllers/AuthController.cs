using MediatR;
using Microsoft.AspNetCore.Mvc;
using Users.Application.DTOs;
using Users.Application.Features.Commands.CreateUser;
using Users.Application.Features.Commands.LoginUser;
using Users.Application.Services;
using Users.Domain.Entities;

namespace Users.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;

        public AuthController(IMediator mediator, ITokenService tokenService, IConfiguration configuration)
        {
            _mediator = mediator;
            _configuration = configuration;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateUserDto userDto)
        {
            var command = new CreateUserCommand(userDto);
            User createdUser = await _mediator.Send(command);

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"]!);
            var expiryDate = DateTime.UtcNow.AddMinutes(expiryMinutes);

            var token = _tokenService.GenerateToken(createdUser, expiryDate);
            var result = new AuthResultDto(token, expiryDate);

            return CreatedAtAction(nameof(Login), result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var command = new LoginUserCommand(loginDto);
                var userId = await _mediator.Send(command);

                return Ok(new { UserId = userId, Message = "Аутентификация успешна." });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}