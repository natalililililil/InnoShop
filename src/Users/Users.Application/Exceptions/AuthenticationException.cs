namespace Users.Application.Exceptions
{
    public class AuthenticationException : Exception
    {
        public int StatusCode { get; } = 401;

        public AuthenticationException(string message) : base(message) { }
    }
}