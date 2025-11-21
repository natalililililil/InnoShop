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
        private readonly IEmailService _emailService;
        private readonly IPasswordHasher<object> _passwordHasher;

        public CreateUserHandler(IUserRepository userRepository, IEmailService emailService, IPasswordHasher<object> passwordHasher)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _passwordHasher = passwordHasher;
        }

        public async Task<User> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var dto = request.User;

            var existingUser = await _userRepository.GetByEmailAsync(dto.Email, cancellationToken);
            if (existingUser != null)
            {
                throw new ValidationException("Пользователь с таким email уже зарегистрирован");
            }

            if (!Enum.TryParse<Role>(dto.Role, true, out var parsedRole))
            {
                throw new ValidationException($"Некорректное значение роли: '{dto.Role}'. Допустимые значения: User, Admin");
            }
            var role = parsedRole;


            var passwordHash = _passwordHasher.HashPassword(null!, dto.Password);
            var user = new User(dto.Name, dto.Email, passwordHash, role); 
            
            var confirmationToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            var tokenExpiry = DateTime.UtcNow.AddHours(24);
            user.SetEmailConfirmationToken(confirmationToken, tokenExpiry);

            try
            {
                var confirmationLink = $"https://localhost:7096/api/auth/confirm-email?email={user.Email}&token={confirmationToken}";
                var subject = "Подтверждение регистрации аккаунта";
                var body = $"Пожалуйста, подтвердите ваш адрес электронной почты, перейдя по ссылке: <a href='{confirmationLink}'>Подтвердить аккаунт</a>";
                
                await _emailService.SendEmailAsync(user.Email, subject, body);
            }
            catch (System.Net.Mail.SmtpFailedRecipientException)
            {
                throw new ValidationException("Указанный адрес электронной почты не существует. Пожалуйста, проверьте правильность написания почты");
            }
            catch (System.Net.Mail.SmtpException ex)
            {
                throw new Exception("Ошибка сервера при отправке письма. Регистрация отменена", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Не удалось отправить письмо подтверждения. Отмена регистрации", ex);
            }

            _userRepository.Create(user);
            await _userRepository.SaveAsync(cancellationToken);

            return user;
        }
    }
}