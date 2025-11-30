using MediatR;
using Products.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Products.Application.Features.Queries.SearchProducts
{
    public record SearchProductsQuery(string Query) : IRequest<IEnumerable<ProductDto>>;
}
