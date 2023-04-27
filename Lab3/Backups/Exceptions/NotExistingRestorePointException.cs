namespace Backups.Exceptions;

public class NotExistingRestorePointException : Exception
{
    public NotExistingRestorePointException()
        : base("Requested restore point does not exist")
    { }
}
