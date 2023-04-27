using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;

using Backups.Entities.DataObjects.Repositories;
using Backups.Entities.DataObjects.Storages;
using Backups.Entities.StorageAlgorithms;
using Backups.Exceptions;

namespace Backups.Entities;

public class BackupTask
{
    private static readonly string BackupTaskNamePrefix = "BackupTask";
    private static readonly string NameDataSeparator = "-";

    private static readonly string InvalidNameCharacters = "\\/:*?<>|";
    private static readonly string RegexInvalidBackupTaskName = string.Format("[{0}]", InvalidNameCharacters);

    private static readonly string CurrentDirectory = string.Format(".{0}", Path.DirectorySeparatorChar);

    private static readonly int MinBackupTaskNameLength = 1;
    private static readonly int MaxBackupTaskNameLength = 32;

    private int id = 0;
    private int restorePointId = 0;

    private List<BackupObject> backupObjects;
    private List<RestorePoint> restorePoints;

    public BackupTask(string name, Repository repository, StorageAlgorithm storageAlgorithm)
    {
        if (string.IsNullOrEmpty(name) || Regex.IsMatch(name, RegexInvalidBackupTaskName))
        {
            throw new ArgumentException(
                string.Format("Backup task name must be non-empty string and must not contain the following characters: {0}", InvalidNameCharacters),
                nameof(name));
        }

        if (name.Length < MinBackupTaskNameLength || name.Length > MaxBackupTaskNameLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(name),
                string.Format("Backup task name length must be from {0} to {1}", MinBackupTaskNameLength, MaxBackupTaskNameLength));
        }

        if (repository is null)
            throw new ArgumentNullException(nameof(repository), "Invalid repository");

        if (storageAlgorithm is null)
            throw new ArgumentNullException(nameof(storageAlgorithm), "Invalid storage algorithm");

        backupObjects = new List<BackupObject>();
        restorePoints = new List<RestorePoint>();

        id = GetNextBackupTaskId();

        CreationTime = DateTime.Now;

        Name = GenerateBackupTaskName(name);

        Repository = repository;
        StorageAlgorithm = storageAlgorithm;

        repository.AddDataObject(new DataObjects.Directory(Name), CurrentDirectory);
    }

    public int Id { get => id; }

    public string Name { get; }

    public DateTime CreationTime { get; }

    public Repository Repository { get; }

    public StorageAlgorithm StorageAlgorithm { get; }

    public ReadOnlyCollection<BackupObject> BackupObjects => backupObjects.AsReadOnly();

    public ReadOnlyCollection<RestorePoint> RestorePoints => restorePoints.AsReadOnly();

    public RestorePoint Execute()
    {
        var restorePoint = new RestorePoint(Name, GetNextRestorePointId());

        foreach (BackupObject backupObject in backupObjects)
        {
            backupObject.UpgradeBackupVersion();
            restorePoint.AddBackupObject(backupObject);
        }

        AddRestorePoint(restorePoint);

        List<Storage> storages = StorageAlgorithm.CreateStorage(backupObjects);

        foreach (Storage storage in storages)
            Repository.AddDataObject(storage, Path.Combine(".", restorePoint.Path));

        return restorePoint;
    }

    public BackupObject AddBackupObject(BackupObject newBackupObject)
    {
        if (newBackupObject is null)
            throw new ArgumentNullException(nameof(newBackupObject), "Invalid backup object");

        if (backupObjects.Contains(newBackupObject))
            throw new DuplicateBackupObjectException();

        backupObjects.Add(newBackupObject);

        return newBackupObject;
    }

    public void RemoveBackupObject(BackupObject backupObjectToRemove)
    {
        if (backupObjectToRemove is null)
            throw new ArgumentNullException(nameof(backupObjectToRemove), "Invalid backup object");

        if (!backupObjects.Contains(backupObjectToRemove))
            throw new NotExistingBackupObjectException();

        backupObjects.Remove(backupObjectToRemove);
    }

    public RestorePoint AddRestorePoint(RestorePoint newRestorePoint)
    {
        if (newRestorePoint is null)
            throw new ArgumentNullException(nameof(newRestorePoint), "Invalid restore point");

        if (restorePoints.Contains(newRestorePoint))
            throw new DuplicateRestorePointException();

        foreach (RestorePoint restorePoint in restorePoints)
        {
            if (restorePoint.Equals(newRestorePoint))
                throw new DuplicateRestorePointException();
        }

        restorePoints.Add(newRestorePoint);

        Repository.AddDataObject(new DataObjects.Directory(newRestorePoint.Name), Path.Combine(".", Name));

        return newRestorePoint;
    }

    private string GenerateBackupTaskName(string name)
    {
        return new StringBuilder(BackupTaskNamePrefix)
            .Append(NameDataSeparator)
            .Append(name)
            .Append(NameDataSeparator)
            .Append(Id)
            .ToString();
    }

    private int GetNextBackupTaskId()
    {
        return id++;
    }

    private int GetNextRestorePointId()
    {
        return restorePointId++;
    }
}
