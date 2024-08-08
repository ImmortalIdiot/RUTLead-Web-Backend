namespace api.Exceptions
{
    public class UserAlreadyExistsException : Exception
    {
        public UserAlreadyExistsException(string message)
        : base(message)
        {
        }
    }
}
