namespace Users.Application.DTOs
{
    public record AuthResultDto(string Token, DateTime ExpiryDate);
}