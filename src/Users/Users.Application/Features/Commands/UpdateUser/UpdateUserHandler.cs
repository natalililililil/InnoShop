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
                ?? throw new NotFoundException($"User with ID {request.Id} not found");

            user.UpdateName(request.User.Name);
            user.UpdateEmail(request.User.Email);

            //пока пропускает фигню
            if (!Enum.TryParse<Role>(request.User.Role, true, out var role))
                throw new ArgumentException("Invalid role");

            user.UpdateRole(role);

            _userRepository.Update(user);
            await _userRepository.SaveAsync(cancellationToken);

            return true;
        }

    }
}
