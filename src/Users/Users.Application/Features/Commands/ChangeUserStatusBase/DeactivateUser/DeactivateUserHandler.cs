using MediatR;
using Users.Domain.Interfaces;
using System.Net.Http;
using Users.Domain.Entities;

namespace Users.Application.Features.Commands.ChangeUserStatusBase.DeactivateUser
{
    public class DeactivateUserHandler : ChangeUserStatusBaseHandler<DeactivateUserCommand>
    {
        public DeactivateUserHandler(IUserRepository repository, IHttpClientFactory httpClientFactory) : base(repository, httpClientFactory) { }

        protected override void ChangeUserState(User user)
        {
            user.Deactivate();
        }

        protected override string ProductsApiPath(Guid userId)
        {
            return $"/api/products/owner/{userId}/soft-delete";
        }
    }
}
