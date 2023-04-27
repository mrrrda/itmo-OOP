namespace Isu.Extra.Exceptions;

public class ScheduleIntersectionException : Exception
{
    public ScheduleIntersectionException()
        : base("Student classes must not intersect with ognp classes")
    { }
}
