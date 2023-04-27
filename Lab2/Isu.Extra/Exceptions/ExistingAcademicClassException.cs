namespace Isu.Extra.Exceptions;

public class ExistingAcademicClassException : Exception
{
    public ExistingAcademicClassException()
        : base("Academic class already assigned")
    { }
}
