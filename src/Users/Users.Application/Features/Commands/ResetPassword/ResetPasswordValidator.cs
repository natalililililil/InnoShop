using FluentValidation;
using Users.Application.DTOs;

namespace Users.Application.Features.Commands.ResetPassword
{
    public class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordValidator()
        {
            RuleFor(r => r.Dto.Email)
                .NotEmpty().WithMessage("Email не может быть пустым")
                .EmailAddress().WithMessage("Некорректный формат Email");

            RuleFor(r => r.Dto.Token)
                .NotEmpty().WithMessage("Токен сброса не может быть пустым");

            RuleFor(r => r.Dto.NewPassword)
                .NotEmpty().WithMessage("Новый пароль не может быть пустым")
                .MinimumLength(6).WithMessage("Пароль должен содержать не менее 6 символов");
        }
    }
}