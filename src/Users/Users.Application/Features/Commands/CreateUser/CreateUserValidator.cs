using FluentValidation;
using Users.Application.DTOs;

namespace Users.Application.Features.Commands.CreateUser
{
    public class CreateUserValidator : AbstractValidator<CreateUserDto>
    {
        public CreateUserValidator() 
        {
            RuleFor(u => u.Name).NotEmpty().MaximumLength(50);
            RuleFor(u => u.Email).NotEmpty().EmailAddress();
            RuleFor(u => u.Password).NotEmpty().MinimumLength(6);
        }
    }
}
