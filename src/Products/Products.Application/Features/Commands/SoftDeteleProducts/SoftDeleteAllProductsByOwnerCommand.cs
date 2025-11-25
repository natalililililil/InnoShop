using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Products.Application.Features.Commands.SoftDeteleProducts
{
    public record SoftDeleteAllProductsByOwnerCommand(Guid OwnerId) : IRequest<bool>;
}
