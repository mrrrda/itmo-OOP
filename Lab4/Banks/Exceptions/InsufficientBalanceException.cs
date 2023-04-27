namespace Banks.Exceptions;

public class InsufficientBalanceException : Exception
{
    public InsufficientBalanceException()
        : base("Insufficient balance")
    { }
}
