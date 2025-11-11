using MediatR;
using Users.Application.Exceptions;
using Users.Domain.Interfaces;

namespace Users.Application.Features.Commands.UpdateUser
{
    public class UpdateUserHandler : IRequestHandler<UpdateUserCommand>
    {
        private readonly IUserRepository _userRepository;

        public UpdateUserHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new NotFoundException($"User with ID {request.Id} not found");

            user = new(user.Name, request.Email, user.PasswordHash, user.Role);
            _userRepository.Update(user);
            await _userRepository.SaveAsync(cancellationToken);
        }
    }
}
