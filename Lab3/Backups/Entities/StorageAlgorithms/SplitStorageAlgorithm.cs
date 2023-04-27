using System.IO.Compression;
using System.Text;

using Backups.Entities.DataObjects.Storages;

namespace Backups.Entities.StorageAlgorithms;

public class SplitStorageAlgorithm : StorageAlgorithm
{
    public override List<Storage> CreateStorage(List<BackupObject> backupObjects)
    {
        if (backupObjects is null)
            throw new ArgumentNullException(nameof(backupObjects), "Invalid backup objects list");

        if (backupObjects.Count == 0)
            throw new ArgumentException("Empty backup objects list", nameof(backupObjects));

        Dictionary<BackupObject, string> backupObjectsPaths = GetBackupObjectsPaths(backupObjects);

        var zipArchives = new Dictionary<string, ZipArchive>();

        foreach (BackupObject backupObject in backupObjects)
        {
            if (backupObject.DataObject is not DataObjects.File backupFile)
                continue;

            var memoryStream = new MemoryStream();
            var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Update);

            ZipArchiveEntry zipArchiveEntry = zipArchive.CreateEntry(backupObjectsPaths[backupObject]);

            using Stream zipArchiveEntryStream = zipArchiveEntry.Open();
            backupFile.Data.CopyTo(zipArchiveEntryStream);

            zipArchives.Add(GenerateStorageName(backupObject), zipArchive);
        }

        var storages = new List<Storage>();
        foreach (string zipArchiveName in zipArchives.Keys)
            storages.Add(new ZipArchiveStorage(zipArchiveName, zipArchives[zipArchiveName]));

        return storages;
    }

    private string GenerateStorageName(BackupObject backupObject)
    {
        return new StringBuilder(backupObject.DataObject.Name)
            .Append("(")
            .Append(backupObject.BackupVersion)
            .Append(")")
            .ToString();
    }
}
