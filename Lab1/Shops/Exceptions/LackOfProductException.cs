namespace Shops.Exceptions;

public class LackOfProductException : Exception
{
    public LackOfProductException(string productName)
        : base(string.Format("Product is not available: {0}", productName))
    { }
}
