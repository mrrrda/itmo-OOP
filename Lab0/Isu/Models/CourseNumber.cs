using System.Text.RegularExpressions;

namespace Isu.Models;

public class CourseNumber
{
    private static readonly int BachelorsDegreeCode = 3;
    private static readonly int MastersDegreeCode = 4;

    private static readonly int MaxBachelorsCourseNumber = 4;
    private static readonly int MaxMastersCourseNumber = 2;

    private static readonly string RegexNumeric = "^\\d+$";

    public CourseNumber(string degree, string number)
    {
        if (string.IsNullOrEmpty(degree) || !Regex.IsMatch(degree, RegexNumeric) || (int.Parse(degree) != BachelorsDegreeCode && int.Parse(degree) != MastersDegreeCode))
        {
            throw new ArgumentException(
                string.Format("Course degree must be either {0} or {1}", BachelorsDegreeCode, MastersDegreeCode),
                nameof(degree));
        }

        if (string.IsNullOrEmpty(number) || !Regex.IsMatch(number, RegexNumeric))
            throw new ArgumentException("Course number must be a digit", nameof(number));

        int degreeNum = int.Parse(degree);
        int numberNum = int.Parse(number);

        if (degreeNum == BachelorsDegreeCode && (numberNum < 0 || numberNum > MaxBachelorsCourseNumber))
        {
            throw new ArgumentException(
                nameof(degreeNum),
                string.Format("Bachelors course number must be positive and not exceed: {0}", MaxBachelorsCourseNumber));
        }

        if (degreeNum == MastersDegreeCode && (numberNum < 0 || numberNum > MaxMastersCourseNumber))
        {
            throw new ArgumentException(
                nameof(numberNum),
                string.Format("Masters course number must be positive and not exceed: {0}", MaxMastersCourseNumber));
        }

        Degree = int.Parse(degree);
        Number = int.Parse(number);
    }

    public int Degree { get; }

    public int Number { get; }
}
