using Banks.Accounts;
using Banks.Exceptions;

namespace Banks.Transactions;

public class TransferTransaction : Transaction
{
    public TransferTransaction(TransactionHandler bankAccountFrom, TransactionHandler bankAccountTo, decimal amount)
        : base(bankAccountFrom, amount)
    {
        if (bankAccountTo is null)
            throw new ArgumentNullException(nameof(bankAccountTo), "Invalid bank account");

        if (bankAccountTo.Equals(bankAccountFrom))
            throw new UnavaliableOperationException("Cannot do transfer to the same bank account");

        BankAccountTo = bankAccountTo;

        WithdrawTransaction = new WithdrawTransaction(BankAccount, Amount);
        DepositTransaction = new DepositTransaction(BankAccountTo, Amount);
    }

    public TransactionHandler BankAccountTo { get; }

    internal WithdrawTransaction WithdrawTransaction { get; private set; }
    internal DepositTransaction DepositTransaction { get; private set; }

    internal override decimal SignedAmount { get => Amount; }

    public override void Execute()
    {
        BankAccount.DoTransaction(WithdrawTransaction);
        BankAccountTo.DoTransaction(DepositTransaction);
    }

    public override void Undo()
    {
        BankAccount.UndoTransaction(WithdrawTransaction);
        BankAccountTo.UndoTransaction(DepositTransaction);

        Canceled = true;
    }
}
