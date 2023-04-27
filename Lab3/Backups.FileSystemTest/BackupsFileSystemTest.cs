using Backups.Entities;
using Backups.Entities.DataObjects.Repositories;
using Backups.Entities.StorageAlgorithms;

using Directory = Backups.Entities.DataObjects.Directory;
using File = Backups.Entities.DataObjects.File;

string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
System.IO.Directory.SetCurrentDirectory(path);

var repository = new FileSystemRepository("Repository", ".");

var firstFile = new File("File1", "txt");
var secondFile = new File("File2", "txt");
var thirdFile = new File("File3", "txt");

var folder = new Directory("folder");

repository.AddDataObject(firstFile, ".");
repository.AddDataObject(secondFile, ".");
repository.AddDataObject(folder, ".");
repository.AddDataObject(thirdFile, Path.Combine(".", folder.Name));

var firstBackupTask = new BackupTask("BackupTaskSingle", repository, new SingleStorageAlgorithm());

firstBackupTask.AddBackupObject(new BackupObject(firstFile));
firstBackupTask.AddBackupObject(new BackupObject(secondFile));
firstBackupTask.AddBackupObject(new BackupObject(folder));
firstBackupTask.AddBackupObject(new BackupObject(thirdFile));

firstBackupTask.Execute();

var secondBackupTask = new BackupTask("BackupTaskSplit", repository, new SplitStorageAlgorithm());

secondBackupTask.AddBackupObject(new BackupObject(firstFile));
secondBackupTask.AddBackupObject(new BackupObject(secondFile));

secondBackupTask.Execute();
