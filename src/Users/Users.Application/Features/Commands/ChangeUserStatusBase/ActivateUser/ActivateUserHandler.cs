using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Domain.Entities;
using Users.Domain.Interfaces;

namespace Users.Application.Features.Commands.ChangeUserStatusBase.ActivateUser
{
    public class ActivateUserHandler : ChangeUserStatusBaseHandler<ActivateUserCommand>
    {
        public ActivateUserHandler(IUserRepository repository, IHttpClientFactory httpClientFactory) : base(repository, httpClientFactory) { }

        protected override void ChangeUserState(User user)
        {
            user.Activate();
        }

        protected override string ProductsApiPath(Guid userId)
        {
            return $"/api/products/owner/{userId}/soft-restore";
        }
    }
}
