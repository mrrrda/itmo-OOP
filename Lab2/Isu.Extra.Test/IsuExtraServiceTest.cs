using System.Collections.ObjectModel;

using Isu.Exceptions;
using Isu.Extra.Entities;
using Isu.Extra.Exceptions;
using Isu.Extra.Models;
using Isu.Extra.Services;
using Isu.Models;

using Xunit;

namespace Isu.Extra.Test;

public class IsuExtraServiceTest
{
    [Fact]
    public void AddFaculty_FacultyHasOgnpList()
    {
        var isuService = new IsuExtraService();

        Faculty faculty = isuService.AddFaculty("C", "Computer Science");

        Assert.True(faculty is FacultyExtra);

        var facultyExtra = faculty as FacultyExtra;

        ReadOnlyCollection<Ognp>? facultyOgnps = facultyExtra?.Ognps;

        Assert.NotNull(facultyOgnps);
        Assert.Empty(facultyOgnps);
    }

    [Fact]
    public void AddStudent_StudentOgnpIsNull()
    {
        var isuService = new IsuExtraService();

        isuService.AddFaculty("C", "Computer Science");
        Group group = isuService.AddGroup("C32121");
        Student student = isuService.AddStudent(group, "Mariya Izerakova");

        Assert.True(student is StudentExtra);

        var studentExtra = student as StudentExtra;

        Assert.NotNull(studentExtra);
        Assert.Empty(studentExtra?.Ognps);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("?")]
    [InlineData("Physics?")]
    [InlineData("Physics1")]
    public void AddDisciplineWithInvalidName_ThrowException(string name)
    {
        var isuService = new IsuExtraService();

        Assert.Throws<ArgumentException>(() => isuService.AddDiscipline(name));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("1")]
    [InlineData("M3")]
    [InlineData("?")]
    [InlineData("Mariya 5")]
    [InlineData("Mariya Izerakova?")]
    public void CreateTutorWithInvalidName_ThrowException(string fullName)
    {
        Assert.Throws<ArgumentException>(() => new Tutor(0, fullName));
    }

    [Fact]
    public void ReachMaxTutorPerUniversity_ThrowException()
    {
        var isuService = new IsuExtraService();

        for (int i = 0; i < IsuExtraService.MaxTutorsNumber; i++)
            isuService.AddTutor("Mariya Izerakova");

        Assert.Throws<TutorsNumberOverflowException>(() => isuService.AddTutor("Mariya Izerakova"));
    }

    [Fact]
    public void AddDuplicateDiscipline_ThrowException()
    {
        var isuService = new IsuExtraService();

        isuService.AddDiscipline("Physics");

        Assert.Throws<DuplicateDisciplineException>(() => isuService.AddDiscipline("Physics"));
    }

    [Fact]
    public void AddOgnpToFaculty_OgnpIsAdded()
    {
        var isuService = new IsuExtraService();

        Faculty faculty = isuService.AddFaculty("C", "Computer science");

        Discipline firstDscipline = isuService.AddDiscipline("Physics");
        Discipline secondDiscipline = isuService.AddDiscipline("Math");

        Ognp ognp = isuService.AddOgnp(faculty, firstDscipline, secondDiscipline);

        Ognp? foundOgnp = isuService.FindOgnp(firstDscipline, secondDiscipline);

        var facultyExtra = faculty as FacultyExtra;
        ReadOnlyCollection<Ognp>? facultyOgnps = facultyExtra?.Ognps;

        Assert.True(facultyOgnps?.Count == 1);
        Assert.Contains(ognp, facultyOgnps);
        Assert.NotNull(foundOgnp);
        Assert.Equal(ognp, foundOgnp);
    }

    [Fact]
    public void AddOgnp_ThrowFacultyOgnpOverflowException()
    {
        var isuService = new IsuExtraService();

        Faculty faculty = isuService.AddFaculty("C", "Computer science");

        Discipline firstDscipline = isuService.AddDiscipline("Physics");
        Discipline secondDiscipline = isuService.AddDiscipline("Math");
        Discipline thrirdDiscipline = isuService.AddDiscipline("OOP");
        Discipline fourthDiscipline = isuService.AddDiscipline("Discrete Math");

        Discipline fifthDiscipline = isuService.AddDiscipline("C++");
        Discipline sixthDiscipline = isuService.AddDiscipline("Java");

        isuService.AddOgnp(faculty, firstDscipline, secondDiscipline);
        isuService.AddOgnp(faculty, thrirdDiscipline, fourthDiscipline);

        Assert.Throws<OgnpsNumberOverflowException>(() => isuService.AddOgnp(faculty, fifthDiscipline, sixthDiscipline));
    }

    [Fact]
    public void AddOgnp_ThrowDuplicateOgnpException()
    {
        var isuService = new IsuExtraService();

        Faculty faculty = isuService.AddFaculty("C", "Computer science");

        Discipline firstDscipline = isuService.AddDiscipline("Physics");
        Discipline secondDiscipline = isuService.AddDiscipline("Math");

        isuService.AddOgnp(faculty, firstDscipline, secondDiscipline);

        Assert.Throws<DuplicateOgnpException>(() => isuService.AddOgnp(faculty, firstDscipline, secondDiscipline));
    }

    [Fact]
    public void AddStudyStreamToOgnp_StudyStreamIsAdded()
    {
        var isuService = new IsuExtraService();

        Faculty faculty = isuService.AddFaculty("C", "Computer science");

        Discipline firstDscipline = isuService.AddDiscipline("Physics");
        Discipline secondDiscipline = isuService.AddDiscipline("Math");

        Ognp ognp = isuService.AddOgnp(faculty, firstDscipline, secondDiscipline);
        var studyStream = new StudyStream();

        isuService.AddStudyStream(ognp, studyStream);

        Assert.Contains(studyStream, ognp.StudyStreams);
    }

    [Fact]
    public void AddStudyStreamToOgnp_ThrowDuplicateStudyUnitException()
    {
        var schedule = new Schedule();
        var classService = new ClassService();
        var isuService = new IsuExtraService();

        Faculty faculty = isuService.AddFaculty("C", "Computer science");

        Discipline firstDscipline = isuService.AddDiscipline("Physics");
        Discipline secondDiscipline = isuService.AddDiscipline("Math");

        Ognp ognp = isuService.AddOgnp(faculty, firstDscipline, secondDiscipline);

        var firstStudyStream = new StudyStream();
        isuService.AddStudyStream(ognp, firstStudyStream);

        Assert.Throws<DuplicateStudyUnitException>(() => isuService.AddStudyStream(ognp, firstStudyStream));
    }

    [Fact]
    public void AssignClass_ClassAssigned()
    {
        var schedule = new Schedule();
        var isuService = new IsuExtraService();
        var classService = new ClassService();

        isuService.AddFaculty("C", "Computer science");

        Group group = isuService.AddGroup("C32121");

        Discipline discipline = isuService.AddDiscipline("Physics");

        Tutor tutor = isuService.AddTutor("Mariya Izerakova");

        AcademicClass academicClass = classService.AssignClass(schedule, discipline, group, "E", "Monday", "08:20", tutor, 1);

        ReadOnlyCollection<AcademicClass> classes = classService.GetClassesListByStudyUnit(schedule, group);

        Assert.NotEmpty(classes);
        Assert.Equal(classes[0], academicClass);
    }

    [Fact]
    public void AssignClass_ThrowExistingClassException()
    {
        var schedule = new Schedule();
        var isuService = new IsuExtraService();
        var classService = new ClassService();

        isuService.AddFaculty("C", "Computer science");

        Group firstGroup = isuService.AddGroup("C32121");
        Group secondGroup = isuService.AddGroup("C32122");

        Discipline discipline = isuService.AddDiscipline("Physics");

        Tutor tutor = isuService.AddTutor("Mariya Izerakova");

        AcademicClass academicClass = classService.AssignClass(schedule, discipline, firstGroup, "E", "Monday", "08:20", tutor, 1);

        Assert.Throws<ExistingAcademicClassException>(() => classService.AssignClass(schedule, discipline, firstGroup, "E", "Monday", "08:20", tutor, 1));
        Assert.Throws<ExistingAcademicClassException>(() => classService.AssignClass(schedule, discipline, secondGroup, "E", "Monday", "08:20", tutor, 1));
    }

    [Fact]
    public void AssignClass_ThrowUnavaliableTutorException()
    {
        var schedule = new Schedule();
        var isuService = new IsuExtraService();
        var classService = new ClassService();

        Faculty faculty = isuService.AddFaculty("C", "Computer science");

        Discipline firstDscipline = isuService.AddDiscipline("Physics");
        Discipline secondDiscipline = isuService.AddDiscipline("Math");

        Tutor tutor = isuService.AddTutor("Mariya Izerakova");

        Ognp ognp = isuService.AddOgnp(faculty, firstDscipline, secondDiscipline);

        var firstStudyStream = new StudyStream();
        var secondStudyStream = new StudyStream();

        isuService.AddStudyStream(ognp, firstStudyStream);
        isuService.AddStudyStream(ognp, secondStudyStream);

        classService.AssignClass(schedule, firstDscipline, ognp.StudyStreams[0], "E", "Monday", "08:20", tutor, 1);

        Assert.Throws<UnavaliableTutorException>(() => classService.AssignClass(schedule, firstDscipline, ognp.StudyStreams[1], "E", "Monday", "08:20", tutor, 2));
    }

    [Fact]
    public void AssignClass_ThrowUnavaliableClassroomException()
    {
        var schedule = new Schedule();
        var isuService = new IsuExtraService();
        var classService = new ClassService();

        Faculty faculty = isuService.AddFaculty("C", "Computer science");

        Discipline firstDscipline = isuService.AddDiscipline("Physics");
        Discipline secondDiscipline = isuService.AddDiscipline("Math");

        Tutor firstTutor = isuService.AddTutor("Mariya Izerakova");
        Tutor secondTutor = isuService.AddTutor("Elena Izerakova");

        Ognp ognp = isuService.AddOgnp(faculty, firstDscipline, secondDiscipline);

        var firstStudyStream = new StudyStream();
        var secondStudyStream = new StudyStream();

        isuService.AddStudyStream(ognp, firstStudyStream);
        isuService.AddStudyStream(ognp, secondStudyStream);

        classService.AssignClass(schedule, firstDscipline, ognp.StudyStreams[0], "E", "Monday", "08:20", firstTutor, 1);

        Assert.Throws<UnavaliableClassroomException>(() => classService.AssignClass(schedule, firstDscipline, ognp.StudyStreams[1], "E", "Monday", "08:20", secondTutor, 1));
    }

    [Fact]
    public void AddStudyUnitToClass_StudyUnitAdded()
    {
        var schedule = new Schedule();
        var isuService = new IsuExtraService();
        var classService = new ClassService();

        isuService.AddFaculty("C", "Computer science");

        Group firstGroup = isuService.AddGroup("C32121");
        Group secondGroup = isuService.AddGroup("C32122");

        Discipline discipline = isuService.AddDiscipline("Physics");

        Tutor tutor = isuService.AddTutor("Mariya Izerakova");

        AcademicClass academicClass = classService.AssignClass(schedule, discipline, firstGroup, "E", "Monday", "08:20", tutor, 1);
        classService.AddStudyUnitToClass(schedule, academicClass, secondGroup);

        Assert.Contains(secondGroup, academicClass.StudyUnits);
    }

    [Fact]
    public void AddStudyUnitToClass_ThrowDuplicateStudyUnitException()
    {
        var schedule = new Schedule();
        var isuService = new IsuExtraService();
        var classService = new ClassService();

        isuService.AddFaculty("C", "Computer science");

        Group group = isuService.AddGroup("C32121");

        Discipline discipline = isuService.AddDiscipline("Physics");

        Tutor tutor = isuService.AddTutor("Mariya Izerakova");

        AcademicClass academicClass = classService.AssignClass(schedule, discipline, group, "E", "Monday", "08:20", tutor, 1);

        Assert.Throws<DuplicateStudyUnitException>(() => classService.AddStudyUnitToClass(schedule, academicClass, group));
    }

    [Fact]
    public void AddStudyUnitToClass_ThrowNotExistingClassException()
    {
        var schedule = new Schedule();
        var isuService = new IsuExtraService();
        var classService = new ClassService();

        isuService.AddFaculty("C", "Computer science");

        Group group = isuService.AddGroup("C32121");

        Discipline discipline = isuService.AddDiscipline("Physics");

        Tutor tutor = isuService.AddTutor("Mariya Izerakova");

        AcademicClass academicClass = classService.AssignClass(schedule, discipline, group, "E", "Monday", "08:20", tutor, 1);
        classService.RemoveClass(schedule, discipline);

        Assert.Throws<NotExistingAcademicClassException>(() => classService.AddStudyUnitToClass(schedule, academicClass, group));
    }

    [Fact]
    public void ChangeClassSchedule_ScheduleChanged()
    {
        var schedule = new Schedule();
        var isuService = new IsuExtraService();
        var classService = new ClassService();

        isuService.AddFaculty("C", "Computer science");

        Group group = isuService.AddGroup("C32121");

        Discipline discipline = isuService.AddDiscipline("Physics");

        string oldWeekFlag = "E";
        string newWeekFlag = "O";

        string oldDayOfWeek = "Monday";
        string newDayOfWeek = "Friday";

        string oldTime = "08:20";
        string newTime = "12:00";

        Tutor oldTutor = isuService.AddTutor("Mariya Izerakova");
        Tutor newTutor = isuService.AddTutor("Elena Izerakova");

        int oldClassroom = 1;
        int newClassroom = 2;

        AcademicClass academicClass = classService.AssignClass(schedule, discipline, group, oldWeekFlag, oldDayOfWeek, oldTime, oldTutor, oldClassroom);

        classService.ChangeClassSchedule(schedule, academicClass, newWeekFlag, newDayOfWeek, newTime, newTutor, newClassroom);

        AcademicClass foundClass = classService.GetClassByFullInfo(schedule, discipline, newWeekFlag, newDayOfWeek, newTime, newTutor, newClassroom);

        Assert.Equal(newWeekFlag, academicClass.WeekFlag);
        Assert.Equal(newDayOfWeek, academicClass.DayOfWeek);
        Assert.Equal(newTime, academicClass.Time);
        Assert.Equal(newTutor, academicClass.Tutor);
        Assert.Equal(newClassroom, academicClass.Classroom);

        Assert.Equal(foundClass, academicClass);

        Assert.Equal(newTutor, academicClass.Tutor);
        Assert.DoesNotContain(group, oldTutor.StudyUnits);
        Assert.Contains(group, newTutor.StudyUnits);
    }

    [Fact]
    public void ChangeClassTime_TimeChanged()
    {
        var schedule = new Schedule();
        var isuService = new IsuExtraService();
        var classService = new ClassService();

        isuService.AddFaculty("C", "Computer science");

        Group group = isuService.AddGroup("C32121");

        Discipline discipline = isuService.AddDiscipline("Physics");

        Tutor tutor = isuService.AddTutor("Mariya Izerakova");

        AcademicClass academicClass = classService.AssignClass(schedule, discipline, group, "E", "Monday", "08:20", tutor, 1);

        classService.ChangeClassTime(schedule, academicClass, "O", "Monday", "08:20");

        Assert.Equal("O", academicClass.WeekFlag);
        Assert.Equal("Monday", academicClass.DayOfWeek);
        Assert.Equal("08:20", academicClass.Time);

        ReadOnlyCollection<AcademicClass> initialClasses = classService.GetClassesListByFullTime(schedule, "E", "Monday", "08:20");
        ReadOnlyCollection<AcademicClass> foundClasses = classService.GetClassesListByFullTime(schedule, "O", "Monday", "08:20");

        Assert.Empty(initialClasses);
        Assert.NotEmpty(foundClasses);
        Assert.Equal(foundClasses[0], academicClass);
    }

    [Fact]
    public void ChangeClassTutor_TutorChanged()
    {
        var schedule = new Schedule();
        var isuService = new IsuExtraService();
        var classService = new ClassService();

        isuService.AddFaculty("C", "Computer science");

        Group group = isuService.AddGroup("C32121");

        Discipline discipline = isuService.AddDiscipline("Physics");

        Tutor oldTutor = isuService.AddTutor("Mariya Izerakova");
        Tutor newTutor = isuService.AddTutor("Elena Izerakova");

        AcademicClass academicClass = classService.AssignClass(schedule, discipline, group, "E", "Monday", "08:20", oldTutor, 1);

        classService.ChangeClassTutor(schedule, academicClass, newTutor);

        Assert.Equal(newTutor, academicClass.Tutor);
        Assert.DoesNotContain(group, oldTutor.StudyUnits);
        Assert.Contains(group, newTutor.StudyUnits);
    }

    [Fact]
    public void ChangeClassRoom_ClassroomChanged()
    {
        var schedule = new Schedule();
        var isuService = new IsuExtraService();
        var classService = new ClassService();

        isuService.AddFaculty("C", "Computer science");

        Group group = isuService.AddGroup("C32121");

        Discipline discipline = isuService.AddDiscipline("Physics");

        Tutor tutor = isuService.AddTutor("Mariya Izerakova");

        int oldClassroom = 1;
        int newClassroom = 2;

        AcademicClass academicClass = classService.AssignClass(schedule, discipline, group, "E", "Monday", "08:20", tutor, oldClassroom);

        classService.ChangeClassRoom(schedule, academicClass, newClassroom);

        Assert.Equal(newClassroom, academicClass.Classroom);
    }

