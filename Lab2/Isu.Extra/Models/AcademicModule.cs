namespace Isu.Extra.Models;

public class AcademicModule
{
    public AcademicModule(Discipline firstDiscipline, Discipline secondDiscipline)
    {
        if (firstDiscipline is null)
            throw new ArgumentNullException(nameof(firstDiscipline), "Invalid discipline");

        if (secondDiscipline is null)
            throw new ArgumentNullException(nameof(secondDiscipline), "Invalid discipline");

        if (firstDiscipline.Equals(secondDiscipline))
            throw new ArgumentException("Only distinct disciplines can be added to the Module");

        FirstDiscipline = firstDiscipline;
        SecondDiscipline = secondDiscipline;
    }

    public Discipline FirstDiscipline { get; }

    public Discipline SecondDiscipline { get; }

    public override bool Equals(object? obj)
    {
        return obj is AcademicModule other
            && FirstDiscipline.Equals(other.FirstDiscipline)
            && SecondDiscipline.Equals(other.SecondDiscipline);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this);
    }
}
