namespace Isu.Exceptions;

public class NotExistingStudyUnitException : Exception
{
    public NotExistingStudyUnitException()
        : base("Study unit does not exist")
    { }
}
