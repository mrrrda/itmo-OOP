namespace Banks.Accounts.DataInitializers;

public abstract class BankAccountDataInitializer
{
    protected static readonly decimal InitialBalance = 0;

    protected BankAccountDataInitializer()
    {
        Balance = InitialBalance;
        IsBlocked = false;

        CreationTime = DateTime.Now;
    }

    public decimal Balance { get; internal set; }
    public bool IsBlocked { get; internal set; }

    public DateTime CreationTime { get; }
}
