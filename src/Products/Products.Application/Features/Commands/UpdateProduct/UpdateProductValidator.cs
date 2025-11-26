using FluentValidation;

namespace Products.Application.Features.Commands.UpdateProduct
{
    public class UpdateProductValidator: AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductValidator()
        {
            RuleFor(x => x.Product.Name)
                .NotEmpty().WithMessage("Название продукта обязательно")
                .MaximumLength(150).WithMessage("Название продукта не может превышать 150 символов");

            RuleFor(x => x.Product.Description)
                .MaximumLength(2000).WithMessage("Описание не может превышать 2000 символов");

            RuleFor(x => x.Product.Price)
                .GreaterThan(0).WithMessage("Цена должна быть больше нуля");
        }
    }
}
