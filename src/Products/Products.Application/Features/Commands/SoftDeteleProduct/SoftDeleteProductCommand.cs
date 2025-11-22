using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Products.Application.Features.Commands.SoftDeteleProduct
{
    public record SoftDeleteProductCommand(Guid Id, Guid OwnerId) : IRequest<bool>;
}
