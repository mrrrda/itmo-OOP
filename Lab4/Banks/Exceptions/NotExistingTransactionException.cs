namespace Banks.Exceptions;

public class NotExistingTransactionException : Exception
{
    public NotExistingTransactionException()
        : base("Requested transaction does not exist")
    { }
}
