namespace Banks.Exceptions;

public class UnavaliableOperationException : Exception
{
    public UnavaliableOperationException(string eMessage)
        : base(eMessage)
    { }
}
