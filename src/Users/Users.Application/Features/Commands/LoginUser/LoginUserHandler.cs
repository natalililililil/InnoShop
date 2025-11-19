using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Users.Application.DTOs;
using Users.Application.Services;
using Users.Domain.Interfaces;

namespace Users.Application.Features.Commands.LoginUser
{
    public class LoginUserHandler : IRequestHandler<LoginUserCommand, AuthResultDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<object> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        public LoginUserHandler(IUserRepository userRepository, IPasswordHasher<object> passwordHasher, 
            IConfiguration configuration, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _tokenService = tokenService;
        }

        public async Task<AuthResultDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Login.Email, cancellationToken);

            if (user == null || !user.IsActive)
            {
                throw new Exception("Неверный логин или пароль.");
            }

            var verificationResult = _passwordHasher.VerifyHashedPassword(
                null!,
                user.PasswordHash,
                request.Login.Password
            );

            if (verificationResult == PasswordVerificationResult.Failed)
            {
                throw new Exception("Неверный логин или пароль.");
            }

            if (!user.EmailConfirmed)
            {
                throw new Exception("Аккаунт не подтвержден. Пожалуйста, проверьте свою почту для подтверждения");
            }

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"]!);
            var expiryDate = DateTime.UtcNow.AddMinutes(expiryMinutes);

            var token = _tokenService.GenerateToken(user, expiryDate);

            return new AuthResultDto(token, expiryDate);
        }
    }
}