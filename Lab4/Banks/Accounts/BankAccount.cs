using System.Collections.ObjectModel;

using Banks.Accounts.DataInitializers;
using Banks.BankActors;
using Banks.Exceptions;
using Banks.Transactions;

namespace Banks.Accounts;

public abstract class BankAccount : TransactionHandler
{
    private BankTariff bankTariff;
    private List<Transaction> transactions;

    protected BankAccount(
        int id,
        Client client,
        Bank bank,
        BankTariff bankTariff,
        BankAccountDataInitializer bankAccountDataInitializer)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id), "Bank account id must be positive");

        if (client is null)
            throw new ArgumentNullException(nameof(client), "Invalid client");

        if (bank is null)
            throw new ArgumentNullException(nameof(bank), "Invalid bank");

        if (bankTariff is null)
            throw new ArgumentNullException(nameof(bankTariff), "Invalid bank tariff");

        if (bankAccountDataInitializer is null)
            throw new ArgumentNullException(nameof(bankAccountDataInitializer), "Invalid bank account data initializer");

        transactions = new List<Transaction>();

        Id = id;
        Client = client;
        Bank = bank;
        this.bankTariff = bankTariff;

        Balance = bankAccountDataInitializer.Balance;
        IsBlocked = bankAccountDataInitializer.IsBlocked;
        CreationTime = bankAccountDataInitializer.CreationTime;
    }

    public override int Id { get; }
    public override Bank Bank { get; }

    public override Client Client { get; }

    public override decimal Balance { get; internal set; }

    public override bool IsBlocked { get; internal set; }

    public override BankTariff BankTariff
    {
        get => bankTariff;

        internal set
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value), "Invalid bank tariff");

            bankTariff = value;
        }
    }

    public override DateTime CreationTime { get; }

    public override ReadOnlyCollection<Transaction> Transactions => transactions.AsReadOnly();

    public override abstract decimal CalculateInterest(DateTime dateTo);

    internal override abstract void DoTransaction(Transaction transaction);

    internal override abstract void UndoTransaction(Transaction transaction);

    internal override void AddTransaction(Transaction transaction)
    {
        if (transaction is null)
            throw new ArgumentNullException(nameof(transaction), "Invalid transaction");

        if (transactions.Contains(transaction))
            throw new DuplicateTransactionException();

        transactions.Add(transaction);
    }

    internal override void RemoveTransaction(Transaction transaction)
    {
        if (transaction is null)
            throw new ArgumentNullException(nameof(transaction), "Invalid transaction");

        if (!transactions.Contains(transaction))
            throw new NotExistingTransactionException();

        transactions.Remove(transaction);
    }
}
