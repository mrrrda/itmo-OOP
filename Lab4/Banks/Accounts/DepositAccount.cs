using Banks.Accounts.DataInitializers;
using Banks.BankActors;
using Banks.Models;
using Banks.Transactions;

namespace Banks.Accounts;

public class DepositAccount : BankAccount
{
    public DepositAccount(int id, Client client, Bank bank, BankTariff bankTariff, DepositAccountDataInitializer depositAccountDataInitializer)
        : base(id, client, bank, bankTariff, depositAccountDataInitializer)
    {
        Term = depositAccountDataInitializer.Term;
    }

    public DateTime Term { get; }

    public override decimal CalculateInterest(DateTime dateTo)
    {
        if (DateTime.Compare(DateTime.Now, dateTo) > 0)
            throw new ArgumentException("DateTo must be greater that current date", nameof(dateTo));

        if (BankTariff is null)
            throw new ArgumentNullException(nameof(BankTariff), "Invalid bank tariff");

        List<Transaction> transactions = Transactions.OrderBy(transaction => transaction.CreationTime).ToList();

        decimal accumulatedBalance = 0;

        for (int i = 0; i < Transactions.Count && Transactions[i].CreationTime < InterestLastCalculationDate; i++)
        {
            accumulatedBalance += Transactions[i].SignedAmount;
        }

        int lastBreakpointIndex = 0;
        decimal accumulatedInterest = 0m;

        decimal initialBalance = transactions[0].SignedAmount;
        decimal dailyInterestRate = 0M;

        foreach (DepositInterestRate depositInterestRate in BankTariff.DepositInterestRates)
        {
            if (initialBalance >= depositInterestRate.LowerDepositBoundary && initialBalance <= depositInterestRate.UpperDepositBoundary)
            {
                dailyInterestRate = depositInterestRate.InterestRate;
                break;
            }
        }

        for (DateTime currentDate = InterestLastCalculationDate; currentDate <= dateTo; currentDate = currentDate.AddDays(1))
        {
            for (int i = lastBreakpointIndex; i < Transactions.Count && Transactions[i].CreationTime.Equals(currentDate); i++, lastBreakpointIndex++)
                accumulatedBalance += Transactions[i].SignedAmount;

            accumulatedInterest += accumulatedBalance * dailyInterestRate;
        }

        return accumulatedInterest;
    }

    internal override void DoTransaction(Transaction transaction)
    {
        if (transaction is null)
            throw new ArgumentNullException(nameof(transaction), "Invalid transaction");

        transaction.Execute();
    }

    internal override void UndoTransaction(Transaction transaction)
    {
        if (transaction is null)
            throw new ArgumentNullException(nameof(transaction), "Invalid transaction");

        transaction.Undo();
    }
}
