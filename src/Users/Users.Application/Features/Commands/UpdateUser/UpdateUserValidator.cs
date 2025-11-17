using FluentValidation;

namespace Users.Application.Features.Commands.UpdateUser
{
    public class UpdateUserValidator<TKey> : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.User.Name).NotEmpty().MaximumLength(50);
            RuleFor(x => x.User.Email).NotEmpty().EmailAddress();
        }
    }
}
