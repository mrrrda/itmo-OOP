using Banks.Accounts;

namespace Banks.Transactions;

public abstract class Transaction
{
    protected Transaction(TransactionHandler bankAccount, decimal amount)
    {
        if (bankAccount is null)
            throw new ArgumentNullException(nameof(bankAccount), "Invalid bank account");

        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Transaction amount must be positive");

        CreationTime = DateTime.Now;
        Canceled = false;

        BankAccount = bankAccount;
        Amount = amount;
    }

    public TransactionHandler BankAccount { get; }
    public decimal Amount { get; }

    public bool Canceled { get; internal set; }

    public DateTime CreationTime { get; }

    internal abstract decimal SignedAmount { get; }

    public abstract void Execute();
    public abstract void Undo();
}
