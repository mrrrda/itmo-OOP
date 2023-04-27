using Isu.Exceptions;
using Isu.Models;

namespace Isu.Services;

public class IsuService : IIsuService
{
    public static readonly int MaxFacultiesNumber = 25;
    public static readonly int MaxGroupsNumber = 256;
    public static readonly int MaxStudentsNumber = 10000;

    public static readonly int MaxGroupCapacity = 32;

    protected static readonly string StudentIdFormat = "000000";

    private int nextStudentId = 0;

    public IsuService()
    {
        Faculties = new Dictionary<string, Faculty>();
        Groups = new Dictionary<string, Group>();

        Students = new Dictionary<int, Student>();
    }

    protected Dictionary<string, Faculty> Faculties { get; }
    protected Dictionary<string, Group> Groups { get; }

    protected Dictionary<int, Student> Students { get; }

    public virtual Faculty AddFaculty(string facultyCode, string facultyName)
    {
        if (string.IsNullOrEmpty(facultyCode))
            throw new ArgumentException("Invalid faculty code", nameof(facultyCode));

        if (Faculties.ContainsKey(facultyCode))
            throw new DuplicateFacultyException(facultyCode);

        if (Faculties.Count >= MaxFacultiesNumber)
            throw new FacultiesNumberOverflowException(MaxFacultiesNumber.ToString());

        var newFaculty = new Faculty(facultyCode, facultyName);
        Faculties.Add(newFaculty.Code, newFaculty);

        return newFaculty;
    }

    public Group AddGroup(string groupName)
    {
        if (string.IsNullOrEmpty(groupName))
            throw new ArgumentException("Invalid group name", nameof(groupName));

        if (Groups.ContainsKey(groupName))
            throw new DuplicateStudyUnitException();

        if (Groups.Count >= MaxGroupsNumber)
            throw new StudyUnitsNumberOverflowException(MaxGroupsNumber.ToString());

        var newGroup = new Group(groupName);

        if (!Faculties.ContainsKey(newGroup.GroupName.FacultyCode))
            throw new NotExistingFacultyException(newGroup.GroupName.FacultyCode);

        Groups.Add(newGroup.GroupName.Name, newGroup);

        return newGroup;
    }

    public virtual Student AddStudent(Group group, string fullName)
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

        var newStudent = new Student(id, fullName);
        newStudent.Group = group;

        Students.Add(newStudent.Id, newStudent);
        group.AddStudent(newStudent);

        return newStudent;
    }

    public Student GetStudent(int id)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id), "Student id must be positive");

        try
        {
            return Students[id];
        }
        catch
        {
            throw new StudentNotFoundException(id.ToString(StudentIdFormat));
        }
    }

    public Faculty GetFaculty(string facultyCode)
    {
        if (string.IsNullOrEmpty(facultyCode))
            throw new ArgumentException("Invalid faculty code", nameof(facultyCode));

        try
        {
            return Faculties[facultyCode];
        }
        catch
        {
            throw new FacultyNotFoundException(facultyCode);
        }
    }

    public Student? FindStudent(int id)
    {
        Students.TryGetValue(id, out Student? student);

        return student;
    }

    public IReadOnlyCollection<Student> FindStudents(string groupName)
    {
        if (string.IsNullOrEmpty(groupName))
            throw new ArgumentException("Invalid group name", nameof(groupName));

        Groups.TryGetValue(groupName, out Group? group);

        return group?.Students ?? new List<Student>().AsReadOnly();
    }

    public IReadOnlyCollection<Student> FindStudents(int courseNumber)
    {
        if (courseNumber <= 0)
            throw new ArgumentOutOfRangeException(nameof(courseNumber), "Course number must be greater than 0");

        IEnumerable<Student> studentsQuery = Students.Values
            .Where(student => student.Group?.GroupName.Course.Number == courseNumber);

        return studentsQuery.ToList<Student>().AsReadOnly();
    }

    public Faculty? FindFaculty(string facultyCode)
    {
        if (string.IsNullOrEmpty(facultyCode))
            throw new ArgumentException("Invalid faculty code", nameof(facultyCode));

        Faculties.TryGetValue(facultyCode, out Faculty? faculty);

        return faculty;
    }

    public Group? FindGroup(string groupName)
    {
        if (string.IsNullOrEmpty(groupName))
            throw new ArgumentException("Invalid group name", nameof(groupName));

        Groups.TryGetValue(groupName, out Group? group);

        return group;
    }

    public IReadOnlyCollection<Group> FindGroups(int courseNumber)
    {
        if (courseNumber <= 0)
            throw new ArgumentOutOfRangeException(nameof(courseNumber), "Course number must be greater than 0");

        IEnumerable<Group> groupsQuery = Groups.Values
            .Where(group => group.GroupName.Course.Number == courseNumber);

        return groupsQuery.ToList<Group>().AsReadOnly();
    }

    public void ChangeStudentGroup(Student student, Group newGroup)
    {
        if (student is null)
            throw new ArgumentNullException(nameof(student), "Invalid student");

        if (newGroup is not null && newGroup.Students.Count >= MaxGroupCapacity)
            throw new GroupCapacityOverflowException(MaxGroupCapacity.ToString());

        if (student.Group is not null)
            student.Group.RemoveStudent(student);

        student.Group = newGroup;

        if (student.Group is not null)
            student.Group.AddStudent(student);
    }

    protected int GetNextStudentId()
    {
        return nextStudentId++;
    }
}
