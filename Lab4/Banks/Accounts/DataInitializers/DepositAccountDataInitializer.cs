namespace Banks.Accounts.DataInitializers;

public class DepositAccountDataInitializer : BankAccountDataInitializer
{
    public DepositAccountDataInitializer(DateTime term)
        : base()
    {
        if (DateTime.Compare(DateTime.Now, term) >= 0)
            throw new ArgumentException("Term must be after current date", nameof(term));

        Term = term;
    }

    public DateTime Term { get; }
}
