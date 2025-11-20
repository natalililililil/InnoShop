using MediatR;
using Microsoft.AspNetCore.Mvc;
using Users.Application.DTOs;
using Users.Application.Features.Commands.ConfirmEmail;
using Users.Application.Features.Commands.CreateUser;
using Users.Application.Features.Commands.ForgotPassword;
using Users.Application.Features.Commands.LoginUser;
using Users.Application.Features.Commands.ResetPassword;
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

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
                {
                    return BadRequest("Некорректная ссылка подтверждения");
                }

                var command = new ConfirmEmailCommand(email, token);
                var success = await _mediator.Send(command);

                if (success)
                {
                    return Ok("Ваш аккаунт успешно подтвержден! Теперь вы можете войти");
                }

                return BadRequest("Не удалось подтвердить аккаунт");
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка подтверждения: {ex.Message}");
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            try
            {
                var command = new ForgotPasswordCommand(dto);
                var resultMessage = await _mediator.Send(command);

                return Ok(new { message = "Если пользователь существует, ссылка для сброса была отправлена на почту." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Ошибка при обработке запроса: " + ex.Message });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            try
            {
                var command = new ResetPasswordCommand(dto);
                var success = await _mediator.Send(command);

                if (success)
                if (success)
                {
                    return Ok(new { message = "Пароль успешно сброшен." });
                }
                return BadRequest(new { message = "Не удалось сбросить пароль." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}