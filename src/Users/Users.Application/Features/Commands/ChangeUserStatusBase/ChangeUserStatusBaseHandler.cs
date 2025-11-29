using MediatR;
using Users.Domain.Entities;
using Users.Domain.Interfaces;

namespace Users.Application.Features.Commands.ChangeUserStatusBase
{
    public abstract class ChangeUserStatusBaseHandler<TCommand> : IRequestHandler<TCommand, bool> where TCommand : IRequest<bool>
    {
        private readonly IUserRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;

        protected ChangeUserStatusBaseHandler(IUserRepository repository, IHttpClientFactory httpClientFactory)
        {
            _repository = repository;
            _httpClientFactory = httpClientFactory;
        }

        protected abstract void ChangeUserState(User user);

        protected abstract string ProductsApiPath(Guid userId);

        public async Task<bool> Handle(TCommand request, CancellationToken cancellationToken)
        {
            var userId = (Guid)typeof(TCommand).GetProperty("Id")!.GetValue(request)!;

            var user = await _repository.GetByIdAsync(userId);

            if (user == null)
                return false;

            ChangeUserState(user);

            _repository.Update(user);
            await _repository.SaveAsync();

            var client = _httpClientFactory.CreateClient("ProductsApi");
            var response = await client.PatchAsync(ProductsApiPath(user.Id), null, cancellationToken);

            return true;
        }
    }
}
