using System.Text;
using System.Text.RegularExpressions;

using Isu.Extra.Exceptions;
using Isu.Extra.Models;
using Isu.Models;

namespace Isu.Extra.Entities;

public class Schedule
{
    public static readonly string OddWeekFlag = "O";
    public static readonly string EvenWeekFlag = "E";

    private static readonly string KeypartsSeparator = "_";

    private readonly Dictionary<string, AcademicClass> schedule;

    public Schedule()
    {
        schedule = new Dictionary<string, AcademicClass>();
    }

    public enum DayOfWeek
    {
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday,
        Sunday,
    }

    internal void AddClass(AcademicClass academicClass)
    {
        if (academicClass is null)
            throw new ArgumentNullException(nameof(academicClass), "Invalid academic class");

        string academicClassKey = GetClassKey(
                academicClass.Discipline,
                academicClass.WeekFlag,
                academicClass.DayOfWeek,
                academicClass.Time,
                academicClass.Tutor,
                academicClass.Classroom);

        if (schedule.ContainsKey(academicClassKey))
            throw new ExistingAcademicClassException();

        schedule.Add(academicClassKey, academicClass);
    }

    internal void AddStudyUnitToClass(AcademicClass academicClass, StudyUnit studyUnit)
    {
        if (academicClass is null)
            throw new ArgumentNullException(nameof(academicClass), "Invalid academic class");

        if (studyUnit is null)
            throw new ArgumentNullException(nameof(studyUnit), "Invalid study unit");

        AcademicClass? foundAcademicClass = FindClass(
            academicClass.Discipline,
            academicClass.WeekFlag,
            academicClass.DayOfWeek,
            academicClass.Time,
            academicClass.Tutor,
            academicClass.Classroom);

        if (foundAcademicClass is null)
            throw new NotExistingAcademicClassException();

        List<AcademicClass> studyUnitAcademicClasses = GetClassesListByStudyUnit(studyUnit);

        IEnumerable<AcademicClass> academicClassQuery = studyUnitAcademicClasses.
            Where(studyUnitClass => studyUnitClass.FullTime.Equals(academicClass.FullTime));

        if (academicClassQuery.Any())
            throw new ScheduleIntersectionException();

        foundAcademicClass.AddStudyUnit(studyUnit);
    }

    internal bool CheckClassesIntersection(List<AcademicClass> firstClasses, List<AcademicClass> secondClasses)
    {
        if (firstClasses is null)
            throw new ArgumentNullException(nameof(firstClasses), "Invalid list of classes");

        if (secondClasses is null)
            throw new ArgumentNullException(nameof(secondClasses), "Invalid list of classes");

        foreach (AcademicClass firstClass in firstClasses)
        {
            foreach (AcademicClass secondClass in secondClasses)
            {
                if (firstClass.FullTime.Equals(secondClass.FullTime))
                    return true;
            }
        }

        return false;
    }

    internal void RemoveClass(Discipline discipline)
    {
        List<AcademicClass> foundAcademicClasses = GetClassesListByDiscipline(discipline);

        if (foundAcademicClasses.Count == 0)
            throw new NotExistingDisciplineException();

        string formattedDisciplineName = FormatDisciplineName(discipline);

        var keysToRemove = new List<string>();

        foreach (string academicClassKey in schedule.Keys)
        {
            if (academicClassKey.Contains(formattedDisciplineName))
            {
                var studyUnitsToRemove = new List<StudyUnit>();

                foreach (StudyUnit foundStudyUnit in schedule[academicClassKey].StudyUnits)
                    studyUnitsToRemove.Add(foundStudyUnit);

                keysToRemove.Add(academicClassKey);
                studyUnitsToRemove.ForEach(studyUnit => schedule[academicClassKey].RemoveStudyUnit(studyUnit));
            }
        }

        keysToRemove.ForEach(key => schedule.Remove(key));
    }

    internal void RemoveClassByTime(
        Discipline discipline,
        string weekFlag,
        string dayOfWeek,
        string time)
    {
        List<AcademicClass> foundAcademicClasses = GetClassesListByDisciplineAndFullTime(discipline, weekFlag, dayOfWeek, time);

        if (foundAcademicClasses.Count == 0)
            throw new NotExistingAcademicClassException();

        string formattedDisciplineName = FormatDisciplineName(discipline);

        string academicClassSubKey = new StringBuilder()
            .Append(formattedDisciplineName)
            .Append(KeypartsSeparator)
            .Append(weekFlag)
            .Append(KeypartsSeparator)
            .Append(dayOfWeek)
            .Append(KeypartsSeparator)
            .Append(time)
            .Append(KeypartsSeparator)
            .ToString();

        var keysToRemove = new List<string>();

        foreach (string academicClassKey in schedule.Keys)
        {
            if (academicClassKey.Contains(formattedDisciplineName))
            {
                var studyUnitsToRemove = new List<StudyUnit>();

                foreach (StudyUnit foundStudyUnit in schedule[academicClassKey].StudyUnits)
                    studyUnitsToRemove.Add(foundStudyUnit);

                keysToRemove.Add(academicClassKey);
                studyUnitsToRemove.ForEach(studyUnit => schedule[academicClassKey].RemoveStudyUnit(studyUnit));
            }
        }

        keysToRemove.ForEach(key => schedule.Remove(key));
    }

    internal void DismissAllClassesByStudyUnit(StudyUnit studyUnit)
    {
        List<AcademicClass> foundAcademicClasses = GetClassesListByStudyUnit(studyUnit);

        foreach (AcademicClass academicClass in foundAcademicClasses)
        {
            var studyUnitsToRemove = new List<StudyUnit>();

            foreach (StudyUnit foundStudyUnit in academicClass.StudyUnits)
                studyUnitsToRemove.Add(foundStudyUnit);

            academicClass.RemoveStudyUnit(studyUnit);
            studyUnitsToRemove.ForEach(studyUnit => academicClass.Tutor.RemoveStudyUnit(studyUnit));
        }
    }

    internal void DismissClassByStudyUnit(Discipline discipline, StudyUnit studyUnit)
    {
        List<AcademicClass> foundAcademicClasses = GetClassesListByDisciplineAndStudyUnit(discipline, studyUnit);

        foreach (AcademicClass academicClass in foundAcademicClasses)
        {
            var studyUnitsToRemove = new List<StudyUnit>();

            foreach (StudyUnit foundStudyUnit in academicClass.StudyUnits)
                studyUnitsToRemove.Add(foundStudyUnit);

            academicClass.RemoveStudyUnit(studyUnit);
            studyUnitsToRemove.ForEach(studyUnit => academicClass.Tutor.RemoveStudyUnit(studyUnit));
        }
    }

    internal void DismissClassByStudyUnitAndTime(
        Discipline discipline,
        StudyUnit studyUnit,
        string weekFlag,
        string dayOfWeek,
        string time)
    {
        List<AcademicClass> foundAcademicClasses = GetClassesListByStudyUnitAndTime(discipline, studyUnit, weekFlag, dayOfWeek, time);

        foreach (AcademicClass academicClass in foundAcademicClasses)
        {
            foreach (StudyUnit foundStudyUnit in academicClass.StudyUnits)
                academicClass.Tutor.RemoveStudyUnit(foundStudyUnit);

            academicClass.RemoveStudyUnit(studyUnit);
        }
    }

