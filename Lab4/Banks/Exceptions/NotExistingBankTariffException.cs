namespace Banks.Exceptions;

public class NotExistingBankTariffException : Exception
{
    public NotExistingBankTariffException(string bankTariffName)
        : base(string.Format("Requested bank tariff {0} does not exist at current bank", bankTariffName))
    { }
}
