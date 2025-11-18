using MediatR;
using Microsoft.AspNetCore.Mvc;
using Users.Application.DTOs;
using Users.Application.Features.Commands.ActivateUser;
using Users.Application.Features.Commands.CreateUser;
using Users.Application.Features.Commands.DeactivateUser;
using Users.Application.Features.Commands.DeleteUser;
using Users.Application.Features.Commands.UpdateUser;
using Users.Application.Features.Queries.GetAllUsers;
using Users.Application.Features.Queries.GetUserById;

namespace Users.Api.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllUsersQuery());
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetUserByIdQuery(id));
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDto userDto)
        {
            var command = new CreateUserCommand(userDto);
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = id}, id);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
        {
            var success = await _mediator.Send(new UpdateUserCommand(id, dto));

            if (!success)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _mediator.Send(new DeleteUserCommand(id));

            if (!result)
                return NotFound();

            return NoContent();
        }


        [HttpPatch("{id:guid}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            var success = await _mediator.Send(new DeactivateUserCommand(id));

            if (!success)
                return NotFound();

            return NoContent();
        }

        [HttpPatch("{id:guid}/activate")]
        public async Task<IActionResult> Activate(Guid id)
        {
            var success = await _mediator.Send(new ActivateUserCommand(id));

            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}