    internal void ChangeClassSchedule(
        AcademicClass academicClass,
        string newWeekFlag,
        string newDayOfWeek,
        string newTime,
        Tutor newTutor,
        int newClassroom)
    {
        if (academicClass is null)
            throw new ArgumentNullException(nameof(academicClass), "Invalid academic class");

        AcademicClass? foundAcademicClass = FindClass(
            academicClass.Discipline,
            academicClass.WeekFlag,
            academicClass.DayOfWeek,
            academicClass.Time,
            academicClass.Tutor,
            academicClass.Classroom);

        if (foundAcademicClass is null)
            throw new NotExistingAcademicClassException();

        if (string.IsNullOrEmpty(newWeekFlag) || string.IsNullOrWhiteSpace(newWeekFlag))
            throw new ArgumentException("Invalid new week flag", nameof(newWeekFlag));

        if (string.IsNullOrEmpty(newDayOfWeek) || string.IsNullOrWhiteSpace(newDayOfWeek))
            throw new ArgumentException("Invalid new day of week of week", nameof(newDayOfWeek));

        if (string.IsNullOrEmpty(newTime) || !Regex.IsMatch(newTime, AcademicClass.RegexTime))
        {
            throw new ArgumentException(
                string.Format("Expected new time format is: {0}", AcademicClass.ClassTimeFormat),
                nameof(newTime));
        }

        if (newTutor is null)
            throw new ArgumentNullException(nameof(newTutor), "Invalid new tutor");

        if (newClassroom < 0)
            throw new ArgumentOutOfRangeException(nameof(newClassroom), "New classroom number must be positive");

        string oldAcademicClassKey = GetClassKey(
            academicClass.Discipline,
            academicClass.WeekFlag,
            academicClass.DayOfWeek,
            academicClass.Time,
            academicClass.Tutor,
            academicClass.Classroom);

        foreach (StudyUnit studyUnit in foundAcademicClass.StudyUnits)
            academicClass.Tutor.RemoveStudyUnit(studyUnit);

        schedule.Remove(oldAcademicClassKey);

        academicClass.WeekFlag = newWeekFlag;
        academicClass.DayOfWeek = newDayOfWeek;
        academicClass.Time = newTime;
        academicClass.Tutor = newTutor;
        academicClass.Classroom = newClassroom;

        foreach (StudyUnit studyUnit in foundAcademicClass.StudyUnits)
            academicClass.Tutor.AddStudyUnit(studyUnit);

        string newAcademicClassKey = GetClassKey(
            academicClass.Discipline,
            academicClass.WeekFlag,
            academicClass.DayOfWeek,
            academicClass.Time,
            academicClass.Tutor,
            academicClass.Classroom);

        schedule.Add(newAcademicClassKey, academicClass);
    }

    internal void ChangeClassTime(
        AcademicClass academicClass,
        string newWeekFlag,
        string newDayOfWeek,
        string newTime)
    {
        if (academicClass is null)
            throw new ArgumentNullException(nameof(academicClass), "Invalid academic class");

        AcademicClass? foundAcademicClass = FindClass(
            academicClass.Discipline,
            academicClass.WeekFlag,
            academicClass.DayOfWeek,
            academicClass.Time,
            academicClass.Tutor,
            academicClass.Classroom);

        if (foundAcademicClass is null)
            throw new NotExistingAcademicClassException();

        if (string.IsNullOrEmpty(newWeekFlag) || string.IsNullOrWhiteSpace(newWeekFlag))
            throw new ArgumentException("Invalid new week flag", nameof(newWeekFlag));

        if (string.IsNullOrEmpty(newDayOfWeek) || string.IsNullOrWhiteSpace(newDayOfWeek))
            throw new ArgumentException("Invalid new day of week of week", nameof(newDayOfWeek));

        if (string.IsNullOrEmpty(newTime) || !Regex.IsMatch(newTime, AcademicClass.RegexTime))
        {
            throw new ArgumentException(
                string.Format("Expected new time format is: {0}", AcademicClass.ClassTimeFormat),
                nameof(newTime));
        }

        string oldAcademicClassKey = GetClassKey(
            academicClass.Discipline,
            academicClass.WeekFlag,
            academicClass.DayOfWeek,
            academicClass.Time,
            academicClass.Tutor,
            academicClass.Classroom);

        schedule.Remove(oldAcademicClassKey);

        academicClass.WeekFlag = newWeekFlag;
        academicClass.DayOfWeek = newDayOfWeek;
        academicClass.Time = newTime;

        string newAcademicClassKey = GetClassKey(
            academicClass.Discipline,
            academicClass.WeekFlag,
            academicClass.DayOfWeek,
            academicClass.Time,
            academicClass.Tutor,
            academicClass.Classroom);

        schedule.Add(newAcademicClassKey, academicClass);
    }

