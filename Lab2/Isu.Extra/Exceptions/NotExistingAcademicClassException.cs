namespace Isu.Extra.Exceptions;

public class NotExistingAcademicClassException : Exception
{
    public NotExistingAcademicClassException()
        : base("Academic class does not exist")
    { }
}
