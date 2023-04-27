using System.Collections.ObjectModel;
using System.Net.WebSockets;
using System.Text;

using Isu.Exceptions;
using Isu.Extra.Entities;
using Isu.Extra.Exceptions;
using Isu.Extra.Models;
using Isu.Models;
using Isu.Services;

namespace Isu.Extra.Services;

public class IsuExtraService : IsuService
{
    public static readonly int MaxTutorsNumber = 256;

    public static readonly int MaxDisciplinesNumber = 128;
    public static readonly int MaxFacultyOgnpsNumber = 2;

    public static readonly int MaxStudentOgnpsNumber = 2;

    public static readonly int MaxStudyStreamsNumber = 3;
    public static readonly int MaxStudyStreamCapacity = 32;

    private readonly Dictionary<string, Discipline> disciplines;
    private readonly Dictionary<string, Ognp> ognps;
    private readonly Dictionary<int, Tutor> tutors;

    private int nextTutorId = 0;

    public IsuExtraService()
    {
        disciplines = new Dictionary<string, Discipline>();
        ognps = new Dictionary<string, Ognp>();
        tutors = new Dictionary<int, Tutor>();
    }

    public override Faculty AddFaculty(string facultyCode, string facultyName)
    {
        if (string.IsNullOrEmpty(facultyCode))
            throw new ArgumentException("Invalid faculty code", nameof(facultyCode));

        if (Faculties.ContainsKey(facultyCode))
            throw new DuplicateFacultyException(facultyCode);

        if (Faculties.Count >= MaxFacultiesNumber)
            throw new FacultiesNumberOverflowException(MaxFacultiesNumber.ToString());

        var newFaculty = new FacultyExtra(facultyCode, facultyName);
        Faculties.Add(newFaculty.Code, newFaculty);

        return newFaculty;
    }

    public override Student AddStudent(Group group, string fullName)
    {
        if (group is null)
            throw new ArgumentException("Invalid group", nameof(group));

        if (!Groups.ContainsKey(group.GroupName.Name))
            throw new NotExistingStudyUnitException();

        if (group.StudentsCount >= MaxGroupCapacity)
            throw new GroupCapacityOverflowException(MaxGroupCapacity.ToString());

        if (Students.Count >= MaxStudentsNumber)
            throw new StudentsNumberOverflowException(MaxStudentsNumber.ToString());

        int id = GetNextStudentId();

        var newStudent = new StudentExtra(id, fullName);
        newStudent.Group = group;

        Students.Add(newStudent.Id, newStudent);
        group.AddStudent(newStudent);

        return newStudent;
    }

    public Discipline AddDiscipline(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Invalid discipline name", nameof(name));

        if (disciplines.ContainsKey(name))
            throw new DuplicateDisciplineException(name);

        if (disciplines.Count >= MaxDisciplinesNumber)
            throw new DisciplinesNumberOverflowException(MaxDisciplinesNumber.ToString());

        var discipline = new Discipline(name);

        disciplines.Add(discipline.Name, discipline);

        return discipline;
    }

    public Ognp AddOgnp(Faculty faculty, Discipline firstDiscipline, Discipline secondDiscipline)
    {
        if (faculty is null)
            throw new ArgumentNullException(nameof(faculty), "Invalid faculty");

        if (faculty is not FacultyExtra facultyExtra)
            throw new ArgumentNullException(nameof(facultyExtra), "Invalid faculty");

        if (firstDiscipline is null)
            throw new ArgumentNullException(nameof(firstDiscipline), "Invalid discipline");

        if (secondDiscipline is null)
            throw new ArgumentNullException(nameof(secondDiscipline), "Invalid discipline");

        if (facultyExtra.Ognps.Count >= MaxFacultyOgnpsNumber)
            throw new OgnpsNumberOverflowException(MaxFacultyOgnpsNumber.ToString());

        string ognpKey = new StringBuilder()
            .Append(firstDiscipline.Name.ToLower())
            .Append(' ')
            .Append(secondDiscipline.Name.ToLower())
            .ToString();

        if (ognps.ContainsKey(ognpKey))
            throw new DuplicateOgnpException();

        var ognp = new Ognp(new AcademicModule(firstDiscipline, secondDiscipline));

        facultyExtra.AddOgnp(ognp);
        ognps.Add(ognpKey, ognp);

        return ognp;
    }

    public void AddStudyStream(Ognp ognp, StudyStream studyStream)
    {
        if (ognp is null)
            throw new ArgumentNullException(nameof(ognp), "Invalid ognp");

        if (!ognps.ContainsValue(ognp))
            throw new InvalidOgnpException("Ognps does not exist");

        if (studyStream is null)
            throw new ArgumentNullException(nameof(studyStream), "Invalid study stream");

        if (ognp.StudyStreams.Contains(studyStream))
            throw new DuplicateStudyUnitException();

        if (ognp.StudyStreams.Count >= MaxStudyStreamsNumber)
            throw new StudyUnitsNumberOverflowException(MaxStudyStreamsNumber.ToString());

        ognp.AddStudyStream(studyStream);
    }

    public Tutor AddTutor(string fullName)
    {
        if (tutors.Count >= MaxTutorsNumber)
            throw new TutorsNumberOverflowException(MaxTutorsNumber.ToString());

        int id = GetNextTutorId();

        var newTutor = new Tutor(id, fullName);
        tutors.Add(newTutor.Id, newTutor);

        return newTutor;
    }

