using MediatR;
using Users.Domain.Interfaces;

namespace Users.Application.Features.Commands.ConfirmEmail
{
    public class ConfirmEmailHandler : IRequestHandler<ConfirmEmailCommand, bool>
    {
        private readonly IUserRepository _userRepository;

        public ConfirmEmailHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

            if (user == null || user.EmailConfirmed)
            {
                return true;
            }

            if (user.EmailConfirmationToken != request.Token)
            {
                throw new Exception("Неверный токен подтверждения");
            }

            if (user.EmailConfirmationTokenExpiry.Value < DateTime.UtcNow)
            {
                user.ClearEmailConfirmationToken();
                _userRepository.Update(user);
                await _userRepository.SaveAsync(cancellationToken);
                throw new Exception("Срок действия токена подтверждения истек");
            }

            user.ConfirmEmail();
            user.ClearEmailConfirmationToken();

            _userRepository.Update(user);
            await _userRepository.SaveAsync(cancellationToken);

            return true;
        }
    }
}