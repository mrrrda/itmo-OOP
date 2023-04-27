using System.Collections.ObjectModel;

using Backups.Exceptions;

namespace Backups.Entities;

public class Backup
{
    private List<RestorePoint> restorePoints;

    public Backup()
    {
        restorePoints = new List<RestorePoint>();
    }

    public ReadOnlyCollection<RestorePoint> RestorePoints => restorePoints.AsReadOnly();

    public void AddRestorePoint(RestorePoint newRestorePoint)
    {
        if (newRestorePoint is null)
            throw new ArgumentNullException(nameof(newRestorePoint), "Invalid restore point");

        if (restorePoints.Contains(newRestorePoint))
            throw new DuplicateRestorePointException();

        restorePoints.Add(newRestorePoint);
    }
}
