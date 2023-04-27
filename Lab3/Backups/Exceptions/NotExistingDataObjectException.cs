namespace Backups.Exceptions;

public class NotExistingDataObjectException : Exception
{
    public NotExistingDataObjectException()
        : base("Requested data object does not exist")
    { }
}
