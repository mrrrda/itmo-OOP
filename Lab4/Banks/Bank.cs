using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

using Banks.Accounts;
using Banks.Accounts.DataInitializers;
using Banks.Accounts.Proxies;
using Banks.BankActors;
using Banks.Exceptions;
using Banks.Models;
using Banks.NotificationServices;
using Banks.Transactions;

namespace Banks;

public class Bank
{
    private static readonly Regex RegexBankName = new Regex("^([a-zA-Zа-яА-Я\\d]+[\\s-]?)+$", RegexOptions.Compiled);

    private int nextClientId;
    private int nextAccountId;

    private NotificationService _notificationService;

    private Dictionary<int, TransactionHandler> bankAccounts;
    private Dictionary<int, Client> clients;
    private List<BankTariff> bankTariffs;
    private List<Client> notificationSubscribers;

    public Bank(
        int id,
        string name,
        int maxClientsNumber,
        int maxClientAccountsNumber,
        NotificationService notificationService)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id), "Bank id must be positive");

        if (string.IsNullOrEmpty(name) || !RegexBankName.IsMatch(name))
            throw new ArgumentException("Invalid bank name", nameof(name));

        if (maxClientsNumber < 0)
            throw new ArgumentOutOfRangeException(nameof(maxClientsNumber), "Max clients number must be positive");

        if (maxClientAccountsNumber < 0)
            throw new ArgumentOutOfRangeException(nameof(maxClientAccountsNumber), "Max clients accounts number must be positive");

        if (notificationService is null)
            throw new ArgumentNullException(nameof(notificationService), "Invalid notification service");

        DefaultBankTariff = GetDefaultTariff();

        bankAccounts = new Dictionary<int, TransactionHandler>();
        clients = new Dictionary<int, Client>();
        bankTariffs = new List<BankTariff>() { DefaultBankTariff };
        notificationSubscribers = new List<Client>();

        nextClientId = 0;
        nextAccountId = 0;

        Id = id;
        Name = name;

        MaxClientsNumber = maxClientsNumber;
        MaxClientAccountsNumber = maxClientAccountsNumber;

        _notificationService = notificationService;
    }

    public BankTariff DefaultBankTariff { get; }
    public int Id { get; }
    public string Name { get; }

    public int MaxClientsNumber { get; }
    public int MaxClientAccountsNumber { get; }

    public NotificationService NotificationService
    {
        get => _notificationService;

        set
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value), "Invalid notification service");

            _notificationService = value;
        }
    }

    public ReadOnlyCollection<TransactionHandler> BankAccounts => bankAccounts.Values.ToList<TransactionHandler>().AsReadOnly();
    public ReadOnlyCollection<BankActor> Clients => clients.Values.ToList<BankActor>().AsReadOnly();

    public ReadOnlyCollection<BankTariff> BankTariffs => bankTariffs.AsReadOnly();
    public ReadOnlyCollection<Client> NotificationSubscribers => notificationSubscribers.AsReadOnly();

    public Client RegisterClient(Client newClient)
    {
        if (newClient is null)
            throw new ArgumentNullException(nameof(newClient), "Invalid client");

        if (clients.ContainsValue(newClient))
        {
            throw new DuplicateClientException(
            string.Format(
                "Requested client {0} already exists",
                newClient.Id.ToString()));
        }

        if (clients.Count >= MaxClientsNumber - 1)
            throw new ClientsNumberOverflowException(MaxClientsNumber.ToString());

        newClient.Id = GetNextClientId();
        clients.Add(newClient.Id, newClient);

        return newClient;
    }

    public TransactionHandler RegisterAccount(
        Client client,
        BankAccountDataInitializer bankAccountDataInitializer,
        BankTariff bankTariff)
    {
        if (client is null)
            throw new ArgumentNullException(nameof(client), "Invalid client");

        if (!clients.ContainsValue(client))
            throw new NotExistingClientException();

        if (bankAccountDataInitializer is null)
            throw new ArgumentNullException(nameof(bankTariff), "Invalid bank account data initializer");

        if (bankTariff is null)
            throw new ArgumentNullException(nameof(bankTariff), "Invalid bank tariff");

        if (!bankTariffs.Contains(bankTariff))
            throw new NotExistingBankTariffException(bankTariff.Name);

        int nextAccountId = GetNextAccountId();
        TransactionHandler newBankAccount = CreateBankAccount(nextAccountId, client, bankTariff, bankAccountDataInitializer);

        if (client.BankAccounts.Count >= MaxClientAccountsNumber - 1)
            throw new ClientAccountsNumberOverflowException(MaxClientAccountsNumber.ToString());

        client.AddBankAccount(newBankAccount);
        bankAccounts.Add(newBankAccount.Id, newBankAccount);

        return newBankAccount;
    }

    public void RemoveClient(Client clientToRemove)
    {
        if (clientToRemove is null)
            throw new ArgumentNullException(nameof(clientToRemove), "Invalid client");

        if (!clients.ContainsValue(clientToRemove))
            throw new NotExistingClientException();

        foreach (TransactionHandler bankAccount in clientToRemove.BankAccounts)
            bankAccounts.Remove(bankAccount.Id);

        clients.Remove(clientToRemove.Id);
    }

    public void RemoveBankAccount(TransactionHandler bankAccountToRemove)
    {
        if (bankAccountToRemove is null)
            throw new ArgumentNullException(nameof(bankAccountToRemove), "Invalid bank account");

        if (!bankAccounts.ContainsValue(bankAccountToRemove))
            throw new NotExistingBankAccountException();

        bankAccountToRemove.Client.RemoveBankAccount(bankAccountToRemove);
        bankAccounts.Remove(bankAccountToRemove.Id);

        Notify(
            NotificationService,
            "Bank account has been removed",
            client => client.Equals(bankAccountToRemove.Client));
    }

    public void AddTariff(BankTariff newBankTariff)
    {
        if (newBankTariff is null)
            throw new ArgumentNullException(nameof(newBankTariff), "Invalid tariff");

        if (bankTariffs.Contains(newBankTariff))
            throw new DuplicateBankTariffException("Requested account already exists in current bank");

        bankTariffs.Add(newBankTariff);

        Notify(
            NotificationService,
            string.Format("New bank tariff {0} has been added", newBankTariff.Name),
            null);
    }

    public void RemoveTariff(BankTariff bankTariffToRemove)
    {
        if (bankTariffToRemove is null)
            throw new ArgumentNullException(nameof(bankTariffToRemove), "Invalid tariff");

        if (!bankTariffs.Contains(bankTariffToRemove))
            throw new NotExistingBankTariffException(bankTariffToRemove.Name);

        bankTariffs.Remove(bankTariffToRemove);

        bankAccounts.Values.ToList<TransactionHandler>()
            .Where(bankAccount => bankAccount.BankTariff?.Equals(bankTariffToRemove) ?? false)
            .ToList<TransactionHandler>()
            .ForEach(foundBankAccount =>
                {
                    foundBankAccount.BankTariff = DefaultBankTariff;
                    foundBankAccount.IsBlocked = true;
                });

        Notify(
            NotificationService,
            string.Format("Bank tariff {0} has been removed, account tariff is switched to default", bankTariffToRemove.Name),
            client => client.BankAccounts.Any(bankAccount => bankAccount.BankTariff?.Equals(bankTariffToRemove) ?? false));
    }

    public void ChangeBankAccountTariff(TransactionHandler bankAccount, BankTariff newBankTariff)
    {
        if (bankAccount is null)
            throw new ArgumentNullException(nameof(bankAccount), "Invalid bank account");

        if (newBankTariff is null)
            throw new ArgumentNullException(nameof(newBankTariff), "Invalid bank tariff");

        if (!bankTariffs.Contains(newBankTariff))
            throw new NotExistingBankTariffException(newBankTariff.Name);

        if (newBankTariff.Equals(bankAccount.BankTariff))
            throw new DuplicateBankTariffException("Impossible to change the bank tariff to the same tariff");

        bankAccount.BankTariff = newBankTariff;

        Notify(
            NotificationService,
            string.Format("Bank tariff has been changed to {0}", newBankTariff.Name),
            client => client.Equals(bankAccount.Client));
    }

    public void ChangeDepositInterestRateInTariff(
        BankTariff bankTariff,
        decimal lowerDepositBoundary,
        decimal upperDepositBoundary,
        decimal newInterestRate)
    {
        if (bankTariff is null)
            throw new ArgumentNullException(nameof(bankTariff), "Invalid bank tariff");

        bankTariff.ChangeDepositInterestRate(lowerDepositBoundary, upperDepositBoundary, newInterestRate);

        Notify(
            NotificationService,
            string.Format("Deposit interest rate at tariff {0} has been changed", bankTariff.Name),
            client => client.BankAccounts.Any(bankAccount => bankAccount.BankTariff?.Equals(bankTariff) ?? false));
    }

    public void RemoveDepositInterestRateFromTariff(
        BankTariff bankTariff,
        decimal lowerDepositBoundary,
        decimal upperDepositBoundary,
        decimal interestRate)
    {
        if (bankTariff is null)
            throw new ArgumentNullException(nameof(bankTariff), "Invalid bank tariff");

        if (bankTariff.DepositInterestRates.Count <= 1)
            throw new UnavaliableOperationException("Deposit interests rates must contain at least 1 rate");

        bankTariff.RemoveDepositInterestRate(lowerDepositBoundary, upperDepositBoundary, interestRate);

        Notify(
            NotificationService,
            string.Format("Deposit interest rate at tariff {0} has been removed", bankTariff.Name),
            client => client.BankAccounts.Any(bankAccount => bankAccount.BankTariff?.Equals(bankTariff) ?? false));
    }

    public void ChangeDebitInterestRateInTariff(BankTariff bankTariff, decimal newDebitIntrestRate)
    {
        if (bankTariff is null)
            throw new ArgumentNullException(nameof(bankTariff), "Invalid bank tariff");

        if (!bankTariffs.Contains(bankTariff))
            throw new NotExistingBankTariffException(bankTariff.Name);

        if (newDebitIntrestRate < 0)
            throw new ArgumentOutOfRangeException(nameof(newDebitIntrestRate), "Debit interest rate must be positive");

        bankTariff.DebitInterestRate = newDebitIntrestRate;

        Notify(
            NotificationService,
            string.Format("Debit interest rate at tariff {0} has been changed to {1}", bankTariff.Name, bankTariff.DebitInterestRate),
            client => client.BankAccounts.Any(bankAccount => bankAccount.BankTariff?.Equals(bankTariff) ?? false));
    }

    public void ChangeCreditConditionLowerBoundaryInTariff(BankTariff bankTariff, decimal newLowerBoundary)
    {
        if (bankTariff is null)
            throw new ArgumentNullException(nameof(bankTariff), "Invalid bank tariff");

        if (!bankTariffs.Contains(bankTariff))
            throw new NotExistingBankTariffException(bankTariff.Name);

        if (newLowerBoundary > 0)
            throw new ArgumentOutOfRangeException(nameof(newLowerBoundary), "Lower credit boundary must be negative");

        bankTariff.ChangeCreditConditionLowerBoundary(newLowerBoundary);

        Notify(
            NotificationService,
            string.Format("Credit lower boundary at tariff {0} has been changed to {1}", bankTariff.Name, bankTariff.CreditCondition.LowerCreditBoundary),
            client => client.BankAccounts.Any(bankAccount => bankAccount.BankTariff?.Equals(bankTariff) ?? false));
    }

    public void ChangeCreditConditionComissionInTariff(BankTariff bankTariff, decimal newComission)
    {
        if (bankTariff is null)
            throw new ArgumentNullException(nameof(bankTariff), "Invalid bank tariff");

        if (!bankTariffs.Contains(bankTariff))
            throw new NotExistingBankTariffException(bankTariff.Name);

        if (newComission < 0)
            throw new ArgumentOutOfRangeException(nameof(newComission), "Credit comission must be positive");

        bankTariff.ChangeCreditConditionComission(newComission);

        Notify(
            NotificationService,
            string.Format("Credit comission at tariff {0} has been changed to {1}", bankTariff.Name, bankTariff.CreditCondition.Comission),
            client => client.BankAccounts.Any(bankAccount => bankAccount.BankTariff?.Equals(bankTariff) ?? false));
    }

    public void ChangeUnverifiedClientTransactionLimitInTariff(BankTariff bankTariff, decimal newUnverifiedClientTransactionLimit)
    {
        if (bankTariff is null)
            throw new ArgumentNullException(nameof(bankTariff), "Invalid bank tariff");

        if (!bankTariffs.Contains(bankTariff))
            throw new NotExistingBankTariffException(bankTariff.Name);

        if (newUnverifiedClientTransactionLimit < 0)
            throw new ArgumentOutOfRangeException(nameof(newUnverifiedClientTransactionLimit), "Unverified client transaction limit must be positive");

        bankTariff.UnverifiedClientTransactionLimit = newUnverifiedClientTransactionLimit;

        Notify(
            NotificationService,
            string.Format("Unverified client transaction limit at tariff {0} has been changed to {1}", bankTariff.Name, bankTariff.UnverifiedClientTransactionLimit),
            client => client.BankAccounts.Any(bankAccount => bankAccount.BankTariff?.Equals(bankTariff) ?? false));
    }

    public void AddSubscribtion(Client client)
    {
        if (client is null)
            throw new ArgumentNullException(nameof(client), "Invalid client");

        if (!clients.ContainsValue(client))
            throw new NotExistingClientException();

        if (notificationSubscribers.Contains(client))
            throw new DuplicateClientException(string.Format("Resuested client {0} is already subscribed"));

        notificationSubscribers.Add(client);
    }

    public void RemoveSubscribtion(Client client)
    {
        if (client is null)
            throw new ArgumentNullException(nameof(client), "Invalid client");

        if (!clients.ContainsValue(client))
            throw new NotExistingClientException();

        notificationSubscribers.Remove(client);
    }

    public void Notify(NotificationService notificationService, string notification, Func<Client, bool>? shouldNotify)
    {
        if (notificationService is null)
            throw new ArgumentNullException(nameof(notificationService), "Invalid notification service");

        notificationSubscribers.ForEach(client =>
        {
            if (shouldNotify is null || shouldNotify(client))
                notificationService.Notify(client, notification);
        });
    }

    public void DoTransaction(Transaction transaction)
    {
        if (!bankAccounts.ContainsValue(transaction.BankAccount))
            throw new NotExistingBankAccountException();

        if (transaction is null)
            throw new ArgumentNullException(nameof(transaction), "Invalid transaction");

        transaction.BankAccount.DoTransaction(transaction);

        Notify(
            NotificationService,
            "Transaction has been finished",
            client => client.Equals(transaction.BankAccount.Client));
    }

    public void UndoTransaction(Transaction transaction)
    {
        if (transaction is null)
            throw new ArgumentNullException(nameof(transaction), "Invalid transaction");

        if (!bankAccounts.ContainsValue(transaction.BankAccount))
            throw new NotExistingBankAccountException();

        transaction.BankAccount.UndoTransaction(transaction);

        Notify(
            NotificationService,
            "Refund transaction has been finished",
            client => client.Equals(transaction.BankAccount.Client));
    }

    internal void ApplyTariffRates(DateTime dateTo)
    {
        if (DateTime.Compare(DateTime.Now, dateTo) > 0)
            throw new ArgumentException("DateTo must be greater that current date", nameof(dateTo));

        foreach (TransactionHandler bankAccount in bankAccounts.Values.ToList<TransactionHandler>())
        {
            decimal rate = bankAccount.CalculateInterest(dateTo);

            bankAccount.Balance += rate;
        }
    }

    protected TransactionHandler CreateBankAccount(int bankId, Client client, BankTariff bankTariff, BankAccountDataInitializer bankAccountDataInitializer)
    {
        switch (bankAccountDataInitializer)
        {
            case DebitAccountDataInitializer:
                return new DebitAccountProxy(bankId, client, this, bankTariff, (DebitAccountDataInitializer)bankAccountDataInitializer);

            case DepositAccountDataInitializer:
                return new DepositAccountProxy(bankId, client, this, bankTariff, (DepositAccountDataInitializer)bankAccountDataInitializer);

            case CreditAccountDataInitializer:
                return new CreditAccountProxy(bankId, client, this, bankTariff, (CreditAccountDataInitializer)bankAccountDataInitializer);

            default:
                throw new ArgumentException("Invalid bank account initializer", nameof(bankAccountDataInitializer));
        }
    }

    private BankTariff GetDefaultTariff()
    {
        var defaultBankTariffDepositInterestRates = new List<DepositInterestRate>()
            {
                new DepositInterestRate(0M, 50000M, 0.01M),
                new DepositInterestRate(50000M, 100000M, 0.03M),
                new DepositInterestRate(100000M, 200000M, 0.04M),
            };

        decimal defautBankTariffDebitInterestRate = 0.03M;

        var defautBankTariffCreditCondition = new CreditCondition(-50000, 0.03M);

        decimal defautBankTariffUnverifiedClientTransactionLimit = 100000M;

        return new BankTariff(
            "Default tariff",
            defaultBankTariffDepositInterestRates,
            defautBankTariffDebitInterestRate,
            defautBankTariffCreditCondition,
            defautBankTariffUnverifiedClientTransactionLimit);
    }

    private int GetNextClientId()
    {
        return nextClientId++;
    }

    private int GetNextAccountId()
    {
        return nextAccountId++;
    }
}
