using MediatR;
using Users.Domain.Interfaces;
using System.Security.Cryptography;
using Users.Application.Services;

namespace Users.Application.Features.Commands.ForgotPassword
{
    public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, string>
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService; 

        public ForgotPasswordHandler(IUserRepository userRepository, IEmailService emailService)
        {
            _userRepository = userRepository;
            _emailService = emailService;
        }

        public async Task<string> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Dto.Email, cancellationToken);

            if (user == null || !user.IsActive)
            {
                return "Если пользователь существует, ему будет отправлено письмо.";
            }

            var resetToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            var tokenExpiry = DateTime.UtcNow.AddHours(1); 

            user.SetPasswordResetToken(resetToken, tokenExpiry);

            _userRepository.Update(user); 
            await _userRepository.SaveAsync(cancellationToken);

            var confirmationLink = $"https://localhost:7096/api/auth/reset-password?email={user.Email}&token={resetToken}";

            var subject = "Сброс пароля";
            var body = $"Для создания нового пароля перейдите по ссылке: <a href='{confirmationLink}'>Сброс пароля</a>";

            await _emailService.SendEmailAsync(user.Email, subject, body);

            return $"Токен сброса: {resetToken}. Он был сохранен для пользователя {user.Email}";
        }
    }
}