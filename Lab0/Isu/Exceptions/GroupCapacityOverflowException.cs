namespace Isu.Exceptions;

public class GroupCapacityOverflowException : Exception
{
    public GroupCapacityOverflowException(string maxGroupCapacity)
        : base(string.Format("Max students number {0} in group has been reached", maxGroupCapacity))
    { }
}
