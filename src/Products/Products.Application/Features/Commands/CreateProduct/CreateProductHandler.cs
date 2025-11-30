using MediatR;
using Products.Domain.Entities;
using Products.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Products.Application.Features.Commands.CreateProduct
{
    public class CreateProductHandler : IRequestHandler<CreateProductCommand, Guid>
    {
        private readonly IProductRepository _repository;
        public CreateProductHandler(IProductRepository repository) => _repository = repository;
        public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Product;
            var product = new Product(dto.Name, dto.Description, dto.Price, request.OwnerId);
            await _repository.CreateAsync(product, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            return product.Id;
        }
    }
}
