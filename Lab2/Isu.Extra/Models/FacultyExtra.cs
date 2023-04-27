using System.Collections.ObjectModel;
using Isu.Extra.Exceptions;

using Isu.Models;

namespace Isu.Extra.Models;

public class FacultyExtra : Faculty
{
    private readonly List<Ognp> ognps;

    public FacultyExtra(string code, string name)
        : base(code, name)
    {
        ognps = new List<Ognp>();
    }

    public ReadOnlyCollection<Ognp> Ognps { get => ognps.AsReadOnly(); }

    public void AddOgnp(Ognp ognp)
    {
        if (ognp is null)
            throw new ArgumentNullException(nameof(ognp), "Invalid ognp");

        ognps.Add(ognp);
    }

    public void RemoveOgnp(Ognp ognp)
    {
        if (ognp is null)
            throw new ArgumentNullException(nameof(ognp), "Invalid ognp");

        if (!ognps.Contains(ognp))
            throw new InvalidOgnpException("Ognp not found");

        ognps.Remove(ognp);
    }
}
