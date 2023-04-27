namespace Isu.Exceptions;

public class FacultiesNumberOverflowException : Exception
{
    public FacultiesNumberOverflowException(string maxFacultiesNumber)
        : base(string.Format("Max faculties number {0} has been reached", maxFacultiesNumber))
    { }
}
