using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Users.Domain.Entities;
using Users.Domain.Enums;
using Users.Infrastructure.Persistence;

namespace Users.Tests.Integration_Tests.API
{
    public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        protected readonly CustomWebApplicationFactory _factory;
        protected readonly HttpClient _client;
        protected readonly UserDbContext _context;

        public IntegrationTestBase(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            ClearAuthentication();

            var scope = _factory.Services.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<UserDbContext>();

            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
        }

        protected StringContent GetStringContent(object obj)
        {
            var json = JsonSerializer.Serialize(obj);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        protected void AuthenticateClient(Guid userId, string role)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);

            var claimsData = new { UserId = userId, Role = role };
            var jsonClaims = JsonSerializer.Serialize(claimsData);
            var base64Claims = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonClaims));

            if (_client.DefaultRequestHeaders.Contains("X-Test-Claims"))
            {
                _client.DefaultRequestHeaders.Remove("X-Test-Claims");
            }

            _client.DefaultRequestHeaders.Add("X-Test-Claims", base64Claims);
        }

        protected void ClearAuthentication()
        {
            _client.DefaultRequestHeaders.Remove("Authorization");
            _client.DefaultRequestHeaders.Remove("X-Test-Claims");
        }

        protected async Task<User> SeedUser(string email, string role = "User", bool isActive = true)
        {
            var hasher = new PasswordHasher<object>();
            var passwordHash = hasher.HashPassword(null!, "P@ssword123");

            var roleEnum = role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ? Role.Admin : Role.User;
            var userId = Guid.NewGuid();

            var user = new User(
                userId,
                "Test User",
                email,
                passwordHash,
                roleEnum
            );

            user.ConfirmEmail();

            if (isActive)
            {
                user.Activate();
            }
            else
            {
                user.Deactivate();
            }

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            _context.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            return user;
        }

        public void Dispose()
        {
            _client.Dispose();
            _context.Database.EnsureDeleted();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
