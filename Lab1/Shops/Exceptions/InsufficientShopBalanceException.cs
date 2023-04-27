namespace Shops.Exceptions;

public class InsufficientShopBalanceException : Exception
{
    public InsufficientShopBalanceException()
        : base("Insufficient shop balance")
    { }
}
