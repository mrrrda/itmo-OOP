namespace Isu.Exceptions;

public class StudentsNumberOverflowException : Exception
{
    public StudentsNumberOverflowException(string maxStudentsNumber)
        : base(string.Format("Max students number {0} has been reached", maxStudentsNumber))
    { }
}
