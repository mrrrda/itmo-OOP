namespace Backups.Exceptions;

public class NotExistingStorageDataException : Exception
{
    public NotExistingStorageDataException()
        : base("Requested storage data entry does not exist")
    { }
}
