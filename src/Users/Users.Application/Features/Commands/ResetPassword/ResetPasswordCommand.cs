using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Application.DTOs;

namespace Users.Application.Features.Commands.ResetPassword
{
    public record ResetPasswordCommand(ResetPasswordDto Dto) : IRequest<bool>;
}
