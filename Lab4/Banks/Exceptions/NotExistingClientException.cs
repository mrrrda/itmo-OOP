namespace Banks.Exceptions;

public class NotExistingClientException : Exception
{
    public NotExistingClientException()
        : base("Requested client is not registered in the current bank")
    { }
}
