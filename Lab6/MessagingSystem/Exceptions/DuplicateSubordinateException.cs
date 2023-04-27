namespace MessagingSystem.Exceptions;

public class DuplicateSubordinateException : Exception
{
    public DuplicateSubordinateException()
        : base("Requested subordinate already exists")
    { }
}
