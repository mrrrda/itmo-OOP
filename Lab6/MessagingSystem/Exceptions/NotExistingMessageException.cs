namespace MessagingSystem.Exceptions;

public class NotExistingMessageException : Exception
{
    public NotExistingMessageException()
        : base("Requested message does not exist")
    { }
}
