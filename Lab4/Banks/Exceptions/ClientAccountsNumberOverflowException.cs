namespace Banks.Exceptions;

public class ClientAccountsNumberOverflowException : Exception
{
    public ClientAccountsNumberOverflowException(string maxClientAccountsNumber)
        : base(string.Format("Max client accounts number {0} has been reached", maxClientAccountsNumber))
    { }
}
