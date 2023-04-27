namespace Isu.Exceptions;

public class NotExistingFacultyException : Exception
{
    public NotExistingFacultyException(string facultyCode)
        : base(string.Format("Faculty with code {0} does not exist", facultyCode))
    { }
}
