using Banks.BankActors;
using Banks.Exceptions;
using Banks.Transactions;

namespace Banks.Accounts.Proxies;

public abstract class BankAccountProxy : TransactionHandler
{
    public override bool IsBlocked { get; internal set; }

    public override abstract decimal CalculateInterest(DateTime dateTo);

    internal override void DoTransaction(Transaction transaction)
    {
        if (transaction is null)
            throw new ArgumentNullException(nameof(transaction), "Invalid transaction");
    }

    internal override void UndoTransaction(Transaction transaction)
    {
        if (transaction is null)
            throw new ArgumentNullException(nameof(transaction), "Invalid transaction");

        if (transaction.Canceled)
            throw new UnavaliableOperationException("Impossible to undo transaction twice");
    }

    internal override abstract void AddTransaction(Transaction transaction);

    internal override abstract void RemoveTransaction(Transaction transaction);

    protected bool CheckClientVerificationStatus()
    {
        return Client.Address is not null && Client.PassportId is not null;
    }
}
