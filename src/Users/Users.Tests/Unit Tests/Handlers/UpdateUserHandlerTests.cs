using Moq;
using Users.Domain.Interfaces;
using Users.Application.Features.Commands.UpdateUser;
using Users.Domain.Entities;
using Users.Domain.Enums;
using Users.Application.DTOs;
using Users.Application.Exceptions;
using FluentValidation;

namespace Users.Tests.Unit_Tests.Handlers
{
    public class UpdateUserHandlerTests
    {
        private readonly Mock<IUserRepository> _mockRepo;
        private readonly UpdateUserHandler _handler;
        private readonly Guid TestUserId = Guid.NewGuid();
        private readonly Guid OtherUserId = Guid.NewGuid();

        public UpdateUserHandlerTests()
        {
            _mockRepo = new Mock<IUserRepository>();
            _handler = new UpdateUserHandler(_mockRepo.Object);
        }

        [Fact]
        public async Task Handle_ValidUpdate_ReturnsTrueAndUpdatesUser()
        {
            var user = new User(TestUserId, "OldName", "old@test.com", "hash", Role.User);
            var updateDto = new UpdateUserDto { Name = "NewName", Email = "new@test.com", Role = "Admin" };
            var command = new UpdateUserCommand(TestUserId, updateDto);

            _mockRepo.Setup(r => r.GetByIdAsync(TestUserId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(user);
            _mockRepo.Setup(r => r.GetByEmailAsync("new@test.com", It.IsAny<CancellationToken>()))
                     .ReturnsAsync((User)null!);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result);

            Assert.Equal("NewName", user.Name);
            Assert.Equal("new@test.com", user.Email);
            Assert.Equal(Role.Admin, user.Role);

            _mockRepo.Verify(r => r.Update(user), Times.Once);
            _mockRepo.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsNotFoundException()
        {
            var updateDto = new UpdateUserDto { Name = "NewName", Email = "new@test.com", Role = "User" };
            var command = new UpdateUserCommand(TestUserId, updateDto);

            _mockRepo.Setup(r => r.GetByIdAsync(TestUserId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((User)null!);

            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));

            _mockRepo.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_NewEmailAlreadyExists_ThrowsValidationException()
        {
            var userToUpdate = new User(TestUserId, "OldName", "old@test.com", "hash", Role.User);
            var existingUser = new User(OtherUserId, "Other", "used@test.com", "hash", Role.Admin);

            var updateDto = new UpdateUserDto { Name = "NewName", Email = "used@test.com", Role = "User" };
            var command = new UpdateUserCommand(TestUserId, updateDto);

            _mockRepo.Setup(r => r.GetByIdAsync(TestUserId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(userToUpdate);

            _mockRepo.Setup(r => r.GetByEmailAsync("used@test.com", It.IsAny<CancellationToken>()))
                     .ReturnsAsync(existingUser);

            await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));

            _mockRepo.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_InvalidRole_ThrowsValidationException()
        {
            var user = new User(TestUserId, "OldName", "old@test.com", "hash", Role.User);
            var updateDto = new UpdateUserDto { Name = "NewName", Email = "old@test.com", Role = "InvalidRole" };
            var command = new UpdateUserCommand(TestUserId, updateDto);

            _mockRepo.Setup(r => r.GetByIdAsync(TestUserId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(user);

            await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));

            _mockRepo.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}