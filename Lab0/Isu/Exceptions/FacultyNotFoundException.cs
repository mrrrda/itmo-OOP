namespace Isu.Exceptions;

public class FacultyNotFoundException : Exception
{
    public FacultyNotFoundException(string facultyCode)
        : base(string.Format("Faculty with code {0} not found", facultyCode))
    { }
}
