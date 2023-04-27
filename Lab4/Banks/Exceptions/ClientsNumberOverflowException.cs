namespace Banks.Exceptions;

public class ClientsNumberOverflowException : Exception
{
    public ClientsNumberOverflowException(string maxClientsNumber)
        : base(string.Format("Max clients number {0} in current bank has been reached", maxClientsNumber))
    { }
}
