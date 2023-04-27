namespace Backups.Exceptions;

public class DuplicateStorageDataException : Exception
{
    public DuplicateStorageDataException()
        : base("Storage data entry already exists")
    { }
}
