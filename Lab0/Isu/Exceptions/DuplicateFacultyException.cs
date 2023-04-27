namespace Isu.Exceptions;

public class DuplicateFacultyException : Exception
{
    public DuplicateFacultyException(string facultyCode)
        : base(string.Format("Faculty code {0} already exists", facultyCode))
    { }
}
