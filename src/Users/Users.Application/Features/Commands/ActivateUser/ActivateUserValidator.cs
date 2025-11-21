using FluentValidation;
using Users.Application.Features.Commands.ActivateUser;

public class ActivateUserValidator : AbstractValidator<ActivateUserCommand>
{
    public ActivateUserValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Идентификатор пользователя не может быть пустым");
    }
}
