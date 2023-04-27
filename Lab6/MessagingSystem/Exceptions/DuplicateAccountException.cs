namespace MessagingSystem.Exceptions;

public class DuplicateAccountException : Exception
{
    public DuplicateAccountException()
        : base("Requested account already exists")
    { }
}
