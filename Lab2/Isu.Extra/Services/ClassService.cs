using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

using Isu.Exceptions;
using Isu.Extra.Entities;
using Isu.Extra.Exceptions;
using Isu.Extra.Models;
using Isu.Models;

namespace Isu.Extra.Services;

public class ClassService : IClassService
{
    private static readonly int MaxClassroomNumber = 1024;

    private static readonly int MaxAttendingGroupsAtOnce = 3;
    private static readonly int MaxAttendingStudyStreamsAtOnce = 1;

    private static readonly string FirstClassTime = "08:20";
    private static readonly string LastClassTime = "20:30";

    public ClassService()
    { }

    public AcademicClass AssignClass(
        Schedule schedule,
        Discipline discipline,
        StudyUnit studyUnit,
        string weekFlag,
        string dayOfWeek,
        string time,
        Tutor tutor,
        int classroom)
    {
        if (schedule is null)
            throw new ArgumentNullException(nameof(schedule), "Invalid schedule");

        if (discipline is null)
            throw new ArgumentNullException(nameof(discipline), "Invalid discipline");

        if (studyUnit is null)
            throw new ArgumentNullException(nameof(studyUnit), "Invalid student collection");

        ValidateWeekFlag(weekFlag);
        ValidateDay(dayOfWeek);
        ValidateTime(time);

        if (tutor is null)
            throw new ArgumentNullException(nameof(tutor), "Invalid tutor");

        if (classroom < 0 || classroom > MaxClassroomNumber)
        {
            throw new ArgumentOutOfRangeException(
                nameof(classroom),
                string.Format("Classroom must be positive and not exceed: {0}", MaxClassroomNumber));
        }

        AcademicClass? foundAcademicClass = schedule.FindClass(discipline, weekFlag, dayOfWeek, time, tutor, classroom);

        if (foundAcademicClass is null)
        {
            var academicClass = new AcademicClass(discipline, weekFlag, dayOfWeek, time, tutor, classroom);
            academicClass.AddStudyUnit(studyUnit);

            List<AcademicClass> foundClassesByFullTime = schedule.GetClassesListByFullTime(weekFlag, dayOfWeek, time);

            IEnumerable<AcademicClass> queryByTutor = foundClassesByFullTime
                .Where(classInfo => classInfo.Tutor.Id == tutor.Id);

            if (queryByTutor.Any())
                throw new UnavaliableTutorException(tutor.Id.ToString());

            IEnumerable<AcademicClass> queryByClassroom = foundClassesByFullTime
                .Where(classInfo => classInfo.Classroom == classroom);

            if (queryByClassroom.Any())
                throw new UnavaliableClassroomException(classroom.ToString());

            schedule.AddClass(academicClass);
            tutor.AddStudyUnit(studyUnit);

            return academicClass;
        }
        else
        {
            throw new ExistingAcademicClassException();
        }
    }

    public void AddStudyUnitToClass(
        Schedule schedule,
        AcademicClass academicClass,
        StudyUnit studyUnit)
    {
        if (schedule is null)
            throw new ArgumentNullException(nameof(schedule), "Invalid schedule");

        if (academicClass is null)
            throw new ArgumentNullException(nameof(academicClass), "Invalid academic class");

        if (studyUnit is null)
            throw new ArgumentNullException(nameof(studyUnit), "Invalid student collection");

        AcademicClass? foundAcademicClass = schedule.FindClass(
            academicClass.Discipline,
            academicClass.WeekFlag,
            academicClass.DayOfWeek,
            academicClass.Time,
            academicClass.Tutor,
            academicClass.Classroom);

        if (foundAcademicClass is null)
            throw new NotExistingAcademicClassException();

        if (foundAcademicClass.StudyUnits.Contains(studyUnit))
            throw new DuplicateStudyUnitException();

        if (foundAcademicClass.StudyUnits.Count >= GetMaxAttendingStudyUnitsAtOnce(studyUnit))
            throw new StudyUnitsNumberOverflowException(GetMaxAttendingStudyUnitsAtOnce(studyUnit).ToString());

        schedule.AddStudyUnitToClass(foundAcademicClass, studyUnit);
    }

    public void RemoveClass(Schedule schedule, Discipline discipline)
    {
        if (schedule is null)
            throw new ArgumentNullException(nameof(schedule), "Invalid schedule");

        if (discipline is null)
            throw new ArgumentNullException(nameof(discipline), "Invalid discipline");

        schedule.RemoveClass(discipline);
    }

    public void RemoveClassByTime(Schedule schedule, Discipline discipline, string weekFlag, string dayOfWeek, string time)
    {
        if (schedule is null)
            throw new ArgumentNullException(nameof(schedule), "Invalid schedule");

        if (discipline is null)
            throw new ArgumentNullException(nameof(discipline), "Invalid discipline");

        ValidateWeekFlag(weekFlag);
        ValidateDay(dayOfWeek);
        ValidateTime(time);

        schedule.RemoveClassByTime(discipline, weekFlag, dayOfWeek, time);
    }

    public void DismissAllClassesByStudyUnit(Schedule schedule, StudyUnit studyUnit)
    {
        if (schedule is null)
            throw new ArgumentNullException(nameof(schedule), "Invalid schedule");

        if (studyUnit is null)
            throw new ArgumentNullException(nameof(studyUnit), "Invalid student collection");

        schedule.DismissAllClassesByStudyUnit(studyUnit);
    }

    public void DismissClassByStudyUnit(Schedule schedule, Discipline discipline, StudyUnit studyUnit)
    {
        if (schedule is null)
            throw new ArgumentNullException(nameof(schedule), "Invalid schedule");

        if (discipline is null)
            throw new ArgumentNullException(nameof(discipline), "Invalid discipline");

        if (studyUnit is null)
            throw new ArgumentNullException(nameof(studyUnit), "Invalid student collection");

        schedule.DismissClassByStudyUnit(discipline, studyUnit);
    }

    public void DismissClassByStudyUnitAndTime(
        Schedule schedule,
        Discipline discipline,
        StudyUnit studyUnit,
        string weekFlag,
        string dayOfWeek,
        string time)
    {
        if (schedule is null)
            throw new ArgumentNullException(nameof(schedule), "Invalid schedule");

        if (discipline is null)
            throw new ArgumentNullException(nameof(discipline), "Invalid discipline");

        if (studyUnit is null)
            throw new ArgumentNullException(nameof(studyUnit), "Invalid student collection");

        ValidateWeekFlag(weekFlag);
        ValidateDay(dayOfWeek);
        ValidateTime(time);

        schedule.DismissClassByStudyUnitAndTime(discipline, studyUnit, weekFlag, dayOfWeek, time);
    }

    public void ChangeClassSchedule(
        Schedule schedule,
        AcademicClass academicClass,
        string newWeekFlag,
        string newDayOfWeek,
        string newTime,
        Tutor newTutor,
        int newClassroom)
    {
        if (schedule is null)
            throw new ArgumentNullException(nameof(schedule), "Invalid schedule");

        if (academicClass is null)
            throw new ArgumentNullException(nameof(academicClass), "Invalid academic class");

        ValidateWeekFlag(newWeekFlag);
        ValidateDay(newDayOfWeek);
        ValidateTime(newTime);

        if (newTutor is null)
            throw new ArgumentNullException(nameof(newTutor), "Invalid new tutor");

        if (newClassroom < 0 || newClassroom > MaxClassroomNumber)
        {
            throw new ArgumentOutOfRangeException(
                nameof(newClassroom),
                string.Format("New classroom must be positive and not exceed: {0}", MaxClassroomNumber));
        }

        List<AcademicClass> foundClassesByFullTime = schedule.GetClassesListByFullTime(newWeekFlag, newDayOfWeek, newTime);

        IEnumerable<AcademicClass> queryByTutor = foundClassesByFullTime
                .Where(classInfo => classInfo.Tutor.Id == newTutor.Id);

        if (queryByTutor.Any())
            throw new UnavaliableTutorException(newTutor.Id.ToString());

        IEnumerable<AcademicClass> queryByClassroom = foundClassesByFullTime
                .Where(classInfo => classInfo.Classroom == newClassroom);

        if (queryByClassroom.Any())
            throw new UnavaliableClassroomException(newClassroom.ToString());

        schedule.ChangeClassSchedule(
            academicClass,
            newWeekFlag,
            newDayOfWeek,
            newTime,
            newTutor,
            newClassroom);
    }

    public void ChangeClassTime(
        Schedule schedule,
        AcademicClass academicClass,
        string newWeekFlag,
        string newDayOfWeek,
        string newTime)
    {
        if (schedule is null)
            throw new ArgumentNullException(nameof(schedule), "Invalid schedule");

        if (academicClass is null)
            throw new ArgumentNullException(nameof(academicClass), "Invalid academic class");

        ValidateWeekFlag(newWeekFlag);
        ValidateDay(newDayOfWeek);
        ValidateTime(newTime);

        List<AcademicClass> foundClassesByFullTime = schedule.GetClassesListByFullTime(newWeekFlag, newDayOfWeek, newTime);

        IEnumerable<AcademicClass> queryByTutor = foundClassesByFullTime
                .Where(classInfo => classInfo.Tutor.Id == academicClass.Tutor.Id);

        if (queryByTutor.Any())
            throw new UnavaliableTutorException(academicClass.Tutor.Id.ToString());

        IEnumerable<AcademicClass> queryByClassroom = foundClassesByFullTime
                .Where(classInfo => classInfo.Classroom == academicClass.Classroom);

        if (queryByClassroom.Any())
            throw new UnavaliableClassroomException(academicClass.Classroom.ToString());

        schedule.ChangeClassTime(
            academicClass,
            newWeekFlag,
            newDayOfWeek,
            newTime);
    }

    public void ChangeClassTutor(
        Schedule schedule,
        AcademicClass academicClass,
        Tutor newTutor)
    {
        if (schedule is null)
            throw new ArgumentNullException(nameof(schedule), "Invalid schedule");

        if (academicClass is null)
            throw new ArgumentNullException(nameof(academicClass), "Invalid academic class");

        if (newTutor is null)
            throw new ArgumentNullException(nameof(newTutor), "Invalid new tutor");

        List<AcademicClass> foundClassesByFullTime = schedule.GetClassesListByFullTime(academicClass.WeekFlag, academicClass.DayOfWeek, academicClass.Time);

        IEnumerable<AcademicClass> queryByTutor = foundClassesByFullTime
                .Where(classInfo => classInfo.Tutor.Id == newTutor.Id);

        if (queryByTutor.Any())
            throw new UnavaliableTutorException(newTutor.Id.ToString());

        schedule.ChangeClassTutor(academicClass, newTutor);
    }

    public void ChangeClassRoom(
        Schedule schedule,
        AcademicClass academicClass,
        int newClassroom)
    {
        if (schedule is null)
            throw new ArgumentNullException(nameof(schedule), "Invalid schedule");

        if (academicClass is null)
            throw new ArgumentNullException(nameof(academicClass), "Invalid academic class");

        if (newClassroom < 0 || newClassroom > MaxClassroomNumber)
        {
            throw new ArgumentOutOfRangeException(
                nameof(newClassroom),
                string.Format("New classroom must be positive and not exceed: {0}", MaxClassroomNumber));
        }

        List<AcademicClass> foundClassesByFullTime = schedule.GetClassesListByFullTime(academicClass.WeekFlag, academicClass.DayOfWeek, academicClass.Time);

        IEnumerable<AcademicClass> queryByClassroom = foundClassesByFullTime
                .Where(classInfo => classInfo.Classroom == newClassroom);

        if (queryByClassroom.Any())
            throw new UnavaliableClassroomException(newClassroom.ToString());

        schedule.ChangeClassRoom(academicClass, newClassroom);
    }

    public AcademicClass GetClassByFullInfo(
        Schedule schedule,
        Discipline discipline,
        string weekFlag,
        string dayOfWeek,
        string time,
        Tutor tutor,
        int classroom)
    {
        if (schedule is null)
            throw new ArgumentNullException(nameof(schedule), "Invalid schedule");

        AcademicClass? foundAcademicClass = schedule.FindClass(discipline, weekFlag, dayOfWeek, time, tutor, classroom);

        if (foundAcademicClass is null)
            throw new NotExistingAcademicClassException();

        return foundAcademicClass;
    }

    public ReadOnlyCollection<AcademicClass> GetClassesListByStudyUnit(Schedule schedule, StudyUnit studyUnit)
    {
        if (schedule is null)
            throw new ArgumentNullException(nameof(schedule), "Invalid schedule");

        return schedule.GetClassesListByStudyUnit(studyUnit).AsReadOnly();
    }

