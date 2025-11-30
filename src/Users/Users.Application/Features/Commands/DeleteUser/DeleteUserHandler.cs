using MediatR;

using Users.Domain.Interfaces;

namespace Users.Application.Features.Commands.DeleteUser
{
    public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, bool>
    {
        private readonly IUserRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;

        public DeleteUserHandler(IUserRepository repository, IHttpClientFactory httpClientFactory)
        {
            _repository = repository;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _repository.GetByIdAsync(request.Id);

            if (user == null)
                return false;
            var client = _httpClientFactory.CreateClient("ProductsApi");
            var productsDeletePath = $"/api/products/owner/{user.Id}/delete";

            var response = await client.DeleteAsync(productsDeletePath, cancellationToken);

            if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }

            _repository.Delete(user);
            await _repository.SaveAsync();

            return true;
        }
    }
}
