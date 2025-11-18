using MediatR;
using Microsoft.AspNetCore.Identity;
using Users.Domain.Interfaces;

namespace Users.Application.Features.Commands.LoginUser
{
    public class LoginUserHandler : IRequestHandler<LoginUserCommand, Guid>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<object> _passwordHasher;

        public LoginUserHandler(
            IUserRepository userRepository,
            IPasswordHasher<object> passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<Guid> Handle(LoginUserCommand request, CancellationToken cancellationToken)
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

            return user.Id;
        }
    }
}