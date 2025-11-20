using MediatR;
using Microsoft.AspNetCore.Identity;
using Users.Domain.Interfaces;

namespace Users.Application.Features.Commands.ResetPassword
{
    public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<object> _passwordHasher;

        public ResetPasswordHandler(IUserRepository userRepository, IPasswordHasher<object> passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Dto.Email, cancellationToken);
            var dto = request.Dto;

            if (user == null || !user.IsActive)
            {
                throw new Exception("Неверный запрос или пользователь не найден.");
            }

            if (user.PasswordResetToken == null || user.PasswordResetToken != dto.Token)
            {
                throw new Exception("Неверный или просроченный токен сброса.");
            }

            if (user.PasswordResetTokenExpiry == null || user.PasswordResetTokenExpiry.Value < DateTime.UtcNow)
            {
                user.ClearPasswordResetToken();
                _userRepository.Update(user);
                await _userRepository.SaveAsync(cancellationToken);
                throw new Exception("Срок действия токена сброса истек.");
            }

            var newPasswordHash = _passwordHasher.HashPassword(null!, dto.NewPassword);

            user.UpdatePasswordHash(newPasswordHash);
            user.ClearPasswordResetToken();

            _userRepository.Update(user);
            await _userRepository.SaveAsync(cancellationToken);

            return true;
        }
    }
}