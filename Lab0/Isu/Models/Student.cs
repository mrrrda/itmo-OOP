namespace Isu.Models;

public class Student : UniversityActor
{
    public Student(int id, string fullName)
        : base(id, fullName)
    { }

    public Group? Group { get; set; }
}
