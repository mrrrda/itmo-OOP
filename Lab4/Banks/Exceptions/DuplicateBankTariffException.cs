namespace Banks.Exceptions;

public class DuplicateBankTariffException : Exception
{
    public DuplicateBankTariffException(string eMessage)
        : base(eMessage)
    { }
}
