using System.Collections.ObjectModel;

using Banks.Accounts.DataInitializers;
using Banks.BankActors;
using Banks.Exceptions;
using Banks.Transactions;

namespace Banks.Accounts.Proxies;

public class DepositAccountProxy : BankAccountProxy
{
    private DepositAccount depositAccount;

    public DepositAccountProxy(
        int id,
        Client client,
        Bank bank,
        BankTariff bankTariff,
        DepositAccountDataInitializer depositAccountDataInitializer)
    {
        depositAccount = new DepositAccount(id, client, bank, bankTariff, depositAccountDataInitializer);
    }

    public override int Id { get => depositAccount.Id; }

    public override Bank Bank { get => depositAccount.Bank; }
    public override Client Client { get => depositAccount.Client; }

    public override decimal Balance
    {
        get => depositAccount.Balance;

        internal set => depositAccount.Balance = value;
    }

    public override DateTime CreationTime { get => depositAccount.CreationTime; }

    public override bool IsBlocked
    {
        get => depositAccount.IsBlocked;

        internal set => depositAccount.IsBlocked = value;
    }

    public override BankTariff BankTariff
    {
        get => depositAccount.BankTariff;

        internal set => depositAccount.BankTariff = value;
    }

    public override ReadOnlyCollection<Transaction> Transactions { get => depositAccount.Transactions; }

    public DateTime Term { get => depositAccount.Term; }

    public override decimal CalculateInterest(DateTime dateTo)
    {
        return depositAccount.CalculateInterest(dateTo);
    }

    internal override void DoTransaction(Transaction transaction)
    {
        base.DoTransaction(transaction);

        if (transaction is not DepositTransaction)
        {
            int datesCompare = DateTime.Compare(DateTime.Now, Term);
            if (datesCompare <= 0)
                throw new UnavaliableOperationException("Withdraw and transfer transactions are not avaliavle until deposit term ends");

            if (!CheckClientVerificationStatus() && transaction.Amount > BankTariff?.UnverifiedClientTransactionLimit)
                throw new UnavaliableOperationException(string.Format("Unverified clients transaction amount must not exceed {0}", BankTariff?.UnverifiedClientTransactionLimit));

            if (IsBlocked)
                throw new UnavaliableOperationException("Impossible to do transaction, as the account is blocked");

            if (Balance - transaction.Amount < 0)
                throw new InsufficientBalanceException();
        }

        depositAccount.DoTransaction(transaction);
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

        depositAccount.UndoTransaction(transaction);
    }

    internal override void AddTransaction(Transaction transaction)
    {
        depositAccount.AddTransaction(transaction);
    }

    internal override void RemoveTransaction(Transaction transaction)
    {
        depositAccount.RemoveTransaction(transaction);
    }
}
