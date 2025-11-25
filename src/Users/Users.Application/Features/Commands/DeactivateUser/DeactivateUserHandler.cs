using MediatR;
using Users.Domain.Interfaces;
using System.Net.Http;

namespace Users.Application.Features.Commands.DeactivateUser
{
    public class DeactivateUserHandler : IRequestHandler<DeactivateUserCommand, bool>
    {
        private readonly IUserRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;

        public DeactivateUserHandler(IUserRepository repository, IHttpClientFactory httpClientFactory)
        {
            _repository = repository;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _repository.GetByIdAsync(request.Id);

            if (user == null)
                return false;

            user.Deactivate();
            _repository.Update(user);
            await _repository.SaveAsync();

            var client = _httpClientFactory.CreateClient("ProductsApi");
            var response = await client.PatchAsync($"/api/products/owner/{user.Id}/soft-delete", null, cancellationToken);

            return true;
        }
    }
}
