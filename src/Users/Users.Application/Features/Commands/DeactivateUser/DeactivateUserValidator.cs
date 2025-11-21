using FluentValidation;
using Users.Application.Features.Commands.DeactivateUser;

public class DeactivateUserValidator : AbstractValidator<DeactivateUserCommand>
{
    public DeactivateUserValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Идентификатор пользователя не может быть пустым");
    }
}
