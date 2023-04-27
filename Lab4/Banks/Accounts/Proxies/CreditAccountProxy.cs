using System.Collections.ObjectModel;

using Banks.Accounts.DataInitializers;
using Banks.BankActors;
using Banks.Exceptions;
using Banks.Transactions;

namespace Banks.Accounts.Proxies;

public class CreditAccountProxy : BankAccountProxy
{
    private CreditAccount creditAccount;

    public CreditAccountProxy(
        int id,
        Client client,
        Bank bank,
        BankTariff bankTariff,
        CreditAccountDataInitializer creditAccountDataInitializer)
    {
        creditAccount = new CreditAccount(id, client, bank, bankTariff, creditAccountDataInitializer);
    }

    public override int Id { get => creditAccount.Id; }

    public override Bank Bank { get => creditAccount.Bank; }
    public override Client Client { get => creditAccount.Client; }

    public override decimal Balance
    {
        get => creditAccount.Balance;

        internal set => creditAccount.Balance = value;
    }

    public override DateTime CreationTime { get => creditAccount.CreationTime; }

    public override bool IsBlocked
    {
        get => creditAccount.IsBlocked;

        internal set => creditAccount.IsBlocked = value;
    }

    public override BankTariff BankTariff
    {
        get => creditAccount.BankTariff;

        internal set => creditAccount.BankTariff = value;
    }

    public override ReadOnlyCollection<Transaction> Transactions { get => creditAccount.Transactions; }

    public override decimal CalculateInterest(DateTime dateTo)
    {
        return creditAccount.CalculateInterest(dateTo);
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

            if (Balance - transaction.Amount < BankTariff?.CreditCondition.LowerCreditBoundary)
                throw new UnavaliableOperationException("Impossible to do transaction due to credit limit excess");
        }

        creditAccount.DoTransaction(transaction);
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

        creditAccount.UndoTransaction(transaction);
    }

    internal override void AddTransaction(Transaction transaction)
    {
        creditAccount.AddTransaction(transaction);
    }

    internal override void RemoveTransaction(Transaction transaction)
    {
        creditAccount.RemoveTransaction(transaction);
    }
}
