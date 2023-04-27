namespace Isu.Exceptions;

public class DuplicateStudyUnitException : Exception
{
    public DuplicateStudyUnitException()
        : base("Study unit already exists")
    { }
}
