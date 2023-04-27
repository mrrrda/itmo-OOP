namespace Banks.Exceptions;

public class DuplicateClientException : Exception
{
    public DuplicateClientException(string eMessage)
        : base(eMessage)
    { }
}
