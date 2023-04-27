namespace MessagingSystem.Exceptions;

public class ForbiddenOperationException : Exception
{
    public ForbiddenOperationException(string message)
        : base(message)
    { }
}
