using FluentValidation;
using Users.Application.DTOs;

namespace Users.Application.Features.Commands.LoginUser
{
    public class LoginUserValidator : AbstractValidator<LoginUserCommand>
    {
        public LoginUserValidator()
        {
            RuleFor(u => u.Login.Email)
                .NotEmpty().WithMessage("Email не может быть пустым")
                .EmailAddress().WithMessage("Некорректный формат Email");

            RuleFor(u => u.Login.Password).NotEmpty().WithMessage("Пароль не может быть пустым");
        }
    }
}