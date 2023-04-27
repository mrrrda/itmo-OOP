using System.Text.RegularExpressions;

namespace Isu.Models;

public class Faculty
{
    private static readonly string RegexFacultyCode = "^[A-Z]$";
    private static readonly string RegexFacultyName = "^[a-zA-Zа-яА-Я\\s-]+$";

    private static readonly int MinFacultyNameLength = 1;
    private static readonly int MaxFacultyNameLength = 128;

    private string _name;

    public Faculty(string code, string name)
    {
        if (string.IsNullOrEmpty(code) || !Regex.IsMatch(code, RegexFacultyCode))
            throw new ArgumentException("Faculty code must be an uppercase latin letter", nameof(code));

        if (string.IsNullOrWhiteSpace(name) || !Regex.IsMatch(name, RegexFacultyName) || name.Length < MinFacultyNameLength || name.Length > MaxFacultyNameLength)
        {
            throw new ArgumentException(
                string.Format("Faculty name must be an alphabetic string with length from {0} to {1}", MinFacultyNameLength, MaxFacultyNameLength),
                nameof(name));
        }

        Code = code;
        _name = name;
    }

    public string Code { get; }

    public string Name
    {
        get => _name;

        set
        {
            if (string.IsNullOrEmpty(value) || !Regex.IsMatch(value, RegexFacultyName) || value.Length < MinFacultyNameLength || value.Length > MaxFacultyNameLength)
            {
                throw new ArgumentException(
                string.Format("Faculty name must be an alphabetic string with length from {0} to {1}", MinFacultyNameLength, MaxFacultyNameLength),
                nameof(value));
            }

            _name = value;
        }
    }
}
