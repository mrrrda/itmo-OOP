using Banks.Accounts.DataInitializers;
using Banks.BankActors;
using Banks.Transactions;

namespace Banks.Accounts;

public class CreditAccount : BankAccount
{
    public CreditAccount(int id, Client client, Bank bank, BankTariff bankTariff, CreditAccountDataInitializer creditAccountDataInitializer)
        : base(id, client, bank, bankTariff, creditAccountDataInitializer)
    { }

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

        decimal dailyInterestRate = BankTariff.CreditCondition.Comission / 365;

        for (DateTime currentDate = InterestLastCalculationDate; currentDate <= dateTo; currentDate = currentDate.AddDays(1))
        {
            for (int i = lastBreakpointIndex; i < Transactions.Count && Transactions[i].CreationTime.Equals(currentDate); i++, lastBreakpointIndex++)
                accumulatedBalance += Transactions[i].SignedAmount;

            if (accumulatedBalance < 0)
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
