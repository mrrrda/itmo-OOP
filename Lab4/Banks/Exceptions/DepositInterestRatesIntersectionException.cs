namespace Banks.Exceptions;

public class DepositInterestRatesIntersectionException : Exception
{
    public DepositInterestRatesIntersectionException()
        : base("Deposit boundaries cannot intersect")
    { }
}
