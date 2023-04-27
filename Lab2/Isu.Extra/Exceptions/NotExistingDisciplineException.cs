namespace Isu.Extra.Exceptions;

public class NotExistingDisciplineException : Exception
{
    public NotExistingDisciplineException()
        : base("Requested discipline does not exist")
    { }
}
