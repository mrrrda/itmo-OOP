namespace Backups.Exceptions;

public class DuplicateBackupObjectException : Exception
{
    public DuplicateBackupObjectException()
        : base("Backup object already exists")
    { }
}
