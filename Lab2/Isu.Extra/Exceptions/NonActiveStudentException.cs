namespace Isu.Extra.Exceptions;

public class NonActiveStudentException : Exception
{
    public NonActiveStudentException(string id)
        : base(string.Format("Non-active student {0}", id))
    { }
}
