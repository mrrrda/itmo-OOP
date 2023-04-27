namespace Isu.Exceptions;

public class StudyUnitsNumberOverflowException : Exception
{
    public StudyUnitsNumberOverflowException(string maxStudyUnitsNumber)
        : base(string.Format("Max study units number {0} at academic class has been reached", maxStudyUnitsNumber))
    { }
}
