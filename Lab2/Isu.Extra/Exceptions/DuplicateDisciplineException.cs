namespace Isu.Extra.Exceptions;

public class DuplicateDisciplineException : Exception
{
    public DuplicateDisciplineException(string name)
        : base(string.Format("Discipline {0} already exists", name))
    { }
}
