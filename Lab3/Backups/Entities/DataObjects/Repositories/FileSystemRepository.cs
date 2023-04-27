using Backups.Entities.DataObjects.Storages;
using Backups.Exceptions;

namespace Backups.Entities.DataObjects.Repositories;

public class FileSystemRepository : Repository
{
    public FileSystemRepository(string name, string path)
        : base(name)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Invalid path", nameof(path));

        if (!System.IO.Directory.Exists(path))
            throw new ArgumentException("Directory path does not exist", nameof(path));

        Root = new DirectoryInfo(System.IO.Path.Combine(path, name));
        Root.Create();
    }

    public DirectoryInfo Root { get; }

    public override void AddDataObject(DataObject newDataObject, string relativePath)
    {
        base.AddDataObject(newDataObject, relativePath);

        if (newDataObject is Directory)
            CreateDirectoryFS((Directory)newDataObject);
        else if (newDataObject is BaseFile)
            CreateFileFS((BaseFile)newDataObject);
    }

    public override void RemoveDataObject(DataObject dataObjectToRemove, string relativePath)
    {
        base.RemoveDataObject(dataObjectToRemove, relativePath);

        if (dataObjectToRemove is Directory)
            RemoveDirectoryFS((Directory)dataObjectToRemove);
        else if (dataObjectToRemove is File)
            RemoveFileFS((File)dataObjectToRemove);
    }

    private void CreateDirectoryFS(Directory directory)
    {
        if (directory is null)
            throw new ArgumentNullException(nameof(directory), "Invalid directory");

        System.IO.Directory.CreateDirectory(System.IO.Path.GetFullPath(directory.Path));

        foreach (DataObject dataObject in directory.DataObjects)
        {
            if (dataObject is Directory)
                CreateDirectory((Directory)dataObject);
            else if (dataObject is File)
                CreateFile((File)dataObject);
        }
    }

    private void CreateFileFS(BaseFile baseFile)
    {
        if (baseFile is null)
            throw new ArgumentNullException(nameof(baseFile), "Invalid file");

        if (baseFile is File)
        {
            using FileStream fileStream = System.IO.File.OpenWrite(System.IO.Path.GetFullPath(baseFile.Path));

            var file = (File)baseFile;
            fileStream.Write(file.ReadBytes());
        }
        else if (baseFile is Storage)
        {
            var storage = (Storage)baseFile;
            storage.CreateFile();
        }
    }

    private void RemoveDirectoryFS(Directory directory)
    {
        if (!System.IO.Directory.Exists(directory.Path))
            throw new NotExistingDataObjectException();

        System.IO.Directory.Delete(directory.Path);
    }

    private void RemoveFileFS(File file)
    {
        if (!System.IO.File.Exists(file.Path))
            throw new NotExistingDataObjectException();

        System.IO.File.Delete(file.Path);
    }
}
