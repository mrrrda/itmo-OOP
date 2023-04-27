namespace MessagingSystem.Exceptions;

public class InvalidSubordinateException : Exception
{
    public InvalidSubordinateException()
        : base("Requested employee cannot be assigned as subordinate")
    { }
}
