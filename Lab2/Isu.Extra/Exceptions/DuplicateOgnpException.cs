namespace Isu.Extra.Exceptions;

public class DuplicateOgnpException : Exception
{
    public DuplicateOgnpException()
        : base("Ognp already exists")
    { }
}
