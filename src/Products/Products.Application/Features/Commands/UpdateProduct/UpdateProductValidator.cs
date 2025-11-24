using FluentValidation;

namespace Products.Application.Features.Commands.UpdateProduct
{
    public class UpdateProductValidator: AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductValidator()
        {
            RuleFor(x => x.Product.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Product.Description).MaximumLength(2000);
            RuleFor(x => x.Product.Price).GreaterThanOrEqualTo(0);
        }
    }
}
