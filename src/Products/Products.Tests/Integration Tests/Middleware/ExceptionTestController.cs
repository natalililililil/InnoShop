using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace Products.Tests.Integration_Tests.Middleware
{
    [Route("api/test")]
    [ApiController]
    public class ExceptionTestController : ControllerBase
    {
        [HttpGet("validation")]
        public IActionResult ThrowValidation()
        {
            var failure = new ValidationFailure("Field", "Validation failed message.");
            throw new ValidationException(new[] { failure });
        }

        [HttpGet("unauthorized")]
        public IActionResult ThrowUnauthorizedAccess()
        {
            throw new UnauthorizedAccessException("Access denied message.");
        }

        [HttpGet("argument")]
        public IActionResult ThrowArgumentException()
        {
            throw new ArgumentException("Invalid argument provided.");
        }

        [HttpGet("internal")]
        public IActionResult ThrowInternal()
        {
            throw new InvalidOperationException("Internal server logic error.");
        }
    }
}