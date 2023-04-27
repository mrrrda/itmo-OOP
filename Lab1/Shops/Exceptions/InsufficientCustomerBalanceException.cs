namespace Shops.Exceptions;

public class InsufficientCustomerBalanceException : Exception
{
    public InsufficientCustomerBalanceException()
        : base("Insufficient customer balance")
    { }
}
