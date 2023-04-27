using System.Collections.ObjectModel;

using Banks.Exceptions;
using Banks.NotificationServices;

namespace Banks;

public class CentralBank
{
    public static readonly int DefaultMaxBanksNumber = 1024;

    private static CentralBank? bank;

    private int nextBankId;

    private List<Bank> banks;
    private int _maxBanksNumber;

    private CentralBank()
    {
        nextBankId = 0;
        banks = new List<Bank>();

        _maxBanksNumber = DefaultMaxBanksNumber;
    }

    public int MaxBanksNumber
    {
        get => _maxBanksNumber;

        set
        {
            if (value < 0 || value < banks.Count)
                throw new ArgumentOutOfRangeException(nameof(value), "New max banks number must be positive and not exceed current banks count");

            _maxBanksNumber = value;
        }
    }

    public ReadOnlyCollection<Bank> Banks => banks.AsReadOnly();

    public static CentralBank Get()
    {
        bank ??= new CentralBank();

        return bank;
    }

    public Bank RegisterBank(
        string name,
        int maxClientsNumber,
        int maxClientAccountsNumber,
        NotificationService notificationService)
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Invalid bank name", nameof(name));

        if (maxClientsNumber < 0)
            throw new ArgumentOutOfRangeException(nameof(maxClientsNumber), "Max clients number must be positive");

        if (maxClientAccountsNumber < 0)
            throw new ArgumentOutOfRangeException(nameof(maxClientAccountsNumber), "Max clients accounts number must be positive");

        if (banks.Count >= MaxBanksNumber - 1)
            throw new BanksNumberOverfowException(MaxBanksNumber.ToString());

        if (notificationService is null)
            throw new ArgumentNullException(nameof(notificationService), "Invalid notification service");

        int bankId = GetNextBankId();

        var newBank = new Bank(bankId, name, maxClientsNumber, maxClientAccountsNumber, notificationService);
        banks.Add(newBank);

        return newBank;
    }

    public void ApplyTariffRatesInBanks(DateTime dateTo)
    {
        banks.ForEach(bank => bank.ApplyTariffRates(dateTo));
    }

    private int GetNextBankId()
    {
        return nextBankId++;
    }
}
