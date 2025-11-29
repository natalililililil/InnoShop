using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text;
using System.Text.Json;
using Users.Application.DTOs;
using Users.Domain.Enums;

namespace Users.Tests.Integration_Tests.API
{
    public class UsersControllerTests : IntegrationTestBase
    {
        private const string BaseUrl = "api/users";

        public UsersControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

        private StringContent GetStringContent<T>(T obj)
        {
            var json = JsonSerializer.Serialize(obj);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        [Fact]
        public async Task GetAll_AsAdmin_ReturnsListOfUsers()
        {
            var adminUser = await SeedUser("admin_test_getall@test.com", Role.Admin.ToString());
            var normalUser = await SeedUser("normal_test_getall@test.com", Role.User.ToString());
            AuthenticateClient(adminUser.Id, adminUser.Role.ToString());

            var response = await _client.GetAsync(BaseUrl);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<UserDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(users);
            Assert.True(users.Count >= 2);
            Assert.Contains(users, u => u.Email == adminUser.Email);
        }

        [Fact]
        public async Task GetById_UserExists_ReturnsUser()
        {
            var adminUser = await SeedUser("admin_test_id@get.com", Role.Admin.ToString());
            var targetUser = await SeedUser("target_user@get.com", Role.User.ToString());
            AuthenticateClient(adminUser.Id, adminUser.Role.ToString());

            var response = await _client.GetAsync($"{BaseUrl}/{targetUser.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<UserDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(user);
            Assert.Equal(targetUser.Id, user.Id);
            Assert.Equal("target_user@get.com", user.Email);
        }

        [Fact]
        public async Task GetById_UserNotExists_Returns404NotFound()
        {
            var adminUser = await SeedUser("admin_test_404@get.com", Role.Admin.ToString());
            AuthenticateClient(adminUser.Id, adminUser.Role.ToString());
            var nonExistentId = Guid.NewGuid();

            var response = await _client.GetAsync($"{BaseUrl}/{nonExistentId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetAll_AsUnauthenticated_Returns401Unauthorized()
        {
            var response = await _client.GetAsync(BaseUrl);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Create_AsAdmin_ReturnsCreated()
        {
            var adminUser = await SeedUser("admin_test_post@create.com", Role.Admin.ToString());
            AuthenticateClient(adminUser.Id, adminUser.Role.ToString());
            var newUserDto = new CreateUserDto{
                Name = "New Admin User",
                Email = "new.admin@test.com", 
                Password = "P@ssword123", 
                Role = "Admin"
            };

            var response = await _client.PostAsync(BaseUrl, GetStringContent(newUserDto));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var createdUser = await _context.Users.SingleOrDefaultAsync(u => u.Email == newUserDto.Email);
            Assert.NotNull(createdUser);
        }

        [Fact]
        public async Task Create_AsNormalUser_Returns403Forbidden()
        {
            var normalUser = await SeedUser("user_test_post@create.com", Role.User.ToString());
            AuthenticateClient(normalUser.Id, normalUser.Role.ToString());
            var newUserDto = new CreateUserDto
            {
                Name = "Fail",
                Email = "fail.admin@test.com",
                Password = "P@ssword123",
                Role = "User"
            };

            var response = await _client.PostAsync(BaseUrl, GetStringContent(newUserDto));

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Create_InvalidData_Returns400BadRequest()
        {
            var adminUser = await SeedUser("admin_test_post_bad@create.com", Role.Admin.ToString());
            AuthenticateClient(adminUser.Id, adminUser.Role.ToString());
            var invalidUserDto = new CreateUserDto
            {
                Name = "",
                Email = "invalid-emai",
                Password = "P@ssword123",
                Role = "Admin"
            };

            var response = await _client.PostAsync(BaseUrl, GetStringContent(invalidUserDto));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


        [Fact]
        public async Task Update_AsAdmin_ReturnsNoContent()
        {
            var adminUser = await SeedUser("admin_test_put@update.com", Role.Admin.ToString());
            var userToUpdate = await SeedUser("to_update@test.com", Role.User.ToString());
            AuthenticateClient(adminUser.Id, adminUser.Role.ToString());

            var updateDto = new UpdateUserDto
            {
                Name = "Updated Name",
                Email = "updated.email@test.com",
                Role = "User"
            };

            var response = await _client.PutAsync($"{BaseUrl}/{userToUpdate.Id}", GetStringContent(updateDto));

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            var updatedUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userToUpdate.Id);
            Assert.Equal("Updated Name", updatedUser!.Name);
            Assert.Equal("updated.email@test.com", updatedUser.Email);
        }

        [Fact]
        public async Task Update_NonExistentUser_Returns404NotFound()
        {
            var adminUser = await SeedUser("admin_test_put_404@update.com", Role.Admin.ToString());
            AuthenticateClient(adminUser.Id, adminUser.Role.ToString());
            var nonExistentId = Guid.NewGuid();
            var updateDto = new UpdateUserDto
            {
                Name = "Updated Name",
                Email = "updated.email@test.com",
                Role = "User"
            };

            var response = await _client.PutAsync($"{BaseUrl}/{nonExistentId}", GetStringContent(updateDto));

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Deactivate_AsAdmin_ReturnsNoContentAndDeactivates()
        {
            var adminUser = await SeedUser("admin_test_deact@patch.com", Role.Admin.ToString());
            var userToDeactivate = await SeedUser("deactivate_me@test.com", Role.User.ToString());
            AuthenticateClient(adminUser.Id, adminUser.Role.ToString());

            var response = await _client.PatchAsync($"{BaseUrl}/{userToDeactivate.Id}/deactivate", null);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            var deactivatedUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userToDeactivate.Id);
            Assert.False(deactivatedUser!.IsActive);
        }

        [Fact]
        public async Task Activate_AsAdmin_ReturnsNoContentAndActivates()
        {
            var adminUser = await SeedUser("admin_test_act@patch.com", Role.Admin.ToString());
            var userToActivate = await SeedUser("activate_me@test.com", Role.User.ToString(), isActive: false);
            AuthenticateClient(adminUser.Id, adminUser.Role.ToString());

            var response = await _client.PatchAsync($"{BaseUrl}/{userToActivate.Id}/activate", null);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            var activatedUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userToActivate.Id);
            Assert.True(activatedUser!.IsActive);
        }

        [Fact]
        public async Task Patch_NonExistentUser_Returns404NotFound()
        {
            var adminUser = await SeedUser("admin_test_patch_404@update.com", Role.Admin.ToString());
            AuthenticateClient(adminUser.Id, adminUser.Role.ToString());
            var nonExistentId = Guid.NewGuid();

            var response = await _client.PatchAsync($"{BaseUrl}/{nonExistentId}/deactivate", null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_AsAdmin_ReturnsNoContentAndDeletes()
        {
            var adminUser = await SeedUser("admin_test_del@delete.com", Role.Admin.ToString());
            var userToDelete = await SeedUser("delete_me@test.com", Role.User.ToString());
            AuthenticateClient(adminUser.Id, adminUser.Role.ToString());

            var response = await _client.DeleteAsync($"{BaseUrl}/{userToDelete.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            var deletedUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userToDelete.Id);
            Assert.Null(deletedUser);
        }

        [Fact]
        public async Task Delete_NonExistentUser_Returns404NotFound()
        {
            var adminUser = await SeedUser("admin_test_del_404@delete.com", Role.Admin.ToString());
            AuthenticateClient(adminUser.Id, adminUser.Role.ToString());
            var nonExistentId = Guid.NewGuid();

            var response = await _client.DeleteAsync($"{BaseUrl}/{nonExistentId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
