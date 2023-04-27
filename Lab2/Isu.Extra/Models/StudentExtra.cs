using System.Collections.ObjectModel;

using Isu.Extra.Exceptions;
using Isu.Models;

namespace Isu.Extra.Models;

public class StudentExtra : Student
{
    private readonly List<Ognp> ognps;

    public StudentExtra(int id, string fullName)
        : base(id, fullName)
    {
        ognps = new List<Ognp>();
    }

    public ReadOnlyCollection<Ognp> Ognps { get => ognps.AsReadOnly(); }

    public void AddOgnp(Ognp ognp)
    {
        if (ognp is null)
            throw new ArgumentNullException(nameof(ognp), "Invalid ognp");

        if (ognps.Contains(ognp))
            throw new DuplicateOgnpException();

        ognps.Add(ognp);
    }

    public void RemoveOgnp(Ognp ognp)
    {
        if (ognp is null)
            throw new ArgumentNullException(nameof(ognp), "Invalid ognp");

        if (!ognps.Contains(ognp))
            throw new InvalidOgnpException("Student is not enrolled in requested ognp");

        ognps.Remove(ognp);
    }
}
