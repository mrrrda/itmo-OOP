using System.Text;
using System.Text.RegularExpressions;

namespace Isu.Models;

public class GroupName
{
    public static readonly int LengthBothSpecializationAndNetworking = 7;
    public static readonly int LengthEitherSpecializationOrNetworking = 6;

    private static readonly string RegexFacultyCode = "^[A-Z]$";
    private static readonly string RegexGroupNumber = "^\\d{2}$";
    private static readonly string NetworkingSpecifier = "c";

    private static readonly string GroupNumberFormat = "00";

    private static readonly int MaxGroupNumber = 32;
    private static readonly int MaxSpecializationNumber = 5;

    public GroupName(
        string facultyCode,
        string degreeCode,
        string courseNumber,
        string groupNumber,
        string specialization,
        string networkingSpecifier)
    {
        if (string.IsNullOrEmpty(facultyCode) || !Regex.IsMatch(facultyCode, RegexFacultyCode))
            throw new ArgumentException("Faculty code must be an uppercase latin letter", nameof(facultyCode));

        if (string.IsNullOrEmpty(groupNumber) || !Regex.IsMatch(groupNumber, RegexGroupNumber))
            throw new ArgumentException("Group number must be a not-null value and consist of digits only", nameof(groupNumber));

        if (int.Parse(groupNumber) < 0 || int.Parse(groupNumber) > MaxGroupNumber)
        {
            throw new ArgumentException(
                string.Format("Group number must be positive and not exceed: {0}", MaxGroupNumber),
                nameof(groupNumber));
        }

        if (!specialization.Equals(string.Empty) && (!char.IsDigit(specialization[0]) || int.Parse(specialization) < 0 || int.Parse(specialization) > MaxSpecializationNumber))
        {
            throw new ArgumentException(
                string.Format("When specified, group specialization must be positive number and not exceed: {0}", MaxSpecializationNumber),
                nameof(specialization));
        }

        if (!(networkingSpecifier.Equals(string.Empty) || networkingSpecifier.Equals(NetworkingSpecifier)))
        {
            throw new ArgumentException(
                string.Format("When specified, group networking specifier must be must equal {0}", NetworkingSpecifier),
                nameof(networkingSpecifier));
        }

        FacultyCode = facultyCode;
        Course = new CourseNumber(degreeCode, courseNumber);
        GroupNumber = int.Parse(groupNumber);
        Specialization = specialization;
        Networking = networkingSpecifier;
    }

    public string Name
    {
        get
        {
            return new StringBuilder()
                .Append(FacultyCode)
                .Append(Course.Degree)
                .Append(Course.Number)
                .Append(GroupNumber.ToString(GroupNumberFormat))
                .Append(Specialization)
                .Append(Networking)
                .ToString();
        }
    }

    public string FacultyCode { get; }

    public int GroupNumber { get; }

    public string Specialization { get; }

    public string Networking { get; }

    public CourseNumber Course { get; }
}
