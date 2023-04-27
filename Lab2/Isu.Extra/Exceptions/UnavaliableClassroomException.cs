namespace Isu.Extra.Exceptions;

public class UnavaliableClassroomException : Exception
{
    public UnavaliableClassroomException(string classroom)
        : base(string.Format("Classroom {0} is already occupied at requested time", classroom))
    { }
}
