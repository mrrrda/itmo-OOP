namespace Isu.Extra.Exceptions;

public class TutorsNumberOverflowException : Exception
{
    public TutorsNumberOverflowException(string maxTutorsNumber)
        : base(string.Format("Max tutors number {0} has been reached", maxTutorsNumber))
    { }
}
