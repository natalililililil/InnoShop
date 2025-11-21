using FluentValidation;

namespace Users.Application.Features.Commands.DeleteUser
{
    public class DeleteUserValidator : AbstractValidator<DeleteUserCommand>
    {
        public DeleteUserValidator()
        {
            RuleFor(c => c.Id)
                .NotEmpty().WithMessage("Идентификатор пользователя не может быть пустым.");
        }
    }
}