namespace Users.Application.DTOs
{
    public class UpdateUserDto<TKey>
    {
        public TKey Id { get; set; } = default!;
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}
