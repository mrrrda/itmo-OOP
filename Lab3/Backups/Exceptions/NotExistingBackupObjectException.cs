namespace Backups.Exceptions;

public class NotExistingBackupObjectException : Exception
{
    public NotExistingBackupObjectException()
        : base("Requested backup object does not exist")
    { }
}
