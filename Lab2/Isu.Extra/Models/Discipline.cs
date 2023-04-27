using System.Text.RegularExpressions;

namespace Isu.Extra.Models;

public class Discipline
{
    private static readonly string RegexDisciplineName = "^[a-zA-Zа-яА-Я\\s-\\+#]+$";

    private static readonly int MinDisciplineNameLength = 1;
    private static readonly int MaxDisciplineNameLength = 128;

    public Discipline(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || !Regex.IsMatch(name, RegexDisciplineName) || name.Length < MinDisciplineNameLength || name.Length > MaxDisciplineNameLength)
        {
            throw new ArgumentException(
                string.Format("Discipline name must be an alphabetic string with length from {0} to {1}", MinDisciplineNameLength, MaxDisciplineNameLength),
                nameof(name));
        }

        Name = name;
    }

    public string Name { get; }

    public override bool Equals(object? obj)
    {
        return obj is Discipline other
            && Name.Equals(other.Name);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this);
    }
}
