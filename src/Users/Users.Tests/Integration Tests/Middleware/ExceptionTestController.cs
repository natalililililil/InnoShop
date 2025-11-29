using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Users.Application.Exceptions;

namespace Users.Tests.Integration_Tests.Middleware
{
    [Route("api/test")]
    [ApiController]
    public class ExceptionTestController : ControllerBase
    {
        [HttpGet("validation")]
        public IActionResult ThrowValidation()
        {
            var failure = new ValidationFailure("Field", "Validation failed message.");
            throw new ValidationException(failure.ToString());
        }

        [HttpGet("notfound")]
        public IActionResult ThrowNotFound()
        {
            throw new NotFoundException("Resource not found.");
        }

        [HttpGet("internal")]
        public IActionResult ThrowInternal()
        {
            throw new InvalidOperationException("Internal server logic error.");
        }
    }
}
