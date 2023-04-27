using System.Text;

namespace Isu.Models;

public class Group : StudyUnit
{
    private static readonly int MinGroupNameLength = 5;
    private static readonly int MaxGroupNameLength = 7;

    public Group(string groupName)
        : base()
    {
        if (string.IsNullOrEmpty(groupName) || groupName.Length < MinGroupNameLength || groupName.Length > MaxGroupNameLength)
        {
            throw new ArgumentException(
                string.Format("Group name must be a non-null string with length from {0} to {1}", MinGroupNameLength, MaxGroupNameLength),
                nameof(groupName));
        }

        string[] nameParts = new string[groupName.Length];

        for (int i = 0; i < groupName.Length; i++)
            nameParts[i] = char.ToString(groupName[i]);

        string facultyCode = nameParts[0];

        string degreeCode = nameParts[1];
        string courseNumber = nameParts[2];

        string groupNumber = new StringBuilder()
            .Append(nameParts[3])
            .Append(nameParts[4])
            .ToString();

        string specialization = string.Empty;
        string networkingSpecifier = string.Empty;

        if (groupName.Length == GroupName.LengthEitherSpecializationOrNetworking)
        {
            if (char.IsDigit(nameParts[5][0]))
                specialization = nameParts[5];
            else
                networkingSpecifier = nameParts[5];
        }
        else if (groupName.Length == GroupName.LengthBothSpecializationAndNetworking)
        {
            specialization = nameParts[5];
            networkingSpecifier = nameParts[6];
        }

        GroupName = new GroupName(
            facultyCode,
            degreeCode,
            courseNumber,
            groupNumber,
            specialization,
            networkingSpecifier);
    }

    public GroupName GroupName { get; }
}
