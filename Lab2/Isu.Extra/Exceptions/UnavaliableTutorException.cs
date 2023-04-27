namespace Isu.Extra.Exceptions;

public class UnavaliableTutorException : Exception
{
    public UnavaliableTutorException(string tutorId)
        : base(string.Format("Tutor {0} is unavaliabe because they already have assigned class at requested time", tutorId))
    { }
}
