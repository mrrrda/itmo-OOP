using Backups.Entities.DataObjects;

namespace Backups.Entities;

public class BackupObject
{
    private int _backupVersion;

    public BackupObject(DataObject dataObject)
    {
        if (dataObject is null)
            throw new ArgumentNullException(nameof(dataObject), "Invalid data object");

        DataObject = dataObject;
        _backupVersion = 0;
    }

    public DataObject DataObject { get; }

    internal int BackupVersion
    {
        get => _backupVersion;
    }

    internal void UpgradeBackupVersion()
    {
        _backupVersion++;
    }
}