    public Ognp? FindOgnp(Discipline firstDiscipline, Discipline secondDiscipline)
    {
        if (firstDiscipline is null)
            throw new ArgumentNullException(nameof(firstDiscipline), "Invalid discipline");

        if (secondDiscipline is null)
            throw new ArgumentNullException(nameof(secondDiscipline), "Invalid discipline");

        string ognpKey = new StringBuilder()
            .Append(firstDiscipline.Name.ToLower())
            .Append(' ')
            .Append(secondDiscipline.Name.ToLower())
            .ToString();

        ognps.TryGetValue(ognpKey, out Ognp? ognp);

        return ognp;
    }

    public void AddStudentToOgnp(Schedule schedule, Student student, Ognp ognp)
    {
        if (student is null)
            throw new ArgumentNullException(nameof(student), "Invalid student");

        if (student is not StudentExtra studentExtra)
            throw new ArgumentException("Invalid student", nameof(student));

        if (studentExtra.Ognps.Count >= MaxStudentOgnpsNumber)
            throw new OgnpsNumberOverflowException(MaxStudentOgnpsNumber.ToString());

        HandleStudentOgnpChange(schedule, studentExtra, ognp);
    }

    public void ChangeStudentOgnp(Schedule schedule, Student student, Ognp newOgnp)
    {
        if (student is null)
            throw new ArgumentNullException(nameof(student), "Invalid student");

        if (student is not StudentExtra studentExtra)
            throw new ArgumentException("Invalid student", nameof(student));

        if (studentExtra.Ognps.Count == 0)
            throw new InvalidOgnpException(string.Format("Student {0} is not enrolled in ognp", studentExtra.Id.ToString()));

        if (newOgnp is null)
            throw new ArgumentNullException(nameof(newOgnp), "Invalid new ognp");

        if (studentExtra.Ognps.Contains(newOgnp))
            throw new InvalidOgnpException("Student is already enrolled in requested ognp");

        HandleStudentOgnpChange(schedule, studentExtra, newOgnp);
    }

    public void RemoveStudentFromOgnp(Student student, Ognp ognp)
    {
        if (student is null)
            throw new ArgumentNullException(nameof(student), "Invalid student");

        if (ognp is null)
            throw new ArgumentNullException(nameof(ognp), "Invalid ognp");

        if (student is not StudentExtra studentExtra)
            throw new ArgumentException("Invalid student", nameof(student));

        if (studentExtra.Group is null)
            throw new NonActiveStudentException(studentExtra.Id.ToString(StudentIdFormat));

        if (!studentExtra.Ognps.Contains(ognp))
            throw new InvalidOgnpException(string.Format("Student {0} is not enrolled in requested ognp", studentExtra.Id.ToString()));

        var foundStudyStreams = new List<StudyStream>();

        foreach (Ognp studentOgnp in studentExtra.Ognps)
        {
            foreach (StudyStream studyStream in studentOgnp.StudyStreams)
                foundStudyStreams.Add(studyStream);
        }

        foundStudyStreams.ForEach(foundStudyStream => foundStudyStream.RemoveStudent(studentExtra));

        studentExtra.RemoveOgnp(ognp);
    }

    public ReadOnlyCollection<Student> GetStudentsNotEnrolledInOgnp(Group group)
    {
        if (group is null)
            throw new ArgumentNullException(nameof(group), "Invalid group");

        IEnumerable<Student> studentQuery = group.Students
            .Where(student => student is StudentExtra studentExtra && studentExtra.Ognps.Count == 0);

        return studentQuery.ToList<Student>().AsReadOnly();
    }

    private void HandleStudentOgnpChange(Schedule schedule, StudentExtra student, Ognp ognp)
    {
        if (schedule is null)
            throw new ArgumentNullException(nameof(schedule), "Invalid schedule");

        if (student is null)
            throw new ArgumentNullException(nameof(student), "Invalid student");

        if (ognp is null)
            throw new ArgumentNullException(nameof(ognp), "Invalid ognp");

        if (student.Group is null)
            throw new NonActiveStudentException(student.Id.ToString(StudentIdFormat));

        string facultyCode = student.Group.GroupName.FacultyCode;

        if (GetFaculty(facultyCode) is not FacultyExtra faculty)
            throw new ArgumentNullException(nameof(faculty), "Invalid faculty");

        if (faculty.Ognps.Contains(ognp))
            throw new InvalidOgnpException("Students cannot enroll in ognp of their own faculties");

        List<AcademicClass> groupSchedule = schedule.GetClassesListByStudyUnit(student.Group);

        var ognpSchedules = new List<AcademicClass>[ognp.StudyStreams.Count];

        for (int i = 0; i < ognp.StudyStreams.Count; i++)
            ognpSchedules[i] = schedule.GetClassesListByStudyUnit(ognp.StudyStreams[i]);

        bool[] hasClassesIntersection = new bool[ognpSchedules.Length];

        for (int i = 0; i < ognpSchedules.Length; i++)
            hasClassesIntersection[i] = schedule.CheckClassesIntersection(groupSchedule, ognpSchedules[i]);

        if (hasClassesIntersection.Length > 0 && Array.TrueForAll(hasClassesIntersection, flag => flag))
            throw new ScheduleIntersectionException();

        for (int i = 0; i < ognp.StudyStreams.Count; i++)
        {
            if (!hasClassesIntersection[i] && ognp.StudyStreams[i].StudentsCount < MaxStudyStreamCapacity)
            {
                ognp.StudyStreams[i].AddStudent(student);
                student.AddOgnp(ognp);
                break;
            }

            if (i == ognp.StudyStreams.Count - 1)
                throw new InvalidOgnpException("All ognp study streams are full");
        }
    }

    private int GetNextTutorId()
    {
        return nextTutorId++;
    }
}
