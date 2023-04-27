namespace Isu.Extra.Exceptions;

public class OgnpsNumberOverflowException : Exception
{
    public OgnpsNumberOverflowException(string maxFacultyOgnpsNumber)
        : base(string.Format("Max faculty ognps number {0} has been reached", maxFacultyOgnpsNumber))
    { }
}
