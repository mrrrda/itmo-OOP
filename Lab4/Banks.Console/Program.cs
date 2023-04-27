using Banks.Accounts;
using Banks.Accounts.DataInitializers;
using Banks.BankActors;
using Banks.Models;
using Banks.NotificationServices;
using Banks.Transactions;

namespace Banks.Console;

public class Programm
{
    public static void Main()
    {
        string separator = "---------------------------";

        int maxClientsNumber = 100000;
        int maxClientAccountsNumber = 1000000;

        var notificationService = new ConsoleNotificationService();

        var centralBank = CentralBank.Get();
        centralBank.MaxBanksNumber = 5096;

        Bank firstBank = centralBank.RegisterBank("Tinkoff Bank", maxClientsNumber, maxClientAccountsNumber, notificationService);
        Bank secondBank = centralBank.RegisterBank("SberBank", maxClientsNumber, maxClientAccountsNumber, notificationService);
        Bank thirdBank = centralBank.RegisterBank("AlphaBank", maxClientsNumber, maxClientAccountsNumber, notificationService);

        var firstBankTariffDepositInterestRates = new List<DepositInterestRate>()
            {
                new DepositInterestRate(0M, 50000M, 0.01M),
                new DepositInterestRate(50000M, 100000M, 0.03M),
                new DepositInterestRate(100000M, 200000M, 0.04M),
            };

        decimal firstBankTariffDebitInterestRate = 0.03M;

        var firstBankTariffCreditCondition = new CreditCondition(-50000, 0.03M);

        decimal firstBankTariffUnverifiedClientTransactionLimit = 100000M;

        var secondBankTariffDepositInterestRates = new List<DepositInterestRate>()
            {
                new DepositInterestRate(0M, 20000M, 0.01M),
                new DepositInterestRate(20000M, 50000M, 0.02M),
                new DepositInterestRate(50000M, 100000M, 0.03M),
            };

        decimal secondBankTariffDebitInterestRate = 0.025M;

        var secondBankTariffCreditCondition = new CreditCondition(-100000, 0.03M);

        decimal secondBankTariffUnverifiedClientTransactionLimit = 80000M;

        var thirdBankTariffDepositInterestRates = new List<DepositInterestRate>()
            {
                new DepositInterestRate(0M, 50000M, 0.02M),
                new DepositInterestRate(50000M, 100000M, 0.03M),
            };

        decimal thirdBankTariffDebitInterestRate = 0.02M;

        var thirdBankTariffCreditCondition = new CreditCondition(-150000, 0.04M);

        decimal thirdBankTariffUnverifiedClientTransactionLimit = 80000M;

        var firstBankTariff = new BankTariff(
            "Easy start",
            firstBankTariffDepositInterestRates,
            firstBankTariffDebitInterestRate,
            firstBankTariffCreditCondition,
            firstBankTariffUnverifiedClientTransactionLimit);

        var secondBankTariff = new BankTariff(
            "Gaining speed",
            secondBankTariffDepositInterestRates,
            secondBankTariffDebitInterestRate,
            secondBankTariffCreditCondition,
            secondBankTariffUnverifiedClientTransactionLimit);

        var thirdBankTariff = new BankTariff(
            "Full swing",
            thirdBankTariffDepositInterestRates,
            thirdBankTariffDebitInterestRate,
            thirdBankTariffCreditCondition,
            thirdBankTariffUnverifiedClientTransactionLimit);

        firstBank.AddTariff(firstBankTariff);
        secondBank.AddTariff(secondBankTariff);
        thirdBank.AddTariff(thirdBankTariff);

        var clientBuilder = new ClientBuilder();

        System.Console.WriteLine("Enter your name");
        string? name = System.Console.ReadLine();

        if (name is null)
            throw new ArgumentNullException("Name must be a non-null string");

        clientBuilder.SetName(name);

        System.Console.WriteLine("Enter your lastname");
        string? lastname = System.Console.ReadLine();

        if (lastname is null)
            throw new ArgumentNullException("Lastname must be a non-null string");

        clientBuilder.SetLastname(lastname);

        System.Console.WriteLine("Do you want to set passport id? (y/n)");
        string? passportIdChoice = System.Console.ReadLine()?.Trim().ToLower();

        switch (passportIdChoice)
        {
            case "y":
                System.Console.WriteLine("Enter your passport id:");
                string? passporId = System.Console.ReadLine();

                if (passporId is null)
                    throw new ArgumentNullException("Passport id must be a non-null string");

                clientBuilder.SetPassportId(passporId);

                break;

            case "n":
                System.Console.WriteLine("Your account is unverified. Set passport id and address in order to change it");
                break;

            default:
                throw new ArgumentException("Invalid option");
        }

        System.Console.WriteLine("Do you want to set address? (y/n)");
        string? addressChoice = System.Console.ReadLine()?.Trim().ToLower();

        switch (addressChoice)
        {
            case "y":
                var addressBuilder = new AddressBuilder();

                System.Console.WriteLine("Enter your zip:");
                string? zip = System.Console.ReadLine();

                if (zip is null)
                    throw new ArgumentNullException("Zip must be a non-null string");

                addressBuilder.SetZip(zip);

                System.Console.WriteLine("Enter your country:");
                string? country = System.Console.ReadLine();

                if (country is null)
                    throw new ArgumentNullException("Country must be a non-null string");

                addressBuilder.SetCountry(country);

                System.Console.WriteLine("Enter your city:");
                string? city = System.Console.ReadLine();

                if (city is null)
                    throw new ArgumentNullException("City must be a non-null string");

                addressBuilder.SetCity(city);

                System.Console.WriteLine("Enter your street name:");
                string? street = System.Console.ReadLine();

                if (street is null)
                    throw new ArgumentNullException("Street must be a non-null string");

                addressBuilder.SetStreet(street);

                System.Console.WriteLine("Enter your street number:");
                string? streetNumber = System.Console.ReadLine();

                if (streetNumber is null)
                    throw new ArgumentNullException("Street number must be a non-null numeric string");

                addressBuilder.SetStreetNumber(int.Parse(streetNumber));

                System.Console.WriteLine("Do you want to set address building? (y/n)");
                string? buildingChoice = System.Console.ReadLine()?.Trim().ToLower();

                switch (buildingChoice)
                {
                    case "y":
                        System.Console.WriteLine("Enter your building:");
                        string? building = System.Console.ReadLine();

                        if (building is null)
                            throw new ArgumentNullException("Street number must be a non-null alphanumeric string");

                        addressBuilder.SetBuilding(building);

                        break;

                    case "n":
                        break;

                    default:
                        throw new ArgumentException("Invalid option");
                }

                Address address = addressBuilder.FillAddress();
                clientBuilder.SetAddress(address);

                break;

            case "n":
                System.Console.WriteLine("Your account is unverified. Set passport id and address in order to change it");
                break;

            default:
                throw new ArgumentException("Invalid option");
        }

        Client client = clientBuilder.Register();

        while (true)
        {
            System.Console.WriteLine(
                "Choose an option:\n" +
                "0. Check available banks\n" +
                "1. Read the terms and conditions of a bank\n" +
                "2. Create new account\n" +
                "3. Do deposit transaction\n" +
                "4. Do withdraw transaction\n" +
                "5. Do transfer transaction\n" +
                "6. Check balance\n" +
                "7. Change bank tariff\n" +
                "8. Add subscribtion\n" +
                "9. Remove subscribtion\n" +
                "10. Set passport id\n" +
                "11. Set address\n" +
                "12. View interest to date\n" +
                "13. Exit");

            string? option = System.Console.ReadLine()?.Trim();

            switch (option)
            {
                case "0":
                    centralBank.Banks.ToList<Bank>().ForEach(bank => System.Console.WriteLine(bank.Name));

                    System.Console.WriteLine(separator);
                    break;

                case "1":
                    System.Console.WriteLine("Enter bank name");

                    string? readTermsBankName = System.Console.ReadLine()?.Trim().ToLower();

                    IEnumerable<Bank> readTermsFoundBanks = centralBank.Banks.ToList<Bank>()
                        .Where(bank => bank.Name.ToLower().Equals(readTermsBankName));

                    if (readTermsFoundBanks.Count() == 0)
                        throw new ArgumentException("Requested bank does not exists");

                    Bank readTermsFoundBank = readTermsFoundBanks.First();

                    foreach (BankTariff bankTariff in readTermsFoundBank.BankTariffs)
                    {
                        System.Console.WriteLine("Bank Tariff: " + bankTariff.Name);

                        System.Console.WriteLine("Deposit interest rates:");
                        foreach (DepositInterestRate depositInterestRate in bankTariff.DepositInterestRates)
                        {
                            System.Console.WriteLine(
                                "Lower boundary: " + depositInterestRate.LowerDepositBoundary + "\n" +
                                "Upper boundary: " + depositInterestRate.UpperDepositBoundary + "\n" +
                                "Interest rate: " + depositInterestRate.InterestRate + "\n");
                        }

                        System.Console.WriteLine();

                        System.Console.WriteLine("Debit interesrt rate: " + bankTariff.DebitInterestRate);

                        System.Console.WriteLine("Credit lower boundary: " + bankTariff.CreditCondition.LowerCreditBoundary);
                        System.Console.WriteLine("Credit comission: " + bankTariff.CreditCondition.Comission);

                        System.Console.WriteLine("Unverified client transaction limit: " + bankTariff.UnverifiedClientTransactionLimit);
                        System.Console.WriteLine();
                    }

                    System.Console.WriteLine(separator);
                    break;

                case "2":
                    System.Console.WriteLine("Enter bank name");

                    string? chosenBankName = System.Console.ReadLine()?.Trim().ToLower();

                    IEnumerable<Bank> banks = centralBank.Banks.ToList<Bank>()
                        .Where(bank => bank.Name.ToLower().Equals(chosenBankName));

                    if (banks.Count() == 0)
                        throw new ArgumentException("Requested bank does not exists");

                    Bank chosenBank = banks.First();

                    chosenBank.RegisterClient(client);

                    System.Console.WriteLine("Enter bank tariff name");

                    string? chosenBankTariffName = System.Console.ReadLine()?.Trim().ToLower();

                    IEnumerable<BankTariff> bankTariffs = chosenBank.BankTariffs.ToList<BankTariff>()
                        .Where(bankTariff => bankTariff.Name.ToLower().Equals(chosenBankTariffName));

                    if (bankTariffs.Count() == 0)
                        throw new ArgumentException("Requested bank tariff does not exists");

                    BankTariff chosenBankTariff = bankTariffs.First();

                    System.Console.WriteLine(
                        "Choose account type:\n" +
                        "1. Debit\n" +
                        "2. Deposit\n" +
                        "3. Credit");

                    string? accountType = System.Console.ReadLine()?.Trim();
                    TransactionHandler newBankAccount;

                    switch (accountType)
                    {
                        case "1":
                            newBankAccount = chosenBank.RegisterAccount(client, new DebitAccountDataInitializer(), chosenBankTariff);

                            break;

                        case "2":
                            newBankAccount = chosenBank.RegisterAccount(client, new DepositAccountDataInitializer(new DateTime(2023, 11, 19)), chosenBankTariff);

                            break;

                        case "3":
                            newBankAccount = chosenBank.RegisterAccount(client, new CreditAccountDataInitializer(), chosenBankTariff);

                            break;

                        default:
                            throw new ArgumentException("Invalid option");
                    }

                    System.Console.WriteLine(string.Format("Your bank account id: {0}", newBankAccount.Id));
                    System.Console.WriteLine(separator);
                    break;

                case "3":
                    System.Console.WriteLine("Enter your bank account id:");
                    string? depositTransactionId = System.Console.ReadLine()?.Trim();

                    if (depositTransactionId is null)
                        throw new ArgumentNullException("Bank account id must be a non-null numeric string");

                    var depositTransactionFoundBankAccounts = new List<TransactionHandler>();
                    foreach (Bank bank in centralBank.Banks)
                    {
                        foreach (TransactionHandler currentBankAccount in bank.BankAccounts)
                        {
                            if (currentBankAccount.Id == int.Parse(depositTransactionId))
                                depositTransactionFoundBankAccounts.Add(currentBankAccount);
                        }
                    }

                    if (depositTransactionFoundBankAccounts.Count == 0)
                        throw new ArgumentException("Requested bank account does not exist");

                    TransactionHandler depositTransactionBankAccount = depositTransactionFoundBankAccounts[0];

                    System.Console.WriteLine("Enter transaction amount: ");
                    string? depositAmount = System.Console.ReadLine()?.Trim();

                    if (depositAmount is null)
                        throw new ArgumentNullException("Amount must be a non-null numeric string");

                    var depositTransaction = new DepositTransaction(depositTransactionBankAccount, int.Parse(depositAmount));
                    depositTransactionBankAccount.Bank.DoTransaction(depositTransaction);

                    System.Console.WriteLine(separator);
                    break;

                case "4":
                    System.Console.WriteLine("Enter your bank account id:");
                    string? withdrawTransactionId = System.Console.ReadLine()?.Trim();

                    if (withdrawTransactionId is null)
                        throw new ArgumentNullException("Bank account id must be a non-null numeric string");

                    var withdrawTransactionFoundBankAccounts = new List<TransactionHandler>();
                    foreach (Bank bank in centralBank.Banks)
                    {
                        foreach (TransactionHandler currentBankAccount in bank.BankAccounts)
                        {
                            if (currentBankAccount.Id == int.Parse(withdrawTransactionId))
                                withdrawTransactionFoundBankAccounts.Add(currentBankAccount);
                        }
                    }

                    if (withdrawTransactionFoundBankAccounts.Count == 0)
                        throw new ArgumentException("Requested bank account does not exist");

                    TransactionHandler withdrawTransactionBankAccount = withdrawTransactionFoundBankAccounts[0];

                    System.Console.WriteLine("Enter transaction amount: ");
                    string? withdrawTransactionAmount = System.Console.ReadLine()?.Trim();

                    if (withdrawTransactionAmount is null)
                        throw new ArgumentNullException("Amount must be a non-null numeric string");

                    var withdrawTransaction = new WithdrawTransaction(withdrawTransactionBankAccount, int.Parse(withdrawTransactionAmount));
                    withdrawTransactionBankAccount.Bank.DoTransaction(withdrawTransaction);

                    System.Console.WriteLine(separator);
                    break;

                case "5":
                    System.Console.WriteLine("Enter your bank account id:");
                    string? transferTransactionId = System.Console.ReadLine()?.Trim();

                    if (transferTransactionId is null)
                        throw new ArgumentNullException("Bank account id must be a non-null numeric string");

                    var transferTransactionFoundBankAccounts = new List<TransactionHandler>();
                    foreach (Bank bank in centralBank.Banks)
                    {
                        foreach (TransactionHandler currentBankAccount in bank.BankAccounts)
                        {
                            if (currentBankAccount.Id == int.Parse(transferTransactionId))
                                transferTransactionFoundBankAccounts.Add(currentBankAccount);
                        }
                    }

                    if (transferTransactionFoundBankAccounts.Count == 0)
                        throw new ArgumentException("Requested bank account does not exist");

                    TransactionHandler transferTransactionBankAccount = transferTransactionFoundBankAccounts[0];

                    System.Console.WriteLine("Enter recipient bank account id:");
                    string? recipientId = System.Console.ReadLine()?.Trim();

                    if (recipientId is null)
                        throw new ArgumentNullException("Recipient bank account id must be a non-null numeric string");

                    var foundRecipientBankAccounts = new List<TransactionHandler>();
                    foreach (Bank bank in centralBank.Banks)
                    {
                        foreach (TransactionHandler currentBankAccount in bank.BankAccounts)
                        {
                            if (currentBankAccount.Id == int.Parse(recipientId))
                                foundRecipientBankAccounts.Add(currentBankAccount);
                        }
                    }

                    if (foundRecipientBankAccounts.Count == 0)
                        throw new ArgumentException("Recipient bank account does not exist");

                    TransactionHandler recipientBankAccount = foundRecipientBankAccounts[0];

                    System.Console.WriteLine("Enter transaction amount: ");
                    string? amount = System.Console.ReadLine()?.Trim();

                    if (amount is null)
                        throw new ArgumentNullException("Amount must be a non-null numeric string");

                    var transaction = new TransferTransaction(transferTransactionBankAccount, recipientBankAccount, int.Parse(amount));
                    transferTransactionBankAccount.Bank.DoTransaction(transaction);

                    System.Console.WriteLine(separator);
                    break;

                case "6":
                    System.Console.WriteLine("Enter your bank account id:");
                    string? checkBalanceId = System.Console.ReadLine()?.Trim();

                    if (checkBalanceId is null)
                        throw new ArgumentNullException("Bank account id must be a non-null numeric string");

                    var checkBalanceBankAccounts = new List<TransactionHandler>();
                    foreach (Bank bank in centralBank.Banks)
                    {
                        foreach (TransactionHandler currentBankAccount in bank.BankAccounts)
                        {
                            if (currentBankAccount.Id == int.Parse(checkBalanceId))
                                checkBalanceBankAccounts.Add(currentBankAccount);
                        }
                    }

                    if (checkBalanceBankAccounts.Count == 0)
                        throw new ArgumentException("Requested bank account does not exist");

                    TransactionHandler checkBalanceBankAccount = checkBalanceBankAccounts[0];

                    System.Console.WriteLine("Current balance: " + checkBalanceBankAccount.Balance);

                    System.Console.WriteLine(separator);
                    break;

                case "7":
                    System.Console.WriteLine("Enter your bank account id:");
                    string? changeBankTariffId = System.Console.ReadLine()?.Trim();

                    if (changeBankTariffId is null)
                        throw new ArgumentNullException("Bank account id must be a non-null numeric string");

                    var changeBankTariffBankAccounts = new List<TransactionHandler>();
                    foreach (Bank bank in centralBank.Banks)
                    {
                        foreach (TransactionHandler currentBankAccount in bank.BankAccounts)
                        {
                            if (currentBankAccount.Id == int.Parse(changeBankTariffId))
                                changeBankTariffBankAccounts.Add(currentBankAccount);
                        }
                    }

                    if (changeBankTariffBankAccounts.Count == 0)
                        throw new ArgumentException("Requested bank account does not exist");

                    TransactionHandler changeBankTariffBankAccount = changeBankTariffBankAccounts[0];

                    System.Console.WriteLine("Enter bank tariff name");

                    string? changeBankTariffName = System.Console.ReadLine()?.Trim().ToLower();

                    IEnumerable<BankTariff> bankTariffsForChange = changeBankTariffBankAccount.Bank.BankTariffs.ToList<BankTariff>()
                        .Where(bankTariff => bankTariff.Name.ToLower().Equals(changeBankTariffName));

                    if (bankTariffsForChange.Count() == 0)
                        throw new ArgumentException("Requested bank tariff does not exists");

                    BankTariff bankTariffForChange = bankTariffsForChange.First();

                    changeBankTariffBankAccount.Bank.ChangeBankAccountTariff(changeBankTariffBankAccount, bankTariffForChange);

                    System.Console.WriteLine(separator);
                    break;

                case "8":
                    System.Console.WriteLine("Enter bank name");

                    string? addSubscribtionBankName = System.Console.ReadLine()?.Trim().ToLower();

                    IEnumerable<Bank> addSubscribtionBanks = centralBank.Banks.ToList<Bank>()
                        .Where(bank => bank.Name.ToLower().Equals(addSubscribtionBankName));

                    if (addSubscribtionBanks.Count() == 0)
                        throw new ArgumentException("Requested bank does not exists");

                    Bank addSubscribtionBank = addSubscribtionBanks.First();

                    addSubscribtionBank.AddSubscribtion(client);

                    System.Console.WriteLine(separator);
                    break;

                case "9":
                    System.Console.WriteLine("Enter bank name");

                    string? removeSubscribtionBankName = System.Console.ReadLine()?.Trim().ToLower();

                    IEnumerable<Bank> removeSubscribtionBanks = centralBank.Banks.ToList<Bank>()
                        .Where(bank => bank.Name.ToLower().Equals(removeSubscribtionBankName));

                    if (removeSubscribtionBanks.Count() == 0)
                        throw new ArgumentException("Requested bank does not exists");

                    Bank removeSubscribtionBank = removeSubscribtionBanks.First();

                    removeSubscribtionBank.RemoveSubscribtion(client);

                    System.Console.WriteLine(separator);
                    break;

                case "10":
                    System.Console.WriteLine("Enter your passport id:");
                    string? passporId = System.Console.ReadLine();

                    if (passporId is null)
                        throw new ArgumentNullException("Passport id must be a non-null string");

                    client.PassportId = passporId;

                    System.Console.WriteLine(separator);
                    break;

                case "11":
                    var addressBuilder = new AddressBuilder();

                    System.Console.WriteLine("Enter your zip:");
                    string? zip = System.Console.ReadLine();

                    if (zip is null)
                        throw new ArgumentNullException("Zip must be a non-null string");

                    addressBuilder.SetZip(zip);

                    System.Console.WriteLine("Enter your country:");
                    string? country = System.Console.ReadLine();

                    if (country is null)
                        throw new ArgumentNullException("Country must be a non-null string");

                    addressBuilder.SetCountry(country);

                    System.Console.WriteLine("Enter your city:");
                    string? city = System.Console.ReadLine();

                    if (city is null)
                        throw new ArgumentNullException("City must be a non-null string");

                    addressBuilder.SetCity(city);

                    System.Console.WriteLine("Enter your street name:");
                    string? street = System.Console.ReadLine();

                    if (street is null)
                        throw new ArgumentNullException("Street must be a non-null string");

                    addressBuilder.SetStreet(street);

                    System.Console.WriteLine("Enter your street number:");
                    string? streetNumber = System.Console.ReadLine();

                    if (streetNumber is null)
                        throw new ArgumentNullException("Street number must be a non-null numeric string");

                    addressBuilder.SetStreetNumber(int.Parse(streetNumber));

                    System.Console.WriteLine("Do you want to set address building? (y/n)");
                    string? buildingChoice = System.Console.ReadLine()?.Trim().ToLower();

                    switch (buildingChoice)
                    {
                        case "y":
                            System.Console.WriteLine("Enter your building:");
                            string? building = System.Console.ReadLine();

                            if (building is null)
                                throw new ArgumentNullException("Street number must be a non-null alphanumeric string");

                            addressBuilder.SetBuilding(building);

                            break;

                        case "n":
                            break;

                        default:
                            throw new ArgumentException("Invalid option");
                    }

                    Address address = addressBuilder.FillAddress();
                    client.Address = address;

                    System.Console.WriteLine(separator);
                    break;

                case "12":
                    System.Console.WriteLine("Enter your bank account id:");
                    string? dateRewindBankTariffId = System.Console.ReadLine()?.Trim();

                    if (dateRewindBankTariffId is null)
                        throw new ArgumentNullException("Bank account id must be a non-null numeric string");

                    var dateRewindBankTariffBankAccounts = new List<TransactionHandler>();
                    foreach (Bank bank in centralBank.Banks)
                    {
                        foreach (TransactionHandler currentBankAccount in bank.BankAccounts)
                        {
                            if (currentBankAccount.Id == int.Parse(dateRewindBankTariffId))
                                dateRewindBankTariffBankAccounts.Add(currentBankAccount);
                        }
                    }

                    if (dateRewindBankTariffBankAccounts.Count == 0)
                        throw new ArgumentException("Requested bank account does not exist");

                    TransactionHandler dateRewindBankTariffBankAccount = dateRewindBankTariffBankAccounts[0];

                    System.Console.WriteLine("Enter the desired date:");
                    string? date = System.Console.ReadLine();

                    if (date is null)
                        throw new ArgumentNullException(nameof(date), "Date must be a non-null string");

                    decimal dateRewindInterest = dateRewindBankTariffBankAccount.CalculateInterest(DateTime.Parse(date));

                    System.Console.WriteLine(
                        string.Format("Expected interest: {0}\n", dateRewindInterest) +
                        string.Format("Expected total: {0}", dateRewindBankTariffBankAccount.Balance + dateRewindInterest));

                    break;

                case "13":
                    return;

                default:
                    throw new ArgumentException("Invalid option");
            }
        }
    }
}
