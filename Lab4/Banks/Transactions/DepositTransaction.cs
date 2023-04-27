using Banks.Accounts;

namespace Banks.Transactions;

public class DepositTransaction : Transaction
{
    public DepositTransaction(TransactionHandler bankAccount, decimal amount)
            : base(bankAccount, amount)
    { }

    internal override decimal SignedAmount { get => Amount; }

    public override void Execute()
    {
        BankAccount.Balance += Amount;

        BankAccount.AddTransaction(this);
    }

    public override void Undo()
    {
        BankAccount.Balance -= Amount;

        Canceled = true;
        BankAccount.RemoveTransaction(this);
    }
}
