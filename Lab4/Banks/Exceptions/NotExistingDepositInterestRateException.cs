namespace Banks.Exceptions;

public class NotExistingDepositInterestRateException : Exception
{
    public NotExistingDepositInterestRateException()
        : base("Requested deposit interest rate does not exist")
    { }
}
