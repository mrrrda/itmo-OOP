namespace Banks.Exceptions;

public class NotExistingBankAccountException : Exception
{
    public NotExistingBankAccountException()
        : base("Requested bank account does not exist")
    { }
}
