using Backups.Entities;
using Backups.Entities.DataObjects;
using Backups.Entities.DataObjects.Repositories;
using Backups.Entities.DataObjects.Storages;
using Backups.Entities.StorageAlgorithms;
using Backups.Exceptions;

using Xunit;

using Directory = Backups.Entities.DataObjects.Directory;
using File = Backups.Entities.DataObjects.File;

namespace Backups.Test;

public class BackupsTest
{
    [Fact]
    public void AddDataObjectToRepository_ThrowDuplicateDataObjectException()
    {
        var repository = new InMemoryRepository("Repository");

        var firstFile = new File("File1", "txt");
        var secondFile = new File("File2", "txt");

        var firstFolder = new Directory("Folder1");

        repository.AddDataObject(firstFile, ".");
        repository.AddDataObject(secondFile, ".");
        repository.AddDataObject(firstFolder, ".");

        Assert.Throws<DuplicateDataObjectException>(() => repository.AddDataObject(firstFile, "."));
        Assert.Throws<DuplicateDataObjectException>(() => repository.AddDataObject(firstFolder, "."));

        Assert.Throws<DuplicateDataObjectException>(() => repository.AddDataObject(new File("File1", "txt"), "."));
        Assert.Throws<DuplicateDataObjectException>(() => repository.AddDataObject(new Directory("Folder1"), "."));
    }

    [Fact]
    public void RemoveDataObjectFromRepository_ThrowNotExistingDataObjectException()
    {
        var repository = new InMemoryRepository("Repository");

        Assert.Throws<NotExistingDataObjectException>(() => repository.RemoveDataObject(new File("File", "txt"), "."));
        Assert.Throws<NotExistingDataObjectException>(() => repository.RemoveDataObject(new Directory("Folder"), "."));

        var file = new File("File", "txt");
        repository.AddDataObject(file, ".");

        var folder = new Directory("Folder");
        repository.AddDataObject(folder, ".");

        Assert.Throws<NotExistingDataObjectException>(() => repository.RemoveDataObject(new File("File", "txt"), "."));
        Assert.Throws<NotExistingDataObjectException>(() => repository.RemoveDataObject(new Directory("Folder"), "."));
    }

    [Fact]
    public void AddFilesToRepository_RepositoryStructureIsCorrect()
    {
        var repository = new InMemoryRepository("Repository");

        var firstFile = new File("File1", "txt");
        var secondFile = new File("File2", "txt");
        var thirdFile = new File("File3", "txt");
        var fourthFile = new File("File4", "txt");
        var fifthFile = new File("File5", "txt");

        var firstFolder = new Directory("Folder1");
        var secondFolder = new Directory("Folder2");
        var thirdFolder = new Directory("Folder3");

        repository.AddDataObject(firstFile, ".");
        repository.AddDataObject(secondFile, ".");
        repository.AddDataObject(firstFolder, ".");

        repository.AddDataObject(thirdFile, Path.Combine(".", firstFolder.Name));

        repository.AddDataObject(secondFolder, Path.Combine(".", firstFolder.Name));
        firstFolder.AddDataObject(thirdFolder, ".");

        repository.AddDataObject(fourthFile, Path.Combine(".", firstFolder.Name, secondFolder.Name));
        secondFolder.AddDataObject(fifthFile, ".");

        Assert.Equal(3, repository.DataObjects.Count);
        Assert.Equal(firstFile, repository.DataObjects[0]);
        Assert.Equal(secondFile, repository.DataObjects[1]);
        Assert.Equal(firstFolder, repository.DataObjects[2]);

        Assert.Equal(3, firstFolder.DataObjects.Count);
        Assert.Equal(thirdFile, firstFolder.DataObjects[0]);
        Assert.Equal(secondFolder, firstFolder.DataObjects[1]);
        Assert.Equal(thirdFolder, firstFolder.DataObjects[2]);

        Assert.Equal(2, secondFolder.DataObjects.Count);
        Assert.Equal(fourthFile, secondFolder.DataObjects[0]);
        Assert.Equal(fifthFile, secondFolder.DataObjects[1]);
    }

