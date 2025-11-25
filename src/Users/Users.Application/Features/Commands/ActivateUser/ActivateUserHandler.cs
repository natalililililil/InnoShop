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
        private readonly IHttpClientFactory _httpClientFactory;

        public ActivateUserHandler(IUserRepository repository, IHttpClientFactory httpClientFactory)
        {
            _repository = repository;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _repository.GetByIdAsync(request.Id);

            if (user == null)
                return false;

            user.Activate();
            _repository.Update(user);
            await _repository.SaveAsync();

            var client = _httpClientFactory.CreateClient("ProductsApi");
            var response = await client.PatchAsync($"/api/products/owner/{user.Id}/soft-restore", null, cancellationToken);

            return true;
        }
    }
}
