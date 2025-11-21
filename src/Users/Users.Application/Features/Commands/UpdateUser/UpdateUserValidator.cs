using FluentValidation;
using Users.Application.Features.Commands.UpdateUser;
using Users.Domain.Enums;

public class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("ID пользователя не может быть пустым");

        RuleFor(x => x.User.Name)
            .NotEmpty().WithMessage("Имя не может быть пустым при обновлении")
            .MaximumLength(50).WithMessage("Имя не должно превышать 50 символов");

        RuleFor(x => x.User.Email)
            .NotEmpty().WithMessage("Email не может быть пустым при обновлении")
            .EmailAddress().WithMessage("Некорректный формат Email");

        RuleFor(x => x.User.Role)
            .Must(BeAValidRole).WithMessage("Некорректное значение роли. Допустимые значения: User, Admin");
    }

    private bool BeAValidRole(string role)
    {
        return Enum.TryParse<Role>(role, true, out _);
    }
}