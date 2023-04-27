using Banks.Accounts;
using Banks.Accounts.DataInitializers;
using Banks.BankActors;
using Banks.Exceptions;
using Banks.Models;
using Banks.NotificationServices;
using Banks.Transactions;

using Xunit;

namespace Banks.Test;

public class BanksTest
{
    private int maxBanksNumber;
    private int maxClientsNumber;
    private int maxClientAccountsNumber;

    private CentralBank centralBank;
    private NotificationService notificationService;

    private ClientBuilder clientBuilder;

    public BanksTest()
    {
        maxBanksNumber = 5096;
        maxClientsNumber = 10;
        maxClientAccountsNumber = 5;

        centralBank = CentralBank.Get();
        centralBank.MaxBanksNumber = maxBanksNumber;

        notificationService = new ConsoleNotificationService();

        clientBuilder = new ClientBuilder();
    }

    [Fact]
    public void CreateBank_BankIsCreated()
    {
        Bank newBank = centralBank.RegisterBank("Bank", 10, 10, notificationService);

        Assert.NotNull(newBank);
        Assert.Contains(newBank, centralBank.Banks);
    }

    [Theory]
    [InlineData("669026", "Russia", "Saint-Petersburg", "Mayakovskaya", 1, "5A")]
    public void BuildAddress_AddressInfoIsFilledCorrectly(
        string zip,
        string country,
        string city,
        string street,
        int streetNumber,
        string? building = null)
    {
        Address newAddress = GetAddress(zip, country, city, street, streetNumber, building);

        Assert.NotNull(newAddress);

        Assert.Equal(zip, newAddress.Zip);
        Assert.Equal(country, newAddress.Country);
        Assert.Equal(city, newAddress.City);
        Assert.Equal(street, newAddress.Street);
        Assert.Equal(streetNumber, newAddress.StreetNumber);
        Assert.Equal(building, newAddress.Building);
    }

    [Theory]
    [InlineData("Mariya", "Izerakova")]
    [InlineData("Nikita", "Hoffman")]
    public void BuildUnverifiedClient_ClientInfoIsFilledCorrectly(
        string name,
        string lastname)
    {
        Client newClient = GetUnverifiedClient(name, lastname);

        Assert.NotNull(newClient);

        Assert.Equal(name, newClient.Name);
        Assert.Equal(lastname, newClient.Lastname);
    }

    [Fact]
    public void BuildVerifiedClient_ClientInfoIsFilledCorrectly()
    {
        string name = "Mariya";
        string lastname = "Izerakova";
        string passportId = "5000669026";

        Address address = GetAddress("669026", "Russia", "Saint-Petersburg", "Mayakovskaya", 1, "5A");
        Client newClient = GetVerifiedClient(name, lastname, address, passportId);

        Assert.NotNull(newClient);

        Assert.Equal(name, newClient.Name);
        Assert.Equal(lastname, newClient.Lastname);
        Assert.Equal(address, newClient.Address);
        Assert.Equal(passportId, newClient.PassportId);
    }

    [Fact]
    public void AddBankTariff_TariffIsAdded()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        BankTariff bankTariff = GetBankTariff("Tariff", 0M, 100000M, 3M, 3M, -100000M, 3M, 50000M);
        bank.AddTariff(bankTariff);

