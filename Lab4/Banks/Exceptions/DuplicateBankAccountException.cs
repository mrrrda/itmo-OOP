namespace Banks.Exceptions;

public class DuplicateBankAccountException : Exception
{
    public DuplicateBankAccountException()
        : base("Requested bank account already exists")
    { }
}