    [Fact]
    public void RunBackupTaskWithSingleStorageAlgorithm_SuccessfulExecution()
    {
        var repository = new InMemoryRepository("Repository");
        var backupTask = new BackupTask("BackupTask", repository, new SingleStorageAlgorithm());

        Assert.Single(repository.DataObjects);
        Assert.Equal(backupTask.Name, repository.DataObjects[0].FullName);

        repository.AddDataObject(new File("File1", "txt"), ".");
        repository.AddDataObject(new File("File2", "txt"), ".");
        repository.AddDataObject(new Directory("Folder1"), ".");
        repository.AddDataObject(new File("File3", "txt"), Path.Combine(".", "Folder1"));

        Assert.Equal(4, repository.DataObjects.Count);

        var createdFolder = repository.DataObjects[3] as Directory;
        Assert.Single(createdFolder?.DataObjects);

        backupTask.AddBackupObject(new BackupObject(repository.DataObjects[1]));
        backupTask.AddBackupObject(new BackupObject(repository.DataObjects[3]));
        Assert.Equal(2, backupTask.BackupObjects.Count);

        RestorePoint firstRestorePoint = backupTask.Execute();

        backupTask.AddBackupObject(new BackupObject(repository.DataObjects[2]));
        Assert.Equal(3, backupTask.BackupObjects.Count);

        RestorePoint secondRestorePoint = backupTask.Execute();

        Assert.Equal(2, backupTask.RestorePoints.Count);
        Assert.Equal(2, firstRestorePoint.BackupObjects.Count);
        Assert.Equal(3, secondRestorePoint.BackupObjects.Count);

        DataObject? firstRestorePointRoot = repository.FindDataObject(firstRestorePoint.Path);
        DataObject? secondRestorePointRoot = repository.FindDataObject(secondRestorePoint.Path);

        Assert.NotNull(firstRestorePointRoot);
        Assert.NotNull(secondRestorePointRoot);

        Assert.True(firstRestorePointRoot is Directory);
        Assert.True(secondRestorePointRoot is Directory);

        var firstRestorePointDirectory = firstRestorePointRoot as Directory;
        var secondRestorePointDirectory = secondRestorePointRoot as Directory;

        Assert.Equal(1, firstRestorePointDirectory?.DataObjects.Count);
        Assert.True(firstRestorePointDirectory?.DataObjects[0] is ZipArchiveStorage);

        Assert.Equal(1, secondRestorePointDirectory?.DataObjects.Count);
        Assert.True(firstRestorePointDirectory?.DataObjects[0] is ZipArchiveStorage);
    }

    [Fact]
    public void RunBackupTaskWithSplitStorageAlgorithm_SuccessfulExecution()
    {
        var repository = new InMemoryRepository("Repository");
        var backupTask = new BackupTask("BackupTask", repository, new SplitStorageAlgorithm());

        Assert.Single(repository.DataObjects);
        Assert.Equal(backupTask.Name, repository.DataObjects[0].FullName);

        repository.AddDataObject(new Entities.DataObjects.File("File1", "txt"), ".");
        repository.AddDataObject(new Entities.DataObjects.File("File2", "txt"), ".");

        Assert.Equal(3, repository.DataObjects.Count);

        backupTask.AddBackupObject(new BackupObject(repository.DataObjects[1]));
        BackupObject backupObject = backupTask.AddBackupObject(new BackupObject(repository.DataObjects[2]));

        Assert.Equal(2, backupTask.BackupObjects.Count);

        RestorePoint firstRestorePoint = backupTask.Execute();

        backupTask.RemoveBackupObject(backupObject);
        Assert.Single(backupTask.BackupObjects);

        RestorePoint secondRestorePoint = backupTask.Execute();

        Assert.Equal(2, backupTask.RestorePoints.Count);
        Assert.Equal(2, firstRestorePoint.BackupObjects.Count);
        Assert.Single(secondRestorePoint.BackupObjects);

        DataObject? firstRestorePointRoot = repository.FindDataObject(firstRestorePoint.Path);
        DataObject? secondRestorePointRoot = repository.FindDataObject(secondRestorePoint.Path);

        Assert.NotNull(firstRestorePointRoot);
        Assert.NotNull(secondRestorePointRoot);

        Assert.True(firstRestorePointRoot is Entities.DataObjects.Directory);
        Assert.True(secondRestorePointRoot is Entities.DataObjects.Directory);

        var firstRestorePointDirectory = firstRestorePointRoot as Entities.DataObjects.Directory;
        var secondRestorePointDirectory = secondRestorePointRoot as Entities.DataObjects.Directory;

        Assert.Equal(2, firstRestorePointDirectory?.DataObjects.Count);
        Assert.True(firstRestorePointDirectory?.DataObjects[0] is ZipArchiveStorage);
        Assert.True(firstRestorePointDirectory?.DataObjects[1] is ZipArchiveStorage);

        Assert.Equal(1, secondRestorePointDirectory?.DataObjects.Count);
        Assert.True(secondRestorePointDirectory?.DataObjects[0] is ZipArchiveStorage);
    }
}