        Assert.Equal(2, bank.BankTariffs.Count);
        Assert.Contains(bankTariff, bank.BankTariffs);
    }

    [Fact]
    public void RemoveBankTariff_TariffIsRemoved()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        BankTariff bankTariff = GetBankTariff("Tariff", 0M, 100000M, 3M, 3M, -100000M, 3M, 50000M);
        bank.AddTariff(bankTariff);

        Client client = GetUnverifiedClient("Mariya", "Izerakova");
        bank.RegisterClient(client);

        var bankAccountDataInitializer = new CreditAccountDataInitializer();
        TransactionHandler bankAccount = bank.RegisterAccount(client, bankAccountDataInitializer, bankTariff);

        Assert.NotNull(bankTariff);

        bank.RemoveTariff(bankTariff);

        Assert.Single(bank.BankTariffs);
        Assert.NotNull(bankAccount.BankTariff);
    }

    [Fact]
    public void AddBankTariff_ThrowsDuplicateTariffException()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        BankTariff bankTariff = GetBankTariff("Tariff", 0M, 100000M, 3M, 3M, -100000M, 3M, 50000M);
        bank.AddTariff(bankTariff);

        Assert.Throws<DuplicateBankTariffException>(() => bank.AddTariff(bankTariff));
    }

    [Fact]
    public void RemoveBankTariff_ThrowsNotExistingTariffException()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        BankTariff bankTariff = GetBankTariff("Tariff", 0M, 100000M, 3M, 3M, -100000M, 3M, 50000M);

        Assert.Throws<NotExistingBankTariffException>(() => bank.RemoveTariff(bankTariff));
    }

    [Fact]
    public void RegisterClient_ClientIsRegistered()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        Client newClient = GetUnverifiedClient("Mariya", "Izerakova");
        bank.RegisterClient(newClient);

        Assert.Single(bank.Clients);
        Assert.Contains(newClient, bank.Clients);
    }

    [Fact]
    public void Removelient_ClientIsRemoved()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        BankTariff bankTariff = GetBankTariff("Tariff", 0M, 100000M, 3M, 3M, -100000M, 3M, 50000M);
        bank.AddTariff(bankTariff);

        Client clientToRemove = GetUnverifiedClient("Mariya", "Izerakova");
        bank.RegisterClient(clientToRemove);

        var bankAccountDataInitializer = new CreditAccountDataInitializer();
        TransactionHandler bankAccount = bank.RegisterAccount(clientToRemove, bankAccountDataInitializer, bankTariff);

        bank.RemoveClient(clientToRemove);

        Assert.DoesNotContain(clientToRemove, bank.Clients);
        Assert.DoesNotContain(bankAccount, bank.BankAccounts);
    }

    [Fact]
    public void Removelient_ThrowsNotExistingClientException()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        Client clientToRemove = GetUnverifiedClient("Mariya", "Izerakova");

        Assert.Throws<NotExistingClientException>(() => bank.RemoveClient(clientToRemove));
    }

    [Fact]
    public void RegisterClient_ThrowsDuplicateClientException()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        Client newClient = GetUnverifiedClient("Mariya", "Izerakova");
        bank.RegisterClient(newClient);

        Assert.Throws<DuplicateClientException>(() => bank.RegisterClient(newClient));
    }

    [Fact]
    public void RegisterBankAccount_AccountIsRegistered()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        BankTariff bankTariff = GetBankTariff("Tariff", 0M, 100000M, 3M, 3M, -100000M, 3M, 50000M);
        bank.AddTariff(bankTariff);

        Client client = GetUnverifiedClient("Mariya", "Izerakova");
        bank.RegisterClient(client);

        var firstBankAccountDataInitializer = new CreditAccountDataInitializer();
        TransactionHandler firstBankAccount = bank.RegisterAccount(client, firstBankAccountDataInitializer, bankTariff);

        Assert.NotNull(firstBankAccount.BankTariff);

        Assert.Single(bank.BankAccounts);
        Assert.Single(client.BankAccounts);

        var secondBankAccountDataInitializer = new DebitAccountDataInitializer();
        TransactionHandler secondBankAccount = bank.RegisterAccount(client, secondBankAccountDataInitializer, bankTariff);

        Assert.NotNull(secondBankAccount.BankTariff);

        Assert.Equal(2, bank.BankAccounts.Count);
        Assert.Contains(firstBankAccount, bank.BankAccounts);
        Assert.Contains(secondBankAccount, bank.BankAccounts);

        Assert.Equal(2, client.BankAccounts.Count);
        Assert.Contains(firstBankAccount, client.BankAccounts);
        Assert.Contains(secondBankAccount, client.BankAccounts);
    }

    [Fact]
    public void RegisterBankAccount_ThrowsClientAccountsNumberOverflowException()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        BankTariff bankTariff = GetBankTariff("Tariff", 0M, 100000M, 3M, 3M, -100000M, 3M, 50000M);
        bank.AddTariff(bankTariff);

        Client client = GetUnverifiedClient("Mariya", "Izerakova");
        bank.RegisterClient(client);

        var bankAccountDataInitializer = new CreditAccountDataInitializer();

        for (int i = 0; i < maxClientAccountsNumber - 1; i++)
            bank.RegisterAccount(client, bankAccountDataInitializer, bankTariff);

        Assert.Throws<ClientAccountsNumberOverflowException>(() => bank.RegisterAccount(client, bankAccountDataInitializer, bankTariff));
    }

    [Fact]
    public void ChangeBankAccountTariff_TariffIsChanged()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        BankTariff firstBankTariff = GetBankTariff("Tariff1", 0M, 100000M, 3M, 3M, -100000M, 3M, 50000M);
        bank.AddTariff(firstBankTariff);

        BankTariff secondBankTariff = GetBankTariff("Tariff2", 0M, 100000M, 3M, 3M, -100000M, 3M, 50000M);
        bank.AddTariff(secondBankTariff);

        Client client = GetUnverifiedClient("Mariya", "Izerakova");
        bank.RegisterClient(client);

        var bankAccountDataInitializer = new CreditAccountDataInitializer();
        TransactionHandler bankAccount = bank.RegisterAccount(client, bankAccountDataInitializer, firstBankTariff);

        bank.ChangeBankAccountTariff(bankAccount, secondBankTariff);

        Assert.NotNull(bankAccount.BankTariff);
        Assert.Equal(secondBankTariff, bankAccount.BankTariff);
    }

    [Fact]
    public void ChangeBankAccountTariff_ThrowsNotExistingTariffException()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        BankTariff firstBankTariff = GetBankTariff("Tariff1", 0M, 100000M, 3M, 3M, -100000M, 3M, 50000M);
        bank.AddTariff(firstBankTariff);

        BankTariff secondBankTariff = GetBankTariff("Tariff2", 0M, 100000M, 3M, 3M, -100000M, 3M, 50000M);

        Client client = GetUnverifiedClient("Mariya", "Izerakova");
        bank.RegisterClient(client);

        var bankAccountDataInitializer = new CreditAccountDataInitializer();
        TransactionHandler bankAccount = bank.RegisterAccount(client, bankAccountDataInitializer, firstBankTariff);

        Assert.Throws<NotExistingBankTariffException>(() => bank.ChangeBankAccountTariff(bankAccount, secondBankTariff));
    }

    [Fact]
    public void ChangeBankAccountTariff_ThrowsDuplicateTariffException()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        BankTariff bankTariff = GetBankTariff("Tariff1", 0M, 100000M, 3M, 3M, -100000M, 3M, 50000M);
        bank.AddTariff(bankTariff);

        Client client = GetUnverifiedClient("Mariya", "Izerakova");
        bank.RegisterClient(client);

        var bankAccountDataInitializer = new CreditAccountDataInitializer();
        TransactionHandler bankAccount = bank.RegisterAccount(client, bankAccountDataInitializer, bankTariff);

        Assert.Throws<DuplicateBankTariffException>(() => bank.ChangeBankAccountTariff(bankAccount, bankTariff));
    }

    [Fact]
    public void RemoveBankAccount_AccountIsRemoved()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        BankTariff bankTariff = GetBankTariff("Tariff", 0M, 100000M, 3M, 3M, -100000M, 3M, 50000M);
        bank.AddTariff(bankTariff);

        Client client = GetUnverifiedClient("Mariya", "Izerakova");
        bank.RegisterClient(client);

        var bankAccountDataInitializer = new CreditAccountDataInitializer();
        TransactionHandler bankAccount = bank.RegisterAccount(client, bankAccountDataInitializer, bankTariff);

        bank.RemoveBankAccount(bankAccount);

        Assert.Empty(client.BankAccounts);
        Assert.Empty(bank.BankAccounts);
    }

    [Fact]
    public void ChangeCreditConditionLowerBoundaryInTariff_BounaryIsChanged()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        BankTariff bankTariff = GetBankTariff("Tariff", 0M, 100000M, 3M, 3M, -100000M, 3M, 50000M);
        bank.AddTariff(bankTariff);

        decimal newLowerBoundary = 0M;
        bank.ChangeCreditConditionLowerBoundaryInTariff(bank.BankTariffs[1], newLowerBoundary);

        Assert.Equal(newLowerBoundary, bankTariff.CreditCondition.LowerCreditBoundary);
    }

    [Fact]
    public void ChangeCreditConditionComissionInTariff_ComissionIsChanged()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        BankTariff bankTariff = GetBankTariff("Tariff", 0M, 100000M, 3M, 3M, -100000M, 3M, 50000M);
        bank.AddTariff(bankTariff);

        decimal newComission = 4M;
        bank.ChangeCreditConditionComissionInTariff(bank.BankTariffs[1], newComission);

        Assert.Equal(newComission, bankTariff.CreditCondition.Comission);
    }

    [Fact]
    public void ChangeDebitInterestRateInTariff_RateIsChanged()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        BankTariff bankTariff = GetBankTariff("Tariff", 0M, 100000M, 3M, 3M, -100000M, 3M, 50000M);
        bank.AddTariff(bankTariff);

        decimal newInterestRate = 4M;
        bank.ChangeDebitInterestRateInTariff(bank.BankTariffs[1], newInterestRate);

        Assert.Equal(newInterestRate, bankTariff.DebitInterestRate);
    }

    [Fact]
    public void ChangeDepositInterestRateInTariff_RateIsChanged()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        decimal lowerDepositBoundary = 0M;
        decimal upperDepositBoundary = 100000M;

        BankTariff bankTariff = GetBankTariff("Tariff", lowerDepositBoundary, upperDepositBoundary, 3M, 3M, -100000M, 3M, 50000M);
        bank.AddTariff(bankTariff);

        decimal newInterestRate = 1M;

        bank.ChangeDepositInterestRateInTariff(bank.BankTariffs[1], lowerDepositBoundary, upperDepositBoundary, newInterestRate);

        Assert.Equal(newInterestRate, bankTariff.DepositInterestRates[0].InterestRate);
    }

    [Fact]
    public void RemoveDepositInterestRateFromTariff_RateIsRemoved()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        var depositInterestRateToRemove = new DepositInterestRate(20M, 30M, 2);

        var depositInterestRates = new List<DepositInterestRate>()
            {
                new DepositInterestRate(10M, 20M, 1),
                depositInterestRateToRemove,
                new DepositInterestRate(30M, 40M, 3),
                new DepositInterestRate(50M, 80M, 4),
            };

        var creditCondition = new CreditCondition(0M, 5M);

        var bankTariff = new BankTariff("Tariff", depositInterestRates, 5M, creditCondition, 10000M);
        bank.AddTariff(bankTariff);

        bank.RemoveDepositInterestRateFromTariff(bankTariff, 20M, 30M, 2);

        Assert.Equal(7, bankTariff.DepositInterestRates.Count);
        Assert.DoesNotContain(depositInterestRateToRemove, bankTariff.DepositInterestRates);
        Assert.Equal(1, bankTariff.DepositInterestRates[6].InterestRate);
    }

    [Fact]
    public void RemoveDepositInterestRateFromTariff_ThrowsNotExistingRateException()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        var depositInterestRates = new List<DepositInterestRate>()
            {
                new DepositInterestRate(10M, 20M, 1),
                new DepositInterestRate(20M, 30M, 1),
                new DepositInterestRate(30M, 40M, 1),
                new DepositInterestRate(50M, 80M, 1),
            };

        var creditCondition = new CreditCondition(0M, 5M);

        var bankTariff = new BankTariff("Tariff", depositInterestRates, 5M, creditCondition, 10000M);
        bank.AddTariff(bankTariff);

        Assert.Throws<NotExistingDepositInterestRateException>(() => bank.RemoveDepositInterestRateFromTariff(bankTariff, 20M, 25M, 1));
    }

    [Fact]
    public void RemoveDepositInterestRateFromTariff_ThrowsUnavaliableOperationException()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        decimal lowerDepositBoundary = DepositInterestRate.MinLowerBoundary;
        decimal upperDepositBoundary = DepositInterestRate.MaxUpperBoundary;

        decimal interestRate = 4M;

        BankTariff bankTariff = GetBankTariff("Tariff", lowerDepositBoundary, upperDepositBoundary, interestRate, 3M, -100000M, 3M, 50000M);
        bank.AddTariff(bankTariff);

        Assert.Throws<UnavaliableOperationException>(() =>
            bank.RemoveDepositInterestRateFromTariff(bankTariff, lowerDepositBoundary, upperDepositBoundary, interestRate));
    }

    [Fact]
    public void ChangeUnverifiedClientTransactionLimitInTariff()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        decimal firstUnverifiedClientTransactionLimit = 10000M;
        decimal secondUnverifiedClientTransactionLimit = 10000M;

        BankTariff bankTariff = GetBankTariff("Tariff", 0M, 10M, 5M, 3M, -100000M, 3M, firstUnverifiedClientTransactionLimit);
        bank.AddTariff(bankTariff);

        bank.ChangeUnverifiedClientTransactionLimitInTariff(bankTariff, secondUnverifiedClientTransactionLimit);

        Assert.Equal(secondUnverifiedClientTransactionLimit, bankTariff.UnverifiedClientTransactionLimit);
    }

    [Fact]
    public void AddNotificationSubscriber_SubscriberIsAdded()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        Client firstClient = GetUnverifiedClient("Mariya", "Izerakova");
        bank.RegisterClient(firstClient);

        Client secondClient = GetUnverifiedClient("Nikita", "Hoffman");
        bank.RegisterClient(secondClient);

        bank.AddSubscribtion(firstClient);
        bank.AddSubscribtion(secondClient);

        Assert.Equal(2, bank.NotificationSubscribers.Count);
        Assert.Contains(firstClient, bank.NotificationSubscribers);
        Assert.Contains(secondClient, bank.NotificationSubscribers);
    }

    [Fact]
    public void RemoveNotificationSubscriber_SubscriberRemoved()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        Client firstClient = GetUnverifiedClient("Mariya", "Izerakova");
        bank.RegisterClient(firstClient);

        Client secondClient = GetUnverifiedClient("Nikita", "Hoffman");
        bank.RegisterClient(secondClient);

        bank.AddSubscribtion(firstClient);
        bank.AddSubscribtion(secondClient);

        bank.RemoveSubscribtion(secondClient);

        Assert.Single(bank.NotificationSubscribers);
        Assert.Contains(firstClient, bank.NotificationSubscribers);
        Assert.DoesNotContain(secondClient, bank.NotificationSubscribers);
    }

    [Fact]
    public void RemoveNotificationSubscriber_ThrowsNotExistingClientException()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        Client client = GetUnverifiedClient("Mariya", "Izerakova");

        Assert.Throws<NotExistingClientException>(() => bank.RemoveSubscribtion(client));
    }

    [Fact]
    public void DoDepositTransaction_TransactionFinished()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        BankTariff bankTariff = GetBankTariff("Tariff1", 0M, 100000M, 3M, 3M, -100000M, 3M, 50000M);
        bank.AddTariff(bankTariff);

        Client client = GetUnverifiedClient("Mariya", "Izerakova");
        bank.RegisterClient(client);

        var bankAccountDataInitializer = new DebitAccountDataInitializer();
        TransactionHandler bankAccount = bank.RegisterAccount(client, bankAccountDataInitializer, bankTariff);

        decimal firstTransactionAmount = 5000;
        var firstTransaction = new DepositTransaction(bankAccount, firstTransactionAmount);

        bank.DoTransaction(firstTransaction);

        Assert.Single(bankAccount.Transactions);
        Assert.Contains(firstTransaction, bankAccount.Transactions);

        Assert.Equal(firstTransactionAmount, bankAccount.Balance);

        decimal secondTransactionAmount = 10000;
        var secondTransaction = new DepositTransaction(bankAccount, secondTransactionAmount);

        bank.DoTransaction(secondTransaction);

        Assert.Equal(2, bankAccount.Transactions.Count);
        Assert.Contains(secondTransaction, bankAccount.Transactions);

        Assert.Equal(firstTransactionAmount + secondTransactionAmount, bankAccount.Balance);
    }

    [Fact]
    public void UndoDepositTransaction_TransactionFinished()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        BankTariff bankTariff = GetBankTariff("Tariff1", 0M, 100000M, 3M, 3M, -100000M, 3M, 50000M);
        bank.AddTariff(bankTariff);

        Client client = GetUnverifiedClient("Mariya", "Izerakova");
        bank.RegisterClient(client);

        var bankAccountDataInitializer = new DebitAccountDataInitializer();
        TransactionHandler bankAccount = bank.RegisterAccount(client, bankAccountDataInitializer, bankTariff);

        var transaction = new DepositTransaction(bankAccount, 5000);

        bank.DoTransaction(transaction);
        bank.UndoTransaction(transaction);

        Assert.Empty(bankAccount.Transactions);
        Assert.Equal(0, bankAccount.Balance);
    }

    [Fact]
    public void DoWithdrawTransaction_TransactionFinished()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        BankTariff bankTariff = GetBankTariff("Tariff1", 0M, 100000M, 3M, 3M, -100000M, 3M, 50000M);
        bank.AddTariff(bankTariff);

        Client client = GetUnverifiedClient("Mariya", "Izerakova");
        bank.RegisterClient(client);

        var bankAccountDataInitializer = new DebitAccountDataInitializer();
        TransactionHandler bankAccount = bank.RegisterAccount(client, bankAccountDataInitializer, bankTariff);

        decimal firstTransactionAmount = 5000;
        var firstTransaction = new DepositTransaction(bankAccount, firstTransactionAmount);

        bank.DoTransaction(firstTransaction);

        Assert.Single(bankAccount.Transactions);
        Assert.Contains(firstTransaction, bankAccount.Transactions);

        Assert.Equal(firstTransactionAmount, bankAccount.Balance);

        decimal secondTransactionAmount = 1000;
        var secondTransaction = new WithdrawTransaction(bankAccount, secondTransactionAmount);

        bank.DoTransaction(secondTransaction);

        Assert.Equal(2, bankAccount.Transactions.Count);
        Assert.Contains(secondTransaction, bankAccount.Transactions);

        Assert.Equal(firstTransactionAmount - secondTransactionAmount, bankAccount.Balance);
    }

    [Fact]
    public void DoWithdrawTransaction_ThrowsInsufficientBalanceException()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        BankTariff bankTariff = GetBankTariff("Tariff1", 0M, 100000M, 3M, 3M, -100000M, 3M, 50000M);
        bank.AddTariff(bankTariff);

        Client client = GetUnverifiedClient("Mariya", "Izerakova");
        bank.RegisterClient(client);

        var bankAccountDataInitializer = new DebitAccountDataInitializer();
        TransactionHandler bankAccount = bank.RegisterAccount(client, bankAccountDataInitializer, bankTariff);

        decimal firstTransactionAmount = 5000;
        var firstTransaction = new DepositTransaction(bankAccount, firstTransactionAmount);

        bank.DoTransaction(firstTransaction);

        Assert.Single(bankAccount.Transactions);
        Assert.Contains(firstTransaction, bankAccount.Transactions);

        Assert.Equal(firstTransactionAmount, bankAccount.Balance);

        decimal secondTransactionAmount = 10000;
        var secondTransaction = new WithdrawTransaction(bankAccount, secondTransactionAmount);

        Assert.Throws<InsufficientBalanceException>(() => bank.DoTransaction(secondTransaction));
    }

    [Fact]
    public void UndoWithdrawTransaction_TransactionFinished()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        BankTariff bankTariff = GetBankTariff("Tariff1", 0M, 100000M, 3M, 3M, -100000M, 3M, 50000M);
        bank.AddTariff(bankTariff);

        Client client = GetUnverifiedClient("Mariya", "Izerakova");
        bank.RegisterClient(client);

        var bankAccountDataInitializer = new DebitAccountDataInitializer();
        TransactionHandler bankAccount = bank.RegisterAccount(client, bankAccountDataInitializer, bankTariff);

        decimal firstTransactionAmount = 5000;
        var firstTransaction = new DepositTransaction(bankAccount, firstTransactionAmount);

        bank.DoTransaction(firstTransaction);

        Assert.Single(bankAccount.Transactions);
        Assert.Contains(firstTransaction, bankAccount.Transactions);

        Assert.Equal(firstTransactionAmount, bankAccount.Balance);

        decimal secondTransactionAmount = 1000;
        var secondTransaction = new WithdrawTransaction(bankAccount, secondTransactionAmount);

        bank.DoTransaction(secondTransaction);
        bank.UndoTransaction(secondTransaction);

        Assert.Single(bankAccount.Transactions);
        Assert.Equal(firstTransactionAmount, bankAccount.Balance);
    }

    [Fact]
    public void DoTransferTransaction_TransactionFinished()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        BankTariff bankTariff = GetBankTariff("Tariff1", 0M, 100000M, 3M, 3M, -100000M, 3M, 50000M);
        bank.AddTariff(bankTariff);

        Client client = GetUnverifiedClient("Mariya", "Izerakova");
        bank.RegisterClient(client);

        var firstBankAccountDataInitializer = new DebitAccountDataInitializer();
        TransactionHandler firstBankAccount = bank.RegisterAccount(client, firstBankAccountDataInitializer, bankTariff);

        var secondBankAccountDataInitializer = new DebitAccountDataInitializer();
        TransactionHandler secondBankAccount = bank.RegisterAccount(client, secondBankAccountDataInitializer, bankTariff);

        decimal firstTransactionAmount = 5000;
        var firstTransaction = new DepositTransaction(firstBankAccount, firstTransactionAmount);

        bank.DoTransaction(firstTransaction);

        decimal secondTransactionAmount = 1000;
        var secondTransaction = new TransferTransaction(firstBankAccount, secondBankAccount, secondTransactionAmount);

        bank.DoTransaction(secondTransaction);

        Assert.Equal(2, firstBankAccount.Transactions.Count);
        Assert.Single(secondBankAccount.Transactions);

        Assert.Equal(firstTransactionAmount - secondTransactionAmount, firstBankAccount.Balance);
        Assert.Equal(secondTransactionAmount, secondBankAccount.Balance);
    }

    [Fact]
    public void DoTransferTransaction_ThrowsInsufficientBalanceException()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        BankTariff bankTariff = GetBankTariff("Tariff1", 0M, 100000M, 3M, 3M, -100000M, 3M, 50000M);
        bank.AddTariff(bankTariff);

        Client client = GetUnverifiedClient("Mariya", "Izerakova");
        bank.RegisterClient(client);

        var firstBankAccountDataInitializer = new DebitAccountDataInitializer();
        TransactionHandler firstBankAccount = bank.RegisterAccount(client, firstBankAccountDataInitializer, bankTariff);

        var secondBankAccountDataInitializer = new DebitAccountDataInitializer();
        TransactionHandler secondBankAccount = bank.RegisterAccount(client, secondBankAccountDataInitializer, bankTariff);

        decimal firstTransactionAmount = 5000;
        var firstTransaction = new DepositTransaction(firstBankAccount, firstTransactionAmount);

        bank.DoTransaction(firstTransaction);

        decimal secondTransactionAmount = 6000;
        var secondTransaction = new TransferTransaction(firstBankAccount, secondBankAccount, secondTransactionAmount);

        Assert.Throws<InsufficientBalanceException>(() => bank.DoTransaction(secondTransaction));
    }

    [Fact]
    public void UndoTransferTransaction_TransactionFinished()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        BankTariff bankTariff = GetBankTariff("Tariff1", 0M, 100000M, 3M, 3M, -100000M, 3M, 50000M);
        bank.AddTariff(bankTariff);

        Client client = GetUnverifiedClient("Mariya", "Izerakova");
        bank.RegisterClient(client);

        var firstBankAccountDataInitializer = new DebitAccountDataInitializer();
        TransactionHandler firstBankAccount = bank.RegisterAccount(client, firstBankAccountDataInitializer, bankTariff);

        var secondBankAccountDataInitializer = new DebitAccountDataInitializer();
        TransactionHandler secondBankAccount = bank.RegisterAccount(client, secondBankAccountDataInitializer, bankTariff);

        decimal firstTransactionAmount = 5000;
        var firstTransaction = new DepositTransaction(firstBankAccount, firstTransactionAmount);

        bank.DoTransaction(firstTransaction);

        decimal secondTransactionAmount = 1000;
        var secondTransaction = new TransferTransaction(firstBankAccount, secondBankAccount, secondTransactionAmount);

        bank.DoTransaction(secondTransaction);
        bank.UndoTransaction(secondTransaction);

        Assert.Single(firstBankAccount.Transactions);
        Assert.Empty(secondBankAccount.Transactions);

        Assert.Equal(firstTransactionAmount, firstBankAccount.Balance);
        Assert.Equal(0, secondBankAccount.Balance);
    }

    [Fact]
    public void DoTransactionByDepositBankAcount_ThrowsUnavaliableOperationException()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        BankTariff bankTariff = GetBankTariff("Tariff1", 0M, 100000M, 3M, 3M, -100000M, 3M, 50000M);
        bank.AddTariff(bankTariff);

        Client client = GetUnverifiedClient("Mariya", "Izerakova");
        bank.RegisterClient(client);

        var bankAccountDataInitializer = new DepositAccountDataInitializer(new DateTime(2050, 12, 20));
        TransactionHandler bankAccount = bank.RegisterAccount(client, bankAccountDataInitializer, bankTariff);

        decimal firstTransactionAmount = 5000;
        var firstTransaction = new DepositTransaction(bankAccount, firstTransactionAmount);

        bank.DoTransaction(firstTransaction);

        Assert.Single(bankAccount.Transactions);
        Assert.Contains(firstTransaction, bankAccount.Transactions);

        Assert.Equal(firstTransactionAmount, bankAccount.Balance);

        decimal secondTransactionAmount = 1000;
        var secondTransaction = new WithdrawTransaction(bankAccount, secondTransactionAmount);

        Assert.Throws<UnavaliableOperationException>(() => bank.DoTransaction(secondTransaction));
    }

    [Fact]
    public void ReachCreditLimit_ThrowsUnavaliableOperationException()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        decimal creditLowerBoundary = -1000M;

        BankTariff bankTariff = GetBankTariff("Tariff1", 0M, 100000M, 3M, 3M, creditLowerBoundary, 3M, 50000M);
        bank.AddTariff(bankTariff);

        Client client = GetUnverifiedClient("Mariya", "Izerakova");
        bank.RegisterClient(client);

        var bankAccountDataInitializer = new CreditAccountDataInitializer();
        TransactionHandler bankAccount = bank.RegisterAccount(client, bankAccountDataInitializer, bankTariff);

        decimal firstTransactionAmount = 5000;
        var firstTransaction = new DepositTransaction(bankAccount, firstTransactionAmount);

        decimal secondTransactionAmount = 15500;
        var secondTransaction = new WithdrawTransaction(bankAccount, secondTransactionAmount);

        Assert.Throws<UnavaliableOperationException>(() => bank.DoTransaction(secondTransaction));
    }

    [Fact]
    public void DoTransactionByUnverifiedClient_ThrowsUnavaliableOperationException()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        decimal unverifiedClientTransactionLimit = 10000M;

        BankTariff bankTariff = GetBankTariff("Tariff1", 0M, 100000M, 3M, 3M, -100000M, 3M, unverifiedClientTransactionLimit);
        bank.AddTariff(bankTariff);

        Client client = GetUnverifiedClient("Mariya", "Izerakova");
        bank.RegisterClient(client);

        var bankAccountDataInitializer = new DebitAccountDataInitializer();
        TransactionHandler bankAccount = bank.RegisterAccount(client, bankAccountDataInitializer, bankTariff);

        decimal firstTransactionAmount = 5000000M;
        var firstTransaction = new DepositTransaction(bankAccount, firstTransactionAmount);

        bank.DoTransaction(firstTransaction);

        Assert.Single(bankAccount.Transactions);
        Assert.Contains(firstTransaction, bankAccount.Transactions);

        Assert.Equal(firstTransactionAmount, bankAccount.Balance);

        decimal secondTransactionAmount = unverifiedClientTransactionLimit * 2;
        var secondTransaction = new WithdrawTransaction(bankAccount, secondTransactionAmount);

        Assert.Throws<UnavaliableOperationException>(() => bank.DoTransaction(secondTransaction));
    }

    [Fact]
    public void UndoTransactionTwice_ThrowsUnavaliableOperationException()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        BankTariff bankTariff = GetBankTariff("Tariff1", 0M, 100000M, 3M, 3M, -100000M, 3M, 10000M);
        bank.AddTariff(bankTariff);

        Client client = GetUnverifiedClient("Mariya", "Izerakova");
        bank.RegisterClient(client);

        var bankAccountDataInitializer = new DebitAccountDataInitializer();
        TransactionHandler bankAccount = bank.RegisterAccount(client, bankAccountDataInitializer, bankTariff);

        decimal firstTransactionAmount = 5000000M;
        var firstTransaction = new DepositTransaction(bankAccount, firstTransactionAmount);

        bank.DoTransaction(firstTransaction);

        Assert.Single(bankAccount.Transactions);
        Assert.Contains(firstTransaction, bankAccount.Transactions);

        Assert.Equal(firstTransactionAmount, bankAccount.Balance);

        decimal secondTransactionAmount = 5000M;
        var secondTransaction = new WithdrawTransaction(bankAccount, secondTransactionAmount);

        bank.DoTransaction(secondTransaction);
        bank.UndoTransaction(secondTransaction);

        Assert.Throws<UnavaliableOperationException>(() => bank.UndoTransaction(secondTransaction));
    }

    [Fact]
    public void DoTransaction_BankAccountIsBlocked()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        BankTariff bankTariff = GetBankTariff("Tariff1", 0M, 100000M, 3M, 3M, -100000M, 3M, 50000M);
        bank.AddTariff(bankTariff);

        Client client = GetUnverifiedClient("Mariya", "Izerakova");
        bank.RegisterClient(client);

        var firstBankAccountDataInitializer = new DebitAccountDataInitializer();
        TransactionHandler firstBankAccount = bank.RegisterAccount(client, firstBankAccountDataInitializer, bankTariff);

        var secondBankAccountDataInitializer = new DebitAccountDataInitializer();
        TransactionHandler secondBankAccount = bank.RegisterAccount(client, secondBankAccountDataInitializer, bankTariff);

        decimal firstTransactionAmount = 5000;
        var firstTransaction = new DepositTransaction(firstBankAccount, firstTransactionAmount);

        bank.DoTransaction(firstTransaction);

        decimal secondTransactionAmount = 1000;
        var secondTransaction = new TransferTransaction(firstBankAccount, secondBankAccount, secondTransactionAmount);

        bank.DoTransaction(secondTransaction);

        decimal thirdTransactionAmount = 5000;
        var thirdTransaction = new DepositTransaction(secondBankAccount, thirdTransactionAmount);

        bank.DoTransaction(thirdTransaction);

        decimal fourthTransactionAmount = 5500;
        var fourthTransaction = new WithdrawTransaction(secondBankAccount, fourthTransactionAmount);

        bank.DoTransaction(fourthTransaction);

        bank.UndoTransaction(secondTransaction);

        Assert.True(secondBankAccount.IsBlocked);
    }

    [Fact]
    public void DoTransactionByBlockedAccount_ThrowsUnavaliableOperationException()
    {
        Bank bank = centralBank.RegisterBank("Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        BankTariff bankTariff = GetBankTariff("Tariff1", 0M, 100000M, 3M, 3M, -100000M, 3M, 50000M);
        bank.AddTariff(bankTariff);

        Client client = GetUnverifiedClient("Mariya", "Izerakova");
        bank.RegisterClient(client);

        var firstBankAccountDataInitializer = new DebitAccountDataInitializer();
        TransactionHandler firstBankAccount = bank.RegisterAccount(client, firstBankAccountDataInitializer, bankTariff);

        var secondBankAccountDataInitializer = new DebitAccountDataInitializer();
        TransactionHandler secondBankAccount = bank.RegisterAccount(client, secondBankAccountDataInitializer, bankTariff);

        decimal firstTransactionAmount = 5000;
        var firstTransaction = new DepositTransaction(firstBankAccount, firstTransactionAmount);

        bank.DoTransaction(firstTransaction);

        decimal secondTransactionAmount = 1000;
        var secondTransaction = new TransferTransaction(firstBankAccount, secondBankAccount, secondTransactionAmount);

        bank.DoTransaction(secondTransaction);

        decimal thirdTransactionAmount = 5000;
        var thirdTransaction = new DepositTransaction(secondBankAccount, thirdTransactionAmount);

        bank.DoTransaction(thirdTransaction);

        decimal fourthTransactionAmount = 5500;
        var fourthTransaction = new WithdrawTransaction(secondBankAccount, fourthTransactionAmount);

        bank.DoTransaction(fourthTransaction);

        bank.UndoTransaction(secondTransaction);

        decimal fifthTransactionAmount = 1000;
        var fifthTransaction = new WithdrawTransaction(secondBankAccount, fifthTransactionAmount);

        Assert.Throws<UnavaliableOperationException>(() => bank.DoTransaction(fifthTransaction));
    }

    private Client GetUnverifiedClient(
        string name,
        string lastname)
    {
        clientBuilder.SetName(name);
        clientBuilder.SetLastname(lastname);

        return clientBuilder.Register();
    }

    private Client GetVerifiedClient(
        string name,
        string lastname,
        Address? address,
        string? passportId)
    {
        clientBuilder.SetName(name);
        clientBuilder.SetLastname(lastname);
        clientBuilder.SetAddress(address);
        clientBuilder.SetPassportId(passportId);

        return clientBuilder.Register();
    }

    private Address GetAddress(
        string zip,
        string country,
        string city,
        string street,
        int streetNumber,
        string? building)
    {
        var addressBuilder = new AddressBuilder();

        addressBuilder.SetZip(zip);
        addressBuilder.SetCountry(country);
        addressBuilder.SetCity(city);
        addressBuilder.SetStreet(street);
        addressBuilder.SetStreetNumber(streetNumber);
        addressBuilder.SetBuilding(building);

        return addressBuilder.FillAddress();
    }

    private BankTariff GetBankTariff(
        string name,
        decimal initialDepositLowerBoudary,
        decimal initialDepositUpperBoudary,
        decimal initialDepositInterestRate,
        decimal debitInterestRate,
        decimal creditLowerBoundary,
        decimal creditComission,
        decimal unverifiedClientTransactionLimit)
    {
        var depositInterestRates = new List<DepositInterestRate>()
            { new DepositInterestRate(initialDepositLowerBoudary, initialDepositUpperBoudary, initialDepositInterestRate) };

        return new BankTariff(
            name,
            depositInterestRates,
            debitInterestRate,
            new CreditCondition(creditLowerBoundary, creditComission),
            unverifiedClientTransactionLimit);
    }
}
