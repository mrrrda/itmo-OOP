using System.IO.Compression;
using System.Text;

using Backups.Entities.DataObjects.Storages;

namespace Backups.Entities.StorageAlgorithms;

public class SingleStorageAlgorithm : StorageAlgorithm
{
    private static readonly string SingleStoragePrefix = "SingleStorage";
    private static readonly string NameDataSeparator = "-";

    public override List<Storage> CreateStorage(List<BackupObject> backupObjects)
    {
        if (backupObjects is null)
            throw new ArgumentNullException(nameof(backupObjects), "Invalid backup objects list");

        if (backupObjects.Count == 0)
            throw new ArgumentException("Empty backup objects list", nameof(backupObjects));

        Dictionary<BackupObject, string> backupObjectsPaths = GetBackupObjectsPaths(backupObjects);

        var memoryStream = new MemoryStream();
        var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Update);

        foreach (BackupObject backupObject in backupObjects)
        {
            if (backupObject.DataObject is not DataObjects.File backupFile)
                continue;

            ZipArchiveEntry zipArchiveEntry = zipArchive.CreateEntry(backupObjectsPaths[backupObject]);

            using Stream zipArchiveEntryStream = zipArchiveEntry.Open();
            backupFile.Data.CopyTo(zipArchiveEntryStream);
        }

        var storages = new List<Storage>();
        storages.Add(new ZipArchiveStorage(GenerateStorageName(), zipArchive));

        return storages;
    }

    private string GenerateStorageName()
    {
        return new StringBuilder(SingleStoragePrefix)
            .Append(NameDataSeparator)
            .Append(DateTime.Now.ToString("MM.dd.yyyy-HH.mm.ss.fff"))
            .ToString();
    }
}
