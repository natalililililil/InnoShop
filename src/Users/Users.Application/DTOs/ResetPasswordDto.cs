using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Application.DTOs
{
    public record ResetPasswordDto(string Token, string Email, string NewPassword);
}
