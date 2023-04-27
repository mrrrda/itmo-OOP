using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using Isu.Exceptions;
using Isu.Models;

namespace Isu.Extra.Models;

public class AcademicClass
{
    public static readonly string RegexTime = "^([0-1][0-9]|2[0-3]):([0-5][0-9])$";
    public static readonly string ClassTimeFormat = "HH:mm";

    private static readonly string RegexAlphabetic = "^[a-zA-Zа-яА-Я]+$";

    private readonly List<StudyUnit> studyUnits;

    private string _weekFlag;
    private string _dayOfWeek;
    private string _time;

    private Tutor _tutor;
    private int _classroom;

    public AcademicClass(Discipline discipline, string weekFlag, string dayOfWeek, string time, Tutor tutor, int classroom)
    {
        if (discipline is null)
            throw new ArgumentNullException(nameof(discipline), "Invalid discipline");

        if (string.IsNullOrEmpty(weekFlag) || !Regex.IsMatch(weekFlag, RegexAlphabetic))
            throw new ArgumentException("Week flag must be an alphabetic string", nameof(weekFlag));

        if (string.IsNullOrEmpty(dayOfWeek) || !Regex.IsMatch(dayOfWeek, RegexAlphabetic))
            throw new ArgumentException("Day of week must be an alphabetic string", nameof(dayOfWeek));

        if (string.IsNullOrEmpty(time) || !Regex.IsMatch(time, RegexTime))
        {
            throw new ArgumentException(
                string.Format("Expected time format is: {0}", ClassTimeFormat),
                nameof(time));
        }

        if (tutor is null)
            throw new ArgumentNullException(nameof(tutor), "Invalid tutor");

        if (classroom < 0)
            throw new ArgumentOutOfRangeException(nameof(classroom), "Classroom number must be positive");

        studyUnits = new List<StudyUnit>();

        Discipline = discipline;

        _weekFlag = weekFlag;
        _dayOfWeek = dayOfWeek;
        _time = time;

        _tutor = tutor;
        _classroom = classroom;
    }

    public Discipline Discipline { get; }

    public string FullTime
    {
        get => new StringBuilder()
            .Append(WeekFlag)
            .Append(' ')
            .Append(DayOfWeek)
            .Append(' ')
            .Append(Time)
            .ToString();
    }

    public string WeekFlag
    {
        get => _weekFlag;

        internal set
        {
            if (string.IsNullOrEmpty(value) || !Regex.IsMatch(value, RegexAlphabetic))
                throw new ArgumentException("Week flag must be an alphabetic string", nameof(value));

            _weekFlag = value;
        }
    }

    public string DayOfWeek
    {
        get => _dayOfWeek;

        internal set
        {
            if (string.IsNullOrEmpty(value) || !Regex.IsMatch(value, RegexAlphabetic))
                throw new ArgumentException("Day of week must be an alphabetic string", nameof(value));

            _dayOfWeek = value;
        }
    }

    public string Time
    {
        get => _time;

        internal set
        {
            if (string.IsNullOrEmpty(value) || !Regex.IsMatch(value, RegexTime))
            {
                throw new ArgumentException(
                    string.Format("Expected time format is: {0}", ClassTimeFormat),
                    nameof(value));
            }

            _time = value;
        }
    }

    public Tutor Tutor
    {
        get => _tutor;

        internal set
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value), "Invalid tutor");

            _tutor = value;
        }
    }

    public int Classroom
    {
        get => _classroom;

        internal set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Classroom number must be positive");

            _classroom = value;
        }
    }

    public ReadOnlyCollection<StudyUnit> StudyUnits { get => studyUnits.AsReadOnly(); }

    public void AddStudyUnit(StudyUnit studyUnit)
    {
        if (studyUnit is null)
            throw new ArgumentNullException(nameof(studyUnit), "Invalid study unit");

        studyUnits.Add(studyUnit);
    }

    public void RemoveStudyUnit(StudyUnit studyUnit)
    {
        if (studyUnit is null)
            throw new ArgumentNullException(nameof(studyUnit), "Invalid study unit");

        if (!studyUnits.Contains(studyUnit))
            throw new NotExistingStudyUnitException();

        studyUnits.Remove(studyUnit);
    }

    public override bool Equals(object? obj)
    {
        return obj is AcademicClass other
            && Discipline.Equals(other.Discipline)
            && WeekFlag.Equals(other.WeekFlag)
            && DayOfWeek.Equals(other.DayOfWeek)
            && Time.Equals(other.Time)
            && Tutor.Id == other.Tutor.Id
            && Classroom == other.Classroom;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this);
    }
}
