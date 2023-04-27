using System.Collections.ObjectModel;

using Banks.BankActors;
using Banks.Transactions;

namespace Banks.Accounts;

public abstract class TransactionHandler
{
    public abstract int Id { get; }

    public abstract Client Client { get; }
    public abstract Bank Bank { get; }

    public abstract BankTariff BankTariff { get; internal set; }
    public abstract decimal Balance { get; internal set; }

    public abstract bool IsBlocked { get; internal set; }

    public abstract DateTime CreationTime { get; }

    public abstract ReadOnlyCollection<Transaction> Transactions { get; }

    internal DateTime InterestLastCalculationDate { get; set; }

    public abstract decimal CalculateInterest(DateTime dateTo);

    internal abstract void DoTransaction(Transaction transaction);
    internal abstract void UndoTransaction(Transaction transaction);

    internal abstract void AddTransaction(Transaction transaction);
    internal abstract void RemoveTransaction(Transaction transaction);
}
