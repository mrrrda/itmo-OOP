namespace Backups.Exceptions;

public class DuplicateRestorePointException : Exception
{
    public DuplicateRestorePointException()
        : base("Requested restore point already exists")
    { }
}
