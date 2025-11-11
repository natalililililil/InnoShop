namespace Users.Application.DTOs
{
    public record UserDto(Guid Id, string Name, string Email, string Role, bool IsActive);
}
