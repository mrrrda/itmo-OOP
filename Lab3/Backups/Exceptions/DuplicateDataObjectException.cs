namespace Backups.Exceptions;

public class DuplicateDataObjectException : Exception
{
    public DuplicateDataObjectException()
        : base("Data object already exists")
    { }
}