    [Fact]
    public void ChangeClassSchedule_ThrowException()
    {
        var schedule = new Schedule();
        var isuService = new IsuExtraService();
        var classService = new ClassService();

        isuService.AddFaculty("C", "Computer science");

        Group firstGroup = isuService.AddGroup("C32121");
        Group secondGroup = isuService.AddGroup("C32122");
        Group thirdGroup = isuService.AddGroup("C32123");

        Discipline discipline = isuService.AddDiscipline("Physics");

        string oldWeekFlag = "E";
        string newWeekFlag = "O";

        string oldDayOfWeek = "Monday";
        string newDayOfWeek = "Friday";

        string oldTime = "08:20";
        string newTime = "12:00";

        Tutor firstTutor = isuService.AddTutor("Mariya Izerakova");
        Tutor secondTutor = isuService.AddTutor("Elena Izerakova");
        Tutor thirdTutor = isuService.AddTutor("Nikita Izerakov");

        int oldClassroom = 1;
        int newClassroom = 2;

        AcademicClass firstAcademicClass = classService.AssignClass(schedule, discipline, firstGroup, oldWeekFlag, oldDayOfWeek, oldTime, firstTutor, oldClassroom);
        classService.AssignClass(schedule, discipline, secondGroup, newWeekFlag, newDayOfWeek, newTime, secondTutor, oldClassroom);
        classService.AssignClass(schedule, discipline, thirdGroup, newWeekFlag, newDayOfWeek, newTime, firstTutor, newClassroom);

        Assert.Throws<UnavaliableTutorException>(() => classService.ChangeClassSchedule(schedule, firstAcademicClass, newWeekFlag, newDayOfWeek, newTime, secondTutor, newClassroom));
        Assert.Throws<UnavaliableClassroomException>(() => classService.ChangeClassSchedule(schedule, firstAcademicClass, newWeekFlag, newDayOfWeek, newTime, thirdTutor, oldClassroom));
    }

    [Fact]
    public void ChangeClassTutor_ThrowUnavaliableTutorException()
    {
        var schedule = new Schedule();
        var isuService = new IsuExtraService();
        var classService = new ClassService();

        isuService.AddFaculty("C", "Computer science");

        Group firstGroup = isuService.AddGroup("C32121");
        Group secondGroup = isuService.AddGroup("C32122");

        Discipline discipline = isuService.AddDiscipline("Physics");

        Tutor oldTutor = isuService.AddTutor("Mariya Izerakova");
        Tutor newTutor = isuService.AddTutor("Elena Izerakova");

        AcademicClass firstAcademicClass = classService.AssignClass(schedule, discipline, firstGroup, "E", "Monday", "08:20", oldTutor, 1);
        classService.AssignClass(schedule, discipline, secondGroup, "E", "Monday", "08:20", newTutor, 2);

        Assert.Throws<UnavaliableTutorException>(() => classService.ChangeClassTutor(schedule, firstAcademicClass, newTutor));
    }

    [Fact]
    public void ChangeClassRoom_ThrowUnavaliableClassroomException()
    {
        var schedule = new Schedule();
        var isuService = new IsuExtraService();
        var classService = new ClassService();

        isuService.AddFaculty("C", "Computer science");

        Group firstGroup = isuService.AddGroup("C32121");
        Group secondGroup = isuService.AddGroup("C32122");

        Discipline discipline = isuService.AddDiscipline("Physics");

        Tutor firstTutor = isuService.AddTutor("Mariya Izerakova");
        Tutor secondTutor = isuService.AddTutor("Elena Izerakova");

        int oldClassroom = 1;
        int newClassroom = 2;

        AcademicClass firstAcademicClass = classService.AssignClass(schedule, discipline, firstGroup, "E", "Monday", "08:20", firstTutor, oldClassroom);
        classService.AssignClass(schedule, discipline, secondGroup, "E", "Monday", "08:20", secondTutor, newClassroom);

        Assert.Throws<UnavaliableClassroomException>(() => classService.ChangeClassRoom(schedule, firstAcademicClass, newClassroom));
    }

