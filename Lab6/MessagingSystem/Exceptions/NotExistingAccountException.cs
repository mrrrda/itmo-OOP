namespace MessagingSystem.Exceptions;

public class NotExistingAccountException : Exception
{
    public NotExistingAccountException()
        : base("Requested account does not exist")
    { }
}