    internal void ChangeClassTutor(AcademicClass academicClass, Tutor newTutor)
    {
        if (academicClass is null)
            throw new ArgumentNullException(nameof(academicClass), "Invalid academic class");

        AcademicClass? foundAcademicClass = FindClass(
            academicClass.Discipline,
            academicClass.WeekFlag,
            academicClass.DayOfWeek,
            academicClass.Time,
            academicClass.Tutor,
            academicClass.Classroom);

        if (foundAcademicClass is null)
            throw new NotExistingAcademicClassException();

        if (newTutor is null)
            throw new ArgumentNullException(nameof(newTutor), "Invalid new tutor");

        string oldAcademicClassKey = GetClassKey(
            academicClass.Discipline,
            academicClass.WeekFlag,
            academicClass.DayOfWeek,
            academicClass.Time,
            academicClass.Tutor,
            academicClass.Classroom);

        schedule.Remove(oldAcademicClassKey);

        foreach (StudyUnit studyUnit in foundAcademicClass.StudyUnits)
            academicClass.Tutor.RemoveStudyUnit(studyUnit);

        academicClass.Tutor = newTutor;

        foreach (StudyUnit studyUnit in foundAcademicClass.StudyUnits)
            academicClass.Tutor.AddStudyUnit(studyUnit);

        string newAcademicClassKey = GetClassKey(
            academicClass.Discipline,
            academicClass.WeekFlag,
            academicClass.DayOfWeek,
            academicClass.Time,
            academicClass.Tutor,
            academicClass.Classroom);

        schedule.Add(newAcademicClassKey, academicClass);
    }

    internal void ChangeClassRoom(AcademicClass academicClass, int newClassroom)
    {
        if (academicClass is null)
            throw new ArgumentNullException(nameof(academicClass), "Invalid academic class");

        AcademicClass? foundAcademicClass = FindClass(
            academicClass.Discipline,
            academicClass.WeekFlag,
            academicClass.DayOfWeek,
            academicClass.Time,
            academicClass.Tutor,
            academicClass.Classroom);

        if (foundAcademicClass is null)
            throw new NotExistingAcademicClassException();

        if (newClassroom < 0)
            throw new ArgumentOutOfRangeException(nameof(newClassroom), "New classroom number must be positive");

        string oldAcademicClassKey = GetClassKey(
            academicClass.Discipline,
            academicClass.WeekFlag,
            academicClass.DayOfWeek,
            academicClass.Time,
            academicClass.Tutor,
            academicClass.Classroom);

        schedule.Remove(oldAcademicClassKey);

        academicClass.Classroom = newClassroom;

        string newAcademicClassKey = GetClassKey(
            academicClass.Discipline,
            academicClass.WeekFlag,
            academicClass.DayOfWeek,
            academicClass.Time,
            academicClass.Tutor,
            academicClass.Classroom);

        schedule.Add(newAcademicClassKey, academicClass);
    }