    public ReadOnlyCollection<AcademicClass> GetClassesListByDisciplineAndFullTime(
        Schedule schedule,
        Discipline discipline,
        string weekFlag,
        string dayOfWeek,
        string time)
    {
        if (schedule is null)
            throw new ArgumentNullException(nameof(schedule), "Invalid schedule");

        return schedule.GetClassesListByDisciplineAndFullTime(discipline, weekFlag, dayOfWeek, time).AsReadOnly();
    }

    public ReadOnlyCollection<AcademicClass> GetClassesListByFullTime(
        Schedule schedule,
        string weekFlag,
        string dayOfWeek,
        string time)
    {
        if (schedule is null)
            throw new ArgumentNullException(nameof(schedule), "Invalid schedule");

        return schedule.GetClassesListByFullTime(weekFlag, dayOfWeek, time).AsReadOnly();
    }

    public ReadOnlyCollection<AcademicClass> GetClassesListByDiscipline(Schedule schedule, Discipline discipline)
    {
        if (schedule is null)
            throw new ArgumentNullException(nameof(schedule), "Invalid schedule");

        return schedule.GetClassesListByDiscipline(discipline).AsReadOnly();
    }

    public ReadOnlyCollection<AcademicClass> GetClassesListByDisciplineAndStudyUnit(Schedule schedule, Discipline discipline, StudyUnit studyUnit)
    {
        if (schedule is null)
            throw new ArgumentNullException(nameof(schedule), "Invalid schedule");

        return schedule.GetClassesListByDisciplineAndStudyUnit(discipline, studyUnit).AsReadOnly();
    }

    public ReadOnlyCollection<AcademicClass> GetClassesListByStudyUnitAndTime(
        Schedule schedule,
        Discipline discipline,
        StudyUnit studyUnit,
        string weekFlag,
        string dayOfWeek,
        string time)
    {
        if (schedule is null)
            throw new ArgumentNullException(nameof(schedule), "Invalid schedule");

        return schedule.GetClassesListByStudyUnitAndTime(discipline, studyUnit, weekFlag, dayOfWeek, time).AsReadOnly();
    }

// Validation
    private void ValidateWeekFlag(string weekFlag)
    {
        if (!weekFlag.Equals(Schedule.OddWeekFlag) && !weekFlag.Equals(Schedule.EvenWeekFlag))
        {
            throw new ArgumentException(
                string.Format("Week flag can be either {0} or {1}", Schedule.OddWeekFlag, Schedule.EvenWeekFlag),
                nameof(weekFlag));
        }
    }

    private void ValidateDay(string dayOfWeek)
    {
        bool weekDayParseResult = Enum.TryParse(typeof(Schedule.DayOfWeek), dayOfWeek, out object? dayOfWeekKey);

        if (!weekDayParseResult || dayOfWeekKey is null)
        {
            throw new ArgumentException(
                string.Format("The dayOfWeek must be one of the following: {0}, {1}, {2}, {3}, {4}, {5}, {6}", Enum.GetNames(typeof(Schedule.DayOfWeek))),
                nameof(dayOfWeek));
        }

        if (dayOfWeek.Equals(Schedule.DayOfWeek.Sunday.ToString()))
        {
            throw new ArgumentException(
                string.Format("Academic class can't be conducted on {0}", Schedule.DayOfWeek.Sunday),
                nameof(dayOfWeek));
        }
    }

    private void ValidateTime(string time)
    {
        if (!Regex.IsMatch(time, AcademicClass.RegexTime))
        {
            throw new ArgumentException(
                string.Format("Expected time format is: {0}", AcademicClass.ClassTimeFormat),
                nameof(time));
        }

        int firstCompareCoefficent = string.Compare(time, FirstClassTime);
        int secondCompareCoefficent = string.Compare(time, LastClassTime);

        if (firstCompareCoefficent == -1 || secondCompareCoefficent == 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(time),
                string.Format("The academic class time can be conducted between {0} and {1}", FirstClassTime, LastClassTime));
        }
    }

    private int GetMaxAttendingStudyUnitsAtOnce(StudyUnit studyUnit)
    {
        return studyUnit is Isu.Models.Group ? MaxAttendingGroupsAtOnce : MaxAttendingStudyStreamsAtOnce;
    }
}
