using Isu.Models;

namespace Isu.Services;

public interface IIsuService
{
    public Faculty AddFaculty(string facultyCode, string facultyName);
    public Group AddGroup(string groupName);

    public Student AddStudent(Group group, string fullName);

    public Student GetStudent(int id);

    public Student? FindStudent(int id);
    public IReadOnlyCollection<Student> FindStudents(string groupName);
    public IReadOnlyCollection<Student> FindStudents(int courseNumber);

    public Faculty? FindFaculty(string facultyCode);

    public Group? FindGroup(string groupName);
    public IReadOnlyCollection<Group> FindGroups(int courseNumber);

    public void ChangeStudentGroup(Student student, Group newGroup);
}
