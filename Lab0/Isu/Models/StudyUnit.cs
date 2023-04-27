using System.Collections.ObjectModel;

using Isu.Exceptions;

namespace Isu.Models;

public abstract class StudyUnit
{
    private readonly List<Student> students;

    public StudyUnit()
    {
        students = new List<Student>();
    }

    public ReadOnlyCollection<Student> Students { get => students.AsReadOnly(); }

    public int StudentsCount
    {
        get => Students.Count;
    }

    public void AddStudent(Student student)
    {
        if (student is null)
            throw new ArgumentNullException(nameof(student), "Invalid student");

        students.Add(student);
    }

    public void RemoveStudent(Student student)
    {
        if (student is null)
            throw new ArgumentNullException(nameof(student), "Invalid student");

        if (!students.Contains(student))
            throw new StudentNotFoundException(student.Id.ToString());

        students.Remove(student);
    }
}
