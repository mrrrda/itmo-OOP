using System.Text.RegularExpressions;

namespace Isu.Models;

public abstract class UniversityActor
{
    protected static readonly string RegexName = "^[a-zA-Zа-яА-Я\\s-]+$";

    protected static readonly int MinNameLength = 1;
    protected static readonly int MaxNameLength = 256;

    private string _fullName;

    public UniversityActor(int id, string fullName)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id), "Id must be greater than 0");

        if (string.IsNullOrWhiteSpace(fullName) || !Regex.IsMatch(fullName, RegexName) || fullName.Length < MinNameLength || fullName.Length > MaxNameLength)
        {
            throw new ArgumentException(
                string.Format("Name must be an alphabetic string with length from {0} to {1}", MinNameLength, MaxNameLength),
                nameof(fullName));
        }

        FormatFullName(fullName);

        Id = id;
        _fullName = fullName;
    }

    public int Id { get; }

    public string FullName
    {
        get => _fullName;

        set
        {
            if (string.IsNullOrEmpty(value) || !Regex.IsMatch(value, RegexName) || value.Length < MinNameLength || value.Length > MaxNameLength)
            {
                throw new ArgumentException(
                    string.Format("Name must be an alphabetic string with length from {0} to {1}", MinNameLength, MaxNameLength),
                    nameof(value));
            }

            FormatFullName(value);
            _fullName = value;
        }
    }

    protected string FormatFullName(string fullName)
    {
        string trimmed = fullName.Trim();
        string[] initials = trimmed.Split(' ');

        for (int i = 0; i < initials.Length; i++)
        {
            string part = initials[i];
            string[] initialParts = part.Split('-');

            for (int j = 0; j < initialParts.Length; j++)
            {
                initialParts[j] = part.Substring(0, 1).ToUpper() + part.Substring(1, part.Length - 1).ToLower();
            }

            initials[i] = string.Join("-", initialParts);
        }

        return string.Join(" ", initials);
    }
}
