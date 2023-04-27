using Isu.Exceptions;
using Isu.Models;
using Isu.Services;

using Xunit;

namespace Isu.Test;

public class IsuServiceTest
{
    [Theory]
    [InlineData("C", "Compiter Science1")]
    [InlineData("1", "Compiter Science")]
    [InlineData(" ", "Compiter Science")]
    [InlineData("", "Compiter Science")]
    [InlineData("?", "Compiter Science")]
    [InlineData("C", " ")]
    [InlineData("C", "")]
    [InlineData("C", "1")]
    [InlineData("C", "?")]
    public void AddFacultyWithInvalidName_ThrowException(string facultyCode, string facultyName)
    {
        var isuService = new IsuService();

        Assert.Throws<ArgumentException>(() => isuService.AddFaculty(facultyCode, facultyName));
    }

    [Fact]
    public void AddDuplicateFaculty_ThrowExceptions()
    {
        var isuService = new IsuService();

        isuService.AddFaculty("C", "Computer Science");

        Assert.Throws<DuplicateFacultyException>(() => isuService.AddFaculty("C", "Physics"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("M")]
    [InlineData("M3")]
    [InlineData("M52121")]
    [InlineData("M35121")]
    [InlineData("?32121")]
    [InlineData("232121")]
    [InlineData("m32121")]
    [InlineData("M43121")]
    [InlineData("M32331")]
    [InlineData("M32126")]
    public void CreateGroupWithInvalidName_ThrowException(string groupName)
    {
        Assert.Throws<ArgumentException>(() => new Group(groupName));
    }

    [Theory]
    [InlineData("M32121", "M", 3, 2, 12, "1", "")]
    [InlineData("M3201", "M", 3, 2, 1, "", "")]
    [InlineData("M3201c", "M", 3, 2, 1, "", "c")]
    [InlineData("M32071c", "M", 3, 2, 7, "1", "c")]
    public void AddGroupToIsu_GroupInfoIsFilledCorrectly(
        string groupName,
        string expectedFacultyCode,
        int expectedDegree,
        int expectedCourse,
        int expectedGroupNumber,
        string expectedSpecializaton,
        string expectedNetworkingSpecifier)
    {
        var group = new Group(groupName);

        Assert.Equal(group.GroupName.FacultyCode, expectedFacultyCode);
        Assert.Equal<int>(group.GroupName.Course.Degree, expectedDegree);
        Assert.Equal<int>(group.GroupName.Course.Number, expectedCourse);
        Assert.Equal<int>(group.GroupName.GroupNumber, expectedGroupNumber);
        Assert.Equal(group.GroupName.Specialization, expectedSpecializaton);
        Assert.Equal(group.GroupName.Networking, expectedNetworkingSpecifier);
    }

    [Fact]
    public void AddGroup_ThrowNotExistingFacultyException()
    {
        var isuService = new IsuService();
        isuService.AddFaculty("C", "Computer Science");

        Assert.Throws<NotExistingFacultyException>(() => isuService.AddGroup("M32121"));
    }

    [Fact]
    public void AddDuplicateGroup_ThrowExceptions()
    {
        var isuService = new IsuService();

        isuService.AddFaculty("C", "Computer Science");
        isuService.AddGroup("C32121");

        Assert.Throws<DuplicateStudyUnitException>(() => isuService.AddGroup("C32121"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("1")]
    [InlineData("M3")]
    [InlineData("?")]
    [InlineData("Mariya 5")]
    [InlineData("Mariya Izerakova?")]
    public void CreateStudentWithInvalidName_ThrowException(string fullName)
    {
        Assert.Throws<ArgumentException>(() => new Student(0, fullName));
    }

    [Fact]
    public void AddStudent_ThrowNotExistingGroupException()
    {
        var isuService = new IsuService();

        isuService.AddFaculty("C", "Computer Science");
        isuService.AddGroup("C32121");

        Assert.Throws<NotExistingStudyUnitException>(() => isuService.AddStudent(new Group("M32121"), "Mariya Izerakova"));
    }

    [Fact]
    public void GetStudentByInvalidId_ThrowException()
    {
        var isuService = new IsuService();

        Assert.Throws<ArgumentOutOfRangeException>(() => isuService.GetStudent(-1));
    }

    [Fact]
    public void GetStudent_ThrowStudentNotFoundException()
    {
        var isuService = new IsuService();

        isuService.AddFaculty("C", "Computer Science");
        Group group = isuService.AddGroup("C32121");
        Student student = isuService.AddStudent(group, "Mariya Izerakova");

        Assert.Throws<StudentNotFoundException>(() => isuService.GetStudent(1));
        Assert.Throws<StudentNotFoundException>(() => isuService.GetStudent(student.Id + 10));
    }

    [Fact]
    public void AddStudentToGroup_StudentHasGroupAndGroupContainsStudent()
    {
        var isuService = new IsuService();

        isuService.AddFaculty("C", "Computer Science");
        Group group = isuService.AddGroup("C32121");
        Student student = isuService.AddStudent(group, "Mariya Izerakova");

        Assert.True(group.Students.Count == 1);
        Assert.Contains(student, group.Students);

        Assert.NotNull(student.Group);
        Assert.True(student.Group?.Equals(group));

        Assert.NotEmpty(isuService.FindStudents(group.GroupName.Name));
    }

    [Fact]
    public void ReachMaxFacultyPerUniversity_ThrowException()
    {
        var isuService = new IsuService();

        char facultyCode = 'A';

        isuService.AddFaculty(facultyCode.ToString(), "Computer Science");

        for (int i = 0; i < IsuService.MaxFacultiesNumber - 1; i++)
        {
            facultyCode = (char)(facultyCode + 1);
            isuService.AddFaculty(facultyCode.ToString(), "Computer Science");
        }

        facultyCode = (char)(facultyCode + 1);
        Assert.Throws<FacultiesNumberOverflowException>(() => isuService.AddFaculty(facultyCode.ToString(), "Computer Science"));
    }

    [Fact]
    public void ReachMaxStudentPerGroup_ThrowException()
    {
        var isuService = new IsuService();

        isuService.AddFaculty("C", "Computer Science");
        Group group = isuService.AddGroup("C32121");

        for (int i = 0; i < IsuService.MaxGroupCapacity; i++)
            isuService.AddStudent(group, "Mariya Izerakova");

        Assert.Throws<GroupCapacityOverflowException>(() => isuService.AddStudent(group, "Mariya Izerakova"));
    }

    [Fact]
    public void TransferStudentToAnotherGroup_GroupChanged()
    {
        var isuService = new IsuService();

        isuService.AddFaculty("C", "Computer Science");

        Group groupFrom = isuService.AddGroup("C32121");
        Group groupTo = isuService.AddGroup("C32132");

        Student student = isuService.AddStudent(groupFrom, "Mariya Izerakova");

        Assert.NotNull(student.Group);
        Assert.NotEmpty(isuService.FindStudents(groupFrom.GroupName.Name));
        Assert.True(student.Group?.Equals(groupFrom));

        isuService.ChangeStudentGroup(student, groupTo);

        Assert.NotNull(student.Group);
        Assert.NotEmpty(isuService.FindStudents(groupTo.GroupName.Name));
        Assert.True(student.Group?.Equals(groupTo));
        Assert.Empty(isuService.FindStudents(groupFrom.GroupName.Name));
    }
}