    internal AcademicClass? FindClass(
        Discipline discipline,
        string weekFlag,
        string dayOfWeek,
        string time,
        Tutor tutor,
        int classroom)
    {
        if (discipline is null)
            throw new ArgumentNullException(nameof(discipline), "Invalid discipline");

        if (string.IsNullOrEmpty(weekFlag) || string.IsNullOrWhiteSpace(weekFlag))
            throw new ArgumentException("Invalid week flag", nameof(weekFlag));

        if (string.IsNullOrEmpty(dayOfWeek) || string.IsNullOrWhiteSpace(dayOfWeek))
            throw new ArgumentException("Invalid day of week", nameof(dayOfWeek));

        if (string.IsNullOrEmpty(time) || !Regex.IsMatch(time, AcademicClass.RegexTime))
        {
            throw new ArgumentException(
                string.Format("Expected time format is: {0}", AcademicClass.ClassTimeFormat),
                nameof(time));
        }

        if (tutor is null)
            throw new ArgumentNullException(nameof(tutor), "Invalid tutor");

        if (classroom < 0)
            throw new ArgumentOutOfRangeException(nameof(classroom), "Classroom number must be positive");

        string academicClassKey = GetClassKey(
                discipline,
                weekFlag,
                dayOfWeek,
                time,
                tutor,
                classroom);

        schedule.TryGetValue(academicClassKey, out AcademicClass? academicClass);

        return academicClass;
    }

    internal List<AcademicClass> GetClassesListByDisciplineAndFullTime(
        Discipline discipline,
        string weekFlag,
        string dayOfWeek,
        string time)
    {
        if (discipline is null)
            throw new ArgumentNullException(nameof(discipline), "Invalid discipline");

        if (string.IsNullOrEmpty(weekFlag) || string.IsNullOrWhiteSpace(weekFlag))
            throw new ArgumentException("Invalid week flag", nameof(weekFlag));

        if (string.IsNullOrEmpty(dayOfWeek) || string.IsNullOrWhiteSpace(dayOfWeek))
            throw new ArgumentException("Invalid dayOfWeek of week", nameof(dayOfWeek));

        if (string.IsNullOrEmpty(time) || !Regex.IsMatch(time, AcademicClass.RegexTime))
        {
            throw new ArgumentException(
                string.Format("Expected time format is: {0}", AcademicClass.ClassTimeFormat),
                nameof(time));
        }

        var foundClasses = new List<AcademicClass>();

        string formattedDisciplineName = FormatDisciplineName(discipline);

        string academicClassSubKey = new StringBuilder()
            .Append(formattedDisciplineName)
            .Append(KeypartsSeparator)
            .Append(weekFlag)
            .Append(KeypartsSeparator)
            .Append(dayOfWeek)
            .Append(KeypartsSeparator)
            .Append(time)
            .ToString();

        foreach (string academicClassKey in schedule.Keys)
        {
            if (academicClassKey.Contains(academicClassSubKey))
                foundClasses.Add(schedule[academicClassKey]);
        }

        return foundClasses;
    }

    internal List<AcademicClass> GetClassesListByFullTime(
        string weekFlag,
        string dayOfWeek,
        string time)
    {
        if (string.IsNullOrEmpty(weekFlag) || string.IsNullOrWhiteSpace(weekFlag))
            throw new ArgumentException("Invalid week flag", nameof(weekFlag));

        if (string.IsNullOrEmpty(dayOfWeek) || string.IsNullOrWhiteSpace(dayOfWeek))
            throw new ArgumentException("Invalid dayOfWeek of week", nameof(dayOfWeek));

        if (string.IsNullOrEmpty(time) || !Regex.IsMatch(time, AcademicClass.RegexTime))
        {
            throw new ArgumentException(
                string.Format("Expected time format is: {0}", AcademicClass.ClassTimeFormat),
                nameof(time));
        }

        var foundClasses = new List<AcademicClass>();

        string academicClassSubKey = new StringBuilder()
            .Append(weekFlag)
            .Append(KeypartsSeparator)
            .Append(dayOfWeek)
            .Append(KeypartsSeparator)
            .Append(time)
            .ToString();

        foreach (string academicClassKey in schedule.Keys)
        {
            if (academicClassKey.Contains(academicClassSubKey))
                foundClasses.Add(schedule[academicClassKey]);
        }

        return foundClasses;
    }

