namespace Isu.Extra.Exceptions;

public class DisciplinesNumberOverflowException : Exception
{
    public DisciplinesNumberOverflowException(string maxDisciplinesNumber)
        : base(string.Format("Max disciplines number {0} has been reached", maxDisciplinesNumber))
    { }
}
