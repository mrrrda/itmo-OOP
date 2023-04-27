namespace MessagingSystem.Exceptions;

public class NotExistingSubordinateException : Exception
{
    public NotExistingSubordinateException()
        : base("Requested subordinate does not exist")
    { }
}
