using System.Collections.ObjectModel;
using System.Text;

using Backups.Exceptions;

namespace Backups.Entities;

public class RestorePoint
{
    private static readonly string RestorePointPrefix = "RestorePoint";
    private static readonly string NamePartsSeparator = "-";

    private List<BackupObject> backupObjects;

    public RestorePoint(string backupTaskName, int id)
    {
        if (string.IsNullOrEmpty(backupTaskName))
            throw new ArgumentException("Invalid name", nameof(backupTaskName));

        backupObjects = new List<BackupObject>();

        Id = id;

        CreationTime = DateTime.Now;

        Name = GenerateRestorePointName();
        Path = new StringBuilder(backupTaskName)
            .Append(System.IO.Path.DirectorySeparatorChar)
            .Append(Name)
            .ToString();
    }

    public int Id { get; }

    public string Name { get; }

    public string Path { get; }

    public DateTime CreationTime { get; }

    public ReadOnlyCollection<BackupObject> BackupObjects => backupObjects.AsReadOnly();

    public void AddBackupObject(BackupObject newBackupObject)
    {
        if (newBackupObject is null)
            throw new ArgumentNullException(nameof(newBackupObject), "Invalid backup object");

        if (backupObjects.Contains(newBackupObject))
            throw new DuplicateBackupObjectException();

        backupObjects.Add(newBackupObject);
    }

    public override bool Equals(object? obj)
    {
        return obj is RestorePoint other
            && Path.Equals(other.Path);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this);
    }

    private string GenerateRestorePointName()
    {
        return new StringBuilder()
            .Append(RestorePointPrefix)
            .Append(NamePartsSeparator)
            .Append(Id)
            .ToString();
    }
}
