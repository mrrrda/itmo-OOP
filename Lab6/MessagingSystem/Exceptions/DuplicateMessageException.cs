namespace MessagingSystem.Exceptions;

public class DuplicateMessageException : Exception
{
    public DuplicateMessageException()
        : base("Requested message already exists")
    { }
}