    [Fact]
    public void RemoveClass_ClassRemoved()
    {
        var schedule = new Schedule();
        var isuService = new IsuExtraService();
        var classService = new ClassService();

        isuService.AddFaculty("C", "Computer science");

        Group group = isuService.AddGroup("C32121");

        Discipline discipline = isuService.AddDiscipline("Physics");

        Tutor tutor = isuService.AddTutor("Mariya Izerakova");

        classService.AssignClass(schedule, discipline, group, "E", "Monday", "08:20", tutor, 1);

        classService.RemoveClass(schedule, discipline);

        ReadOnlyCollection<AcademicClass> foundClasses = classService.GetClassesListByDiscipline(schedule, discipline);

        Assert.Empty(foundClasses);
    }

    [Fact]
    public void RemoveClass_ThrowNotExistingDisciplineException()
    {
        var schedule = new Schedule();
        var classService = new ClassService();

        var discipline = new Discipline("Physics");

        Assert.Throws<NotExistingDisciplineException>(() => classService.RemoveClass(schedule, discipline));
    }

    [Fact]
    public void RemoveClassByTime_ClassRemoved()
    {
        var schedule = new Schedule();
        var isuService = new IsuExtraService();
        var classService = new ClassService();

        isuService.AddFaculty("C", "Computer science");

        Group group = isuService.AddGroup("C32121");

        Discipline discipline = isuService.AddDiscipline("Physics");

        Tutor tutor = isuService.AddTutor("Mariya Izerakova");

        classService.AssignClass(schedule, discipline, group, "E", "Monday", "08:20", tutor, 1);
        classService.AssignClass(schedule, discipline, group, "O", "Monday", "08:20", tutor, 1);

        classService.RemoveClassByTime(schedule, discipline, "E", "Monday", "08:20");

        ReadOnlyCollection<AcademicClass> foundClasses = classService.GetClassesListByDisciplineAndFullTime(schedule, discipline, "E", "Monday", "08:20");

        Assert.Empty(foundClasses);
    }

    [Fact]
    public void RemoveClassByTime_ThrowNotExistingClassException()
    {
        var schedule = new Schedule();
        var isuService = new IsuExtraService();
        var classService = new ClassService();

        isuService.AddFaculty("C", "Computer science");

        Group group = isuService.AddGroup("C32121");

        Discipline discipline = isuService.AddDiscipline("Physics");

        Tutor tutor = isuService.AddTutor("Mariya Izerakova");

        classService.AssignClass(schedule, discipline, group, "E", "Monday", "08:20", tutor, 1);

        Assert.Throws<NotExistingAcademicClassException>(() => classService.RemoveClassByTime(schedule, discipline, "O", "Monday", "08:20"));
    }

    [Fact]
    public void DismissAllClassesByStudyUnit_ClassesDismissed()
    {
        var schedule = new Schedule();
        var isuService = new IsuExtraService();
        var classService = new ClassService();

        isuService.AddFaculty("C", "Computer science");

        Group group = isuService.AddGroup("C32121");

        Discipline firstDiscipline = isuService.AddDiscipline("Physics");
        Discipline secondDiscipline = isuService.AddDiscipline("Math");

        Tutor tutor = isuService.AddTutor("Mariya Izerakova");

        classService.AssignClass(schedule, firstDiscipline, group, "E", "Monday", "08:20", tutor, 1);
        classService.AssignClass(schedule, secondDiscipline, group, "O", "Monday", "08:20", tutor, 1);

        classService.DismissAllClassesByStudyUnit(schedule, group);

        ReadOnlyCollection<AcademicClass> foundClasses = classService.GetClassesListByStudyUnit(schedule, group);

        Assert.Empty(foundClasses);
        Assert.DoesNotContain(group, tutor.StudyUnits);
    }

    [Fact]
    public void DismissClassByStudyUnit_ClassDismissed()
    {
        var schedule = new Schedule();
        var isuService = new IsuExtraService();
        var classService = new ClassService();

        isuService.AddFaculty("C", "Computer science");

        Group group = isuService.AddGroup("C32121");

        Discipline firstDiscipline = isuService.AddDiscipline("Physics");
        Discipline secondDiscipline = isuService.AddDiscipline("Math");

        Tutor tutor = isuService.AddTutor("Mariya Izerakova");

        classService.AssignClass(schedule, firstDiscipline, group, "E", "Monday", "08:20", tutor, 1);
        AcademicClass secondAcademicClass = classService.AssignClass(schedule, secondDiscipline, group, "O", "Monday", "08:20", tutor, 1);

        classService.DismissClassByStudyUnit(schedule, firstDiscipline, group);

        ReadOnlyCollection<AcademicClass> foundClasses = classService.GetClassesListByStudyUnit(schedule, group);
        ReadOnlyCollection<AcademicClass> foundClassesByFirstDiscipline = classService.GetClassesListByDisciplineAndStudyUnit(schedule, firstDiscipline, group);
        ReadOnlyCollection<AcademicClass> foundClassesBySecondDiscipline = classService.GetClassesListByDisciplineAndStudyUnit(schedule, secondDiscipline, group);

        Assert.NotEmpty(foundClasses);
        Assert.NotEmpty(foundClassesBySecondDiscipline);
        Assert.Empty(foundClassesByFirstDiscipline);

        Assert.Equal(secondAcademicClass, foundClasses[0]);
    }

