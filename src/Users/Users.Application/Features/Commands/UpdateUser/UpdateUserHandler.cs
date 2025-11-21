using MediatR;
using Users.Application.Exceptions;
using Users.Domain.Interfaces;
using Users.Domain.Enums;

namespace Users.Application.Features.Commands.UpdateUser
{
    public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, bool>
    {
        private readonly IUserRepository _userRepository;

        public UpdateUserHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new NotFoundException($"Пользователь с Id: {request.Id} не найден");

            if (user.Email != request.User.Email)
            {
                var existingUser = await _userRepository.GetByEmailAsync(request.User.Email, cancellationToken);
                if (existingUser != null && existingUser.Id != user.Id)
                {
                    throw new ValidationException("Пользователь с таким Email уже зарегистрирован");
                }
            }

            user.UpdateName(request.User.Name);
            user.UpdateEmail(request.User.Email);

            if (!Enum.TryParse<Role>(request.User.Role, true, out var role))
            {
                throw new ValidationException($"Некорректное значение роли: '{request.User.Role}'. Допустимые значения: User, Admin");
            }
            user.UpdateRole(role);

            _userRepository.Update(user);
            await _userRepository.SaveAsync(cancellationToken);

            return true;
        }

    }
}
