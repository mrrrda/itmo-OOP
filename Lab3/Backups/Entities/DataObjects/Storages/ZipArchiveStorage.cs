using System.Collections.ObjectModel;
using System.IO.Compression;

namespace Backups.Entities.DataObjects.Storages;

public class ZipArchiveStorage : Storage
{
    private static readonly string ZipExtension = "zip";

    public ZipArchiveStorage(string name, ZipArchive data)
        : base(name, ZipExtension, (object)data)
    { }

    public override FileStream CreateFile()
    {
        using FileStream fileStream = System.IO.File.Create(Path);
        using var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Update);

        var currentZipArchive = (ZipArchive)Data;
        ReadOnlyCollection<ZipArchiveEntry> zipArchiveEntries = currentZipArchive.Entries;

        foreach (ZipArchiveEntry zipArchiveEntry in zipArchiveEntries)
        {
            ZipArchiveEntry copiedEntry = zipArchive.CreateEntry(zipArchiveEntry.FullName);
            zipArchiveEntry.Open().CopyTo(copiedEntry.Open());
        }

        return fileStream;
    }
}