    [Fact]
    public void DismissClassByStudyUnitAndTime_ClassDismissed()
    {
        var schedule = new Schedule();
        var isuService = new IsuExtraService();
        var classService = new ClassService();

        isuService.AddFaculty("C", "Computer science");

        Group group = isuService.AddGroup("C32121");

        Discipline firstDiscipline = isuService.AddDiscipline("Physics");
        Discipline secondDiscipline = isuService.AddDiscipline("Math");

        Tutor firstTutor = isuService.AddTutor("Mariya Izerakova");
        Tutor secondTutor = isuService.AddTutor("Mariya Izerakova");

        classService.AssignClass(schedule, firstDiscipline, group, "E", "Monday", "08:20", firstTutor, 1);
        AcademicClass secondAcademicClass = classService.AssignClass(schedule, secondDiscipline, group, "E", "Monday", "08:20", secondTutor, 2);

        classService.DismissClassByStudyUnitAndTime(schedule, firstDiscipline, group, "E", "Monday", "08:20");

        ReadOnlyCollection<AcademicClass> foundClasses = classService.GetClassesListByStudyUnit(schedule, group);
        IReadOnlyCollection<AcademicClass> foundClassesByFirstDisciplineAndTime = classService.GetClassesListByStudyUnitAndTime(schedule, firstDiscipline, group, "E", "Monday", "08:20");
        IReadOnlyCollection<AcademicClass> foundClassesBySecondDisciplineAndTime = classService.GetClassesListByStudyUnitAndTime(schedule, secondDiscipline, group, "E", "Monday", "08:20");

        Assert.NotEmpty(foundClasses);
        Assert.Empty(foundClassesByFirstDisciplineAndTime);
        Assert.NotEmpty(foundClassesBySecondDisciplineAndTime);
        Assert.Equal(secondAcademicClass, foundClasses[0]);
    }

    [Fact]
    public void AddStudentToOgnp_StudentHasOgnpAndOgnpContainsStudent()
    {
        var schedule = new Schedule();
        var isuService = new IsuExtraService();
        var classService = new ClassService();

        isuService.AddFaculty("C", "Computer science");
        Faculty secondFaculty = isuService.AddFaculty("M", "Math");

        Group group = isuService.AddGroup("C32121");
        Student student = isuService.AddStudent(group, "Mariya Izerakova");
        Tutor tutor = isuService.AddTutor("Mariya Izerakova");

        Discipline firstDiscipline = isuService.AddDiscipline("Physics");
        Discipline secondDiscipline = isuService.AddDiscipline("Math");

        Ognp ognp = isuService.AddOgnp(secondFaculty, firstDiscipline, secondDiscipline);

        var studyStream = new StudyStream();

        classService.AssignClass(schedule, firstDiscipline, studyStream, "O", "Monday", "08:20", tutor, 1);

        isuService.AddStudyStream(ognp, studyStream);

        isuService.AddStudentToOgnp(schedule, student, ognp);

        var studentExtra = student as StudentExtra;

        Assert.Equal(ognp, studentExtra?.Ognps[0]);
        Assert.Contains(student, ognp.StudyStreams[0].Students);
        Assert.Equal(student, ognp.StudyStreams[0].Students[0]);
    }

    [Fact]
    public void AddStudentToOgnp_ThrowUnavaliavbleOgnpException()
    {
        var schedule = new Schedule();
        var isuService = new IsuExtraService();
        var classService = new ClassService();

        Faculty faculty = isuService.AddFaculty("C", "Computer science");

        Group group = isuService.AddGroup("C32121");
        Student student = isuService.AddStudent(group, "Mariya Izerakova");
        Tutor tutor = isuService.AddTutor("Mariya Izerakova");

        Discipline firstDiscipline = isuService.AddDiscipline("Physics");
        Discipline secondDiscipline = isuService.AddDiscipline("Math");

        Ognp ognp = isuService.AddOgnp(faculty, firstDiscipline, secondDiscipline);

        Assert.Throws<InvalidOgnpException>(() => isuService.AddStudentToOgnp(schedule, student, ognp));
    }

