using Backups.Entities.DataObjects;
using Backups.Entities.DataObjects.Storages;

namespace Backups.Entities.StorageAlgorithms;

public abstract class StorageAlgorithm
{
    public abstract List<Storage> CreateStorage(List<BackupObject> backupObjects);

    protected Dictionary<BackupObject, string> GetBackupObjectsPaths(List<BackupObject> backupObjects)
    {
        if (backupObjects is null)
            throw new ArgumentNullException(nameof(backupObjects), "Invalid backup objects list");

        if (backupObjects.Count == 0)
            throw new ArgumentException("Empty backup objects list", nameof(backupObjects));

        var backupObjectsPaths = new Dictionary<BackupObject, string>();

        foreach (BackupObject backupObject in backupObjects)
        {
            string path = backupObject.DataObject.FullName;
            DataObject? parent = backupObject.DataObject.Parent;

            while (parent is not null && backupObjects.Where(currentBackupObject => currentBackupObject.DataObject.Equals(parent)).Any())
            {
                path = Path.Combine(parent.Name, path);
                parent = parent.Parent;
            }

            backupObjectsPaths.Add(backupObject, path);
        }

        return backupObjectsPaths;
    }
}
