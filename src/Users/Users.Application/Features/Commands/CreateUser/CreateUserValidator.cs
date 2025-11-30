using FluentValidation;
using Users.Application.DTOs;

namespace Users.Application.Features.Commands.CreateUser
{
    public class CreateUserValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserValidator()
        {
            RuleFor(c => c.User.Name).NotEmpty().WithMessage("Имя не может быть пустым").MaximumLength(50);
            RuleFor(c => c.User.Email).NotEmpty().WithMessage("Email не может быть пустым").
                EmailAddress().WithMessage("Некорректный формат Email");
            RuleFor(c => c.User.Password).NotEmpty().WithMessage("Пароль не может быть пустым").
                MinimumLength(6).WithMessage("Пароль должен быть не менее 6 символов");
        }
    }
}
