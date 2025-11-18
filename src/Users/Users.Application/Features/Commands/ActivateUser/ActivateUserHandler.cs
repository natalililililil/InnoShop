using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Domain.Interfaces;

namespace Users.Application.Features.Commands.ActivateUser
{
    public class ActivateUserHandler : IRequestHandler<ActivateUserCommand, bool>
    {
        private readonly IUserRepository _repository;

        public ActivateUserHandler(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _repository.GetByIdAsync(request.Id);

            if (user == null)
                return false;

            user.Activate();
            _repository.Update(user);
            await _repository.SaveAsync();

            return true;
        }
    }
}