    internal List<AcademicClass> GetClassesListByDiscipline(Discipline discipline)
    {
        if (discipline is null)
            throw new ArgumentNullException(nameof(discipline), "Invalid discipline");

        return schedule
            .Values
            .Where(academicClass => academicClass.Discipline.Equals(discipline))
            .ToList<AcademicClass>();
    }

    internal List<AcademicClass> GetClassesListByStudyUnit(StudyUnit studyUnit)
    {
        if (studyUnit is null)
            throw new ArgumentNullException(nameof(studyUnit), "Invalid study unit");

        return schedule
            .Values
            .Where(academicClass => academicClass.StudyUnits.Contains(studyUnit))
            .ToList<AcademicClass>();
    }

    internal List<AcademicClass> GetClassesListByDisciplineAndStudyUnit(Discipline discipline, StudyUnit studyUnit)
    {
        if (discipline is null)
            throw new ArgumentNullException(nameof(discipline), "Invalid discipline");

        if (studyUnit is null)
            throw new ArgumentNullException(nameof(studyUnit), "Invalid study unit");

        return schedule
            .Values
            .Where(academicClass => academicClass.Discipline.Equals(discipline)
                && academicClass.StudyUnits.Contains(studyUnit))
            .ToList<AcademicClass>();
    }

    internal List<AcademicClass> GetClassesListByStudyUnitAndTime(
        Discipline discipline,
        StudyUnit studyUnit,
        string weekFlag,
        string dayOfWeek,
        string time)
    {
        if (discipline is null)
            throw new ArgumentNullException(nameof(discipline), "Invalid discipline");

        if (studyUnit is null)
            throw new ArgumentNullException(nameof(studyUnit), "Invalid study unit");

        if (string.IsNullOrEmpty(weekFlag) || string.IsNullOrWhiteSpace(weekFlag))
            throw new ArgumentException("Invalid week flag", nameof(weekFlag));

        if (string.IsNullOrEmpty(dayOfWeek) || string.IsNullOrWhiteSpace(dayOfWeek))
            throw new ArgumentException("Invalid dayOfWeek of week", nameof(dayOfWeek));

        if (string.IsNullOrEmpty(time) || !Regex.IsMatch(time, AcademicClass.RegexTime))
        {
            throw new ArgumentException(
                string.Format("Expected time format is: {0}", AcademicClass.ClassTimeFormat),
                nameof(time));
        }

        var foundClasses = new List<AcademicClass>();

        string formattedDisciplineName = FormatDisciplineName(discipline);

        string academicClassSubKey = new StringBuilder()
            .Append(formattedDisciplineName)
            .Append(KeypartsSeparator)
            .Append(weekFlag)
            .Append(KeypartsSeparator)
            .Append(dayOfWeek)
            .Append(KeypartsSeparator)
            .Append(time)
            .ToString();

        foreach (string academicClassKey in schedule.Keys)
        {
            if (academicClassKey.Contains(academicClassSubKey)
                && schedule[academicClassKey].StudyUnits.Contains(studyUnit))
                foundClasses.Add(schedule[academicClassKey]);
        }

        return foundClasses;
    }

    private string GetClassKey(
        Discipline discipline,
        string weekFlag,
        string dayOfWeek,
        string time,
        Tutor tutor,
        int classroom)
    {
        string formattedDisciplineName = FormatDisciplineName(discipline);

        return new StringBuilder()
            .Append(formattedDisciplineName)
            .Append(KeypartsSeparator)
            .Append(weekFlag)
            .Append(KeypartsSeparator)
            .Append(dayOfWeek)
            .Append(KeypartsSeparator)
            .Append(time)
            .Append(KeypartsSeparator)
            .Append(tutor.Id)
            .Append(KeypartsSeparator)
            .Append(classroom)
            .ToString();
    }

    private string FormatDisciplineName(Discipline discipline)
    {
        return string.Join("-", discipline.Name.Split(string.Empty));
    }
}