    [Fact]
    public void AddStudentToOgnp_ThrowScheduleIntersectionException()
    {
        var schedule = new Schedule();
        var isuService = new IsuExtraService();
        var classService = new ClassService();

        isuService.AddFaculty("C", "Computer science");
        Faculty secondFaculty = isuService.AddFaculty("M", "Math");

        Group group = isuService.AddGroup("C32121");
        Student student = isuService.AddStudent(group, "Mariya Izerakova");

        Tutor firstTutor = isuService.AddTutor("Mariya Izerakova");
        Tutor secondTutor = isuService.AddTutor("Elena Izerakova");

        Discipline firstDiscipline = isuService.AddDiscipline("Physics");
        Discipline secondDiscipline = isuService.AddDiscipline("Math");

        Ognp ognp = isuService.AddOgnp(secondFaculty, firstDiscipline, secondDiscipline);

        var studyStream = new StudyStream();

        classService.AssignClass(schedule, firstDiscipline, studyStream, "O", "Monday", "08:20", firstTutor, 1);
        classService.AssignClass(schedule, secondDiscipline, group, "O", "Monday", "08:20", secondTutor, 2);

        isuService.AddStudyStream(ognp, studyStream);

        Assert.Throws<ScheduleIntersectionException>(() => isuService.AddStudentToOgnp(schedule, student, ognp));
    }

    [Fact]
    public void RemoveStudentFromOgnp_StudentRemoved()
    {
        var schedule = new Schedule();
        var isuService = new IsuExtraService();
        var classService = new ClassService();

        isuService.AddFaculty("C", "Computer science");
        Faculty secondFaculty = isuService.AddFaculty("M", "Math");

        Group group = isuService.AddGroup("C32121");
        Student student = isuService.AddStudent(group, "Mariya Izerakova");

        Tutor tutor = isuService.AddTutor("Mariya Izerakova");

        Discipline firstDiscipline = isuService.AddDiscipline("Physics");
        Discipline secondDiscipline = isuService.AddDiscipline("Math");

        Ognp ognp = isuService.AddOgnp(secondFaculty, firstDiscipline, secondDiscipline);

        var studyStream = new StudyStream();

        classService.AssignClass(schedule, firstDiscipline, studyStream, "O", "Monday", "08:20", tutor, 1);
        isuService.AddStudyStream(ognp, studyStream);

        isuService.AddStudentToOgnp(schedule, student, ognp);
        isuService.RemoveStudentFromOgnp(student, ognp);

        var studentExtra = student as StudentExtra;

        Assert.Empty(studentExtra?.Ognps);
        Assert.DoesNotContain(student, ognp.StudyStreams[0].Students);
    }

    [Fact]
    public void GetStudentsNotEnrolledInOgnp_StudentsHaveBeenRetrieved()
    {
        var schedule = new Schedule();
        var isuService = new IsuExtraService();
        var classService = new ClassService();

        isuService.AddFaculty("C", "Computer science");
        Faculty secondFaculty = isuService.AddFaculty("M", "Math");

        Group group = isuService.AddGroup("C32121");
        Student firstStudent = isuService.AddStudent(group, "Mariya Izerakova");
        Student secondStudent = isuService.AddStudent(group, "Mariya Izerakova");
        Student thirdStudent = isuService.AddStudent(group, "Mariya Izerakova");

        Tutor tutor = isuService.AddTutor("Mariya Izerakova");

        Discipline firstDiscipline = isuService.AddDiscipline("Physics");
        Discipline secondDiscipline = isuService.AddDiscipline("Math");

        Ognp ognp = isuService.AddOgnp(secondFaculty, firstDiscipline, secondDiscipline);

        var studyStream = new StudyStream();

        classService.AssignClass(schedule, firstDiscipline, studyStream, "O", "Monday", "08:20", tutor, 1);
        isuService.AddStudyStream(ognp, studyStream);

        isuService.AddStudentToOgnp(schedule, firstStudent, ognp);

        ReadOnlyCollection<Student> foundStudent = isuService.GetStudentsNotEnrolledInOgnp(group);

        Assert.NotEmpty(foundStudent);
        Assert.Equal(secondStudent, foundStudent[0]);
        Assert.Equal(thirdStudent, foundStudent[1]);
    }
}
