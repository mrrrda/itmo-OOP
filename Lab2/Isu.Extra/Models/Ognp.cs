using System.Collections.ObjectModel;

using Isu.Exceptions;

namespace Isu.Extra.Models;

public class Ognp
{
    private readonly List<StudyStream> studyStreams;
    public Ognp(AcademicModule module)
    {
        if (module is null)
            throw new ArgumentNullException(nameof(module), "Invalid module");

        studyStreams = new List<StudyStream>();

        Module = module;
    }

    public AcademicModule Module { get; }

    public ReadOnlyCollection<StudyStream> StudyStreams { get => studyStreams.AsReadOnly(); }

    public void AddStudyStream(StudyStream studyStream)
    {
        if (studyStream is null)
            throw new ArgumentNullException(nameof(studyStream), "Invalid study stream");

        studyStreams.Add(studyStream);
    }

    public void RemoveStudyStream(StudyStream studyStream)
    {
        if (studyStream is null)
            throw new ArgumentNullException(nameof(studyStream), "Invalid study stream");

        if (!studyStreams.Contains(studyStream))
            throw new NotExistingStudyUnitException();

        studyStreams.Remove(studyStream);
    }

    public override bool Equals(object? obj)
    {
        return obj is Ognp other
            && Module.Equals(other.Module);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this);
    }
}
