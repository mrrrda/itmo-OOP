namespace MessagingSystem.Exceptions;

public class InvalidSuperiorException : Exception
{
    public InvalidSuperiorException()
        : base("Requested employee cannot be assigned as superior")
    { }
}
