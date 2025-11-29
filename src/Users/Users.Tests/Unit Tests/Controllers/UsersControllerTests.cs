using Moq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Users.Api.Controllers;
using Users.Application.DTOs;
using Users.Application.Features.Commands.CreateUser;
using Users.Application.Features.Commands.DeleteUser;
using Users.Application.Features.Commands.UpdateUser;
using Users.Application.Features.Queries.GetAllUsers;
using Users.Application.Features.Queries.GetUserById;
using Users.Domain.Entities;
using Users.Domain.Enums;
using Users.Application.Features.Commands.ChangeUserStatusBase.DeactivateUser;
using Users.Application.Features.Commands.ChangeUserStatusBase.ActivateUser;

namespace Users.Tests.Unit_Tests.Controlles
{
    public class UsersControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly UsersController _controller;
        private readonly Guid TestUserId = Guid.NewGuid();

        public UsersControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _controller = new UsersController(_mockMediator.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithUserList()
        {
            var expectedList = new List<UserDto> 
            {
                new UserDto(Guid.NewGuid(), "User1", "user1@test.com", "User", true),
                new UserDto(Guid.NewGuid(), "User2", "user2@test.com", "User", true)
            };
            
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllUsersQuery>(), default))
                         .ReturnsAsync(expectedList);

            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUsers = Assert.IsAssignableFrom<IEnumerable<UserDto>>(okResult.Value);
            Assert.Equal(2, returnedUsers.Count());
            _mockMediator.Verify(m => m.Send(It.IsAny<GetAllUsersQuery>(), default), Times.Once);
        }


        [Fact]
        public async Task GetById_ExistingId_ReturnsOkWithUser()
        {
            var expectedUser = new UserDto(TestUserId, "TestUser", "test@test.com", "User", true);
            
            _mockMediator.Setup(m => m.Send(It.IsAny<GetUserByIdQuery>(), default))
                         .ReturnsAsync(expectedUser);

            var result = await _controller.GetById(TestUserId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUser = Assert.IsType<UserDto>(okResult.Value);
            Assert.Equal(TestUserId, returnedUser.Id);
            _mockMediator.Verify(m => m.Send(It.Is<GetUserByIdQuery>(q => q.Id == TestUserId), default), Times.Once);
        }
        

        [Fact]
        public async Task Create_ValidData_ReturnsCreatedAction()
        {
            var userDto = new CreateUserDto { Name = "NewUser", Email = "new@test.com", Password = "Pass123" };
            var createdUserEntity = new User(TestUserId, "NewUser", "new@test.com", "hash", Role.User);

            _mockMediator.Setup(m => m.Send(It.IsAny<CreateUserCommand>(), default))
                         .ReturnsAsync(createdUserEntity);

            var result = await _controller.Create(userDto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetById), createdResult.ActionName);
            Assert.Equal(TestUserId, createdResult.RouteValues["id"]);
            Assert.IsType<User>(createdResult.Value);
            _mockMediator.Verify(m => m.Send(It.IsAny<CreateUserCommand>(), default), Times.Once);
        }


        [Fact]
        public async Task Update_ExistingUser_ReturnsNoContent()
        {
            var updateDto = new UpdateUserDto { Name = "UpdatedName" };
            
            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateUserCommand>(), default))
                         .ReturnsAsync(true);

            var result = await _controller.Update(TestUserId, updateDto);

            Assert.IsType<NoContentResult>(result);
            _mockMediator.Verify(m => m.Send(
                It.Is<UpdateUserCommand>(c => c.Id == TestUserId && c.User == updateDto), default), Times.Once);
        }

        [Fact]
        public async Task Update_NonExistingUser_ReturnsNotFound()
        {
            var updateDto = new UpdateUserDto { Name = "UpdatedName" };
            
            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateUserCommand>(), default))
                         .ReturnsAsync(false);

            var result = await _controller.Update(TestUserId, updateDto);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ExistingUser_ReturnsNoContent()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteUserCommand>(), default))
                         .ReturnsAsync(true);

            var result = await _controller.Delete(TestUserId);

            Assert.IsType<NoContentResult>(result);
            _mockMediator.Verify(m => m.Send(It.Is<DeleteUserCommand>(c => c.Id == TestUserId), default), Times.Once);
        }

        [Fact]
        public async Task Delete_NonExistingUser_ReturnsNotFound()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteUserCommand>(), default))
                         .ReturnsAsync(false);

            var result = await _controller.Delete(TestUserId);

            Assert.IsType<NotFoundResult>(result);
        }
        
        [Fact]
        public async Task Deactivate_ExistingUser_ReturnsNoContent()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DeactivateUserCommand>(), default))
                         .ReturnsAsync(true);

            var result = await _controller.Deactivate(TestUserId);

            Assert.IsType<NoContentResult>(result);
            _mockMediator.Verify(m => m.Send(It.Is<DeactivateUserCommand>(c => c.Id == TestUserId), default), Times.Once);
        }

        [Fact]
        public async Task Deactivate_NonExistingUser_ReturnsNotFound()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DeactivateUserCommand>(), default))
                         .ReturnsAsync(false);

            var result = await _controller.Deactivate(TestUserId);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Activate_ExistingUser_ReturnsNoContent()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<ActivateUserCommand>(), default))
                         .ReturnsAsync(true);

            var result = await _controller.Activate(TestUserId);

            Assert.IsType<NoContentResult>(result);
            _mockMediator.Verify(m => m.Send(It.Is<ActivateUserCommand>(c => c.Id == TestUserId), default), Times.Once);
        }

        [Fact]
        public async Task Activate_NonExistingUser_ReturnsNotFound()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<ActivateUserCommand>(), default))
                         .ReturnsAsync(false);

            var result = await _controller.Activate(TestUserId);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}