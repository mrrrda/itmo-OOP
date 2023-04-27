namespace Isu.Exceptions;

public class StudentNotFoundException : Exception
{
    public StudentNotFoundException(string id)
        : base(string.Format("Student {0} not found", id))
    { }
}
