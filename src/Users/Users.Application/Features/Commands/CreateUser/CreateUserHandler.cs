using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using Users.Application.Services;
using Users.Domain.Entities;
using Users.Domain.Enums;
using Users.Domain.Interfaces;

namespace Users.Application.Features.Commands.CreateUser
{
    public class CreateUserHandler : IRequestHandler<CreateUserCommand, User>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<object> _passwordHasher;
        private readonly IEmailService _emailService;

        public CreateUserHandler(IUserRepository userRepository, IPasswordHasher<object> passwordHasher, 
            IEmailService emailService)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
        }

        public async Task<User> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var dto = request.User;
            var role = Enum.TryParse<Role>(dto.Role, true, out var parsedRole) ? parsedRole : Role.User;

            var passwordHash = _passwordHasher.HashPassword(null!, dto.Password);
            var user = new User(dto.Name, dto.Email, passwordHash, role);

            var confirmationToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            var tokenExpiry = DateTime.UtcNow.AddHours(24);
            user.SetEmailConfirmationToken(confirmationToken, tokenExpiry);

            _userRepository.Create(user);
            await _userRepository.SaveAsync(cancellationToken);

            var confirmationLink = $"https://localhost:7096/api/auth/confirm-email?email={user.Email}&token={confirmationToken}";

            var subject = "Подтверждение регистрации аккаунта";
            var body = $"Пожалуйста, подтвердите ваш адрес электронной почты, перейдя по ссылке: <a href='{confirmationLink}'>Подтвердить аккаунт</a>";

            await _emailService.SendEmailAsync(user.Email, subject, body);

            return user;
        }
    }
}
