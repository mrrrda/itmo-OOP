using System.Collections.ObjectModel;

using Isu.Exceptions;
using Isu.Models;

namespace Isu.Extra.Models;

public class Tutor : UniversityActor
{
    private List<StudyUnit> studyUnits;

    public Tutor(int id, string fullName)
        : base(id, fullName)
    {
        studyUnits = new List<StudyUnit>();
    }

    public ReadOnlyCollection<StudyUnit> StudyUnits { get => studyUnits.AsReadOnly(); }

    public void AddStudyUnit(StudyUnit studyUnit)
    {
        if (studyUnit is null)
            throw new ArgumentNullException(nameof(studyUnit), "Invalid study unit");

        studyUnits.Add(studyUnit);
    }

    public void RemoveStudyUnit(StudyUnit studyUnit)
    {
        if (studyUnit is null)
            throw new ArgumentNullException(nameof(studyUnit), "Invalid study unit");

        if (!studyUnits.Contains(studyUnit))
            throw new NotExistingStudyUnitException();

        studyUnits.Remove(studyUnit);
    }
}
