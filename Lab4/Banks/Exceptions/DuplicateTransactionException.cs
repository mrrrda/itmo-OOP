namespace Banks.Exceptions;

public class DuplicateTransactionException : Exception
{
    public DuplicateTransactionException()
        : base("Requested transaction already exists")
    { }
}
