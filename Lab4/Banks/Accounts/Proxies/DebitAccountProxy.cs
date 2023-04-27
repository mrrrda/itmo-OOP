using System.Collections.ObjectModel;

using Banks.Accounts.DataInitializers;
using Banks.BankActors;
using Banks.Exceptions;
using Banks.Transactions;

namespace Banks.Accounts.Proxies;

public class DebitAccountProxy : BankAccountProxy
{
    private DebitAccount debitAccount;

    public DebitAccountProxy(
        int id,
        Client client,
        Bank bank,
        BankTariff bankTariff,
        DebitAccountDataInitializer debitAccountDataInitializer)
    {
        debitAccount = new DebitAccount(id, client, bank, bankTariff, debitAccountDataInitializer);
    }

    public override int Id { get => debitAccount.Id; }

    public override Bank Bank { get => debitAccount.Bank; }
    public override Client Client { get => debitAccount.Client; }

    public override decimal Balance
    {
        get => debitAccount.Balance;

        internal set => debitAccount.Balance = value;
    }

    public override DateTime CreationTime { get => debitAccount.CreationTime; }

    public override bool IsBlocked
    {
        get => debitAccount.IsBlocked;

        internal set => debitAccount.IsBlocked = value;
    }

    public override BankTariff BankTariff
    {
        get => debitAccount.BankTariff;

        internal set => debitAccount.BankTariff = value;
    }

    public override ReadOnlyCollection<Transaction> Transactions { get => debitAccount.Transactions; }

    public override decimal CalculateInterest(DateTime dateTo)
    {
        return debitAccount.CalculateInterest(dateTo);
    }

    internal override void DoTransaction(Transaction transaction)
    {
        base.DoTransaction(transaction);

        if (transaction is not DepositTransaction)
        {
            if (!CheckClientVerificationStatus() && transaction.Amount > BankTariff?.UnverifiedClientTransactionLimit)
                throw new UnavaliableOperationException(string.Format("Unverified clients transaction amount must not exceed {0}", BankTariff?.UnverifiedClientTransactionLimit));

            if (IsBlocked)
                throw new UnavaliableOperationException("Impossible to do transaction, as the account is blocked");

            if (Balance - transaction.Amount < 0)
                throw new InsufficientBalanceException();
        }

        debitAccount.DoTransaction(transaction);
    }

    internal override void UndoTransaction(Transaction transaction)
    {
        base.UndoTransaction(transaction);

        if (transaction is TransferTransaction)
        {
            var transferTransaction = (TransferTransaction)transaction;

            if (transferTransaction.BankAccountTo.Balance - transferTransaction.Amount < 0)
                transferTransaction.BankAccountTo.IsBlocked = true;
        }

        debitAccount.UndoTransaction(transaction);
    }

    internal override void AddTransaction(Transaction transaction)
    {
        debitAccount.AddTransaction(transaction);
    }

    internal override void RemoveTransaction(Transaction transaction)
    {
        debitAccount.RemoveTransaction(transaction);
    }
}
