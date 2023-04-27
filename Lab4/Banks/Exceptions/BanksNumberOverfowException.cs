namespace Banks.Exceptions;

public class BanksNumberOverfowException : Exception
{
    public BanksNumberOverfowException(string maxBanksNumber)
        : base(string.Format("Max banks number {0} has been reached", maxBanksNumber))
    { }
}
