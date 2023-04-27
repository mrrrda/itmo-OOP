using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;

using Backups.Exceptions;

namespace Backups.Entities.DataObjects;

public class Directory : DataObject
{
    private static readonly string DefaultExtension = string.Empty;

    private List<DataObject> dataObjects;

    public Directory(string name)
        : base(name)
    {
        dataObjects = new List<DataObject>();

        Extension = DefaultExtension;
    }

    public Directory(string name, string extension)
        : base(name)
    {
        dataObjects = new List<DataObject>();

        if (string.IsNullOrEmpty(extension) || !Regex.IsMatch(extension, RegexExtension))
            throw new ArgumentException("Invalid directory extension", nameof(extension));

        Extension = extension;
    }

    public ReadOnlyCollection<DataObject> DataObjects => dataObjects.AsReadOnly();

    public override string FullName
    {
        get
        {
            var fullName = new StringBuilder(Name);

            if (!Extension.Equals(DefaultExtension))
                fullName.Append(".").Append(Extension);

            return fullName.ToString();
        }
    }

    public override string Extension { get; }

    public virtual void AddDataObject(DataObject newDataObject, string relativePath)
    {
        if (newDataObject is null)
            throw new ArgumentNullException(nameof(newDataObject), "Invalid data object");

        if (newDataObject is BaseFile)
            new FileInfo(relativePath);

        if (newDataObject is Directory)
            new DirectoryInfo(relativePath);

        string trimmedPath = relativePath.Trim();

        if (trimmedPath.StartsWith(System.IO.Path.DirectorySeparatorChar))
            throw new ArgumentException("Path must be relative", nameof(relativePath));

        string formattedPath = FormatRelativePath(trimmedPath);

        if (System.IO.Path.GetFileName(formattedPath).Equals(".") || string.IsNullOrEmpty(formattedPath))
        {
            newDataObject.Parent = this;

            if (newDataObject is BaseFile)
                CreateFile((BaseFile)newDataObject);
            else if (newDataObject is Directory)
                CreateDirectory((Directory)newDataObject);
        }
        else
        {
            string[] pathParts = formattedPath.Split(System.IO.Path.DirectorySeparatorChar);
            string currentDirectoryPath = System.IO.Path.Combine(pathParts.Take(1).ToArray());
            string restPath = System.IO.Path.Combine(pathParts.Skip(1).ToArray());

            foreach (DataObject currentDataObject in dataObjects)
            {
                if (currentDataObject is Directory currentDirectory && currentDataObject.FullName.Equals(currentDirectoryPath))
                {
                    currentDirectory.AddDataObject(newDataObject, System.IO.Path.Combine(".", restPath));
                    return;
                }
            }

            var newDirectory = new Directory(currentDirectoryPath);
            newDirectory.AddDataObject(newDataObject, System.IO.Path.Combine(".", restPath));
            CreateDirectory(newDirectory);
        }
    }

    public virtual void RemoveDataObject(DataObject dataObjectToRemove, string relativePath)
    {
        if (dataObjectToRemove is null)
            throw new ArgumentNullException(nameof(dataObjectToRemove), "Invalid data object");

        if (dataObjectToRemove is BaseFile)
            new FileInfo(relativePath);

        if (dataObjectToRemove is Directory)
            new DirectoryInfo(relativePath);

        string trimmedPath = relativePath.Trim();

        if (trimmedPath.StartsWith(System.IO.Path.DirectorySeparatorChar))
            throw new ArgumentException("Path must be relative", nameof(relativePath));

        string formattedPath = FormatRelativePath(trimmedPath);

        if (System.IO.Path.GetFileName(formattedPath).Equals(".") || string.IsNullOrEmpty(formattedPath))
        {
            dataObjectToRemove.Parent = null;

            if (dataObjectToRemove is BaseFile)
                RemoveFile((BaseFile)dataObjectToRemove);
            else if (dataObjectToRemove is Directory)
                RemoveDirectory((Directory)dataObjectToRemove);
        }
        else
        {
            string[] pathParts = formattedPath.Split(System.IO.Path.DirectorySeparatorChar);
            string currentDirectoryPath = System.IO.Path.Combine(pathParts.Take(1).ToArray());
            string restPath = System.IO.Path.Combine(pathParts.Skip(1).ToArray());

            foreach (DataObject currentDataObject in dataObjects)
            {
                if (currentDataObject is Directory currentDirectory && currentDataObject.FullName.Equals(currentDirectoryPath))
                {
                    currentDirectory.RemoveDataObject(dataObjectToRemove, System.IO.Path.Combine(".", restPath));
                    return;
                }
            }

            throw new NotExistingDataObjectException();
        }
    }

    public virtual DataObject? FindDataObject(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            throw new ArgumentException("Invalid relative path", nameof(relativePath));

        string trimmedPath = relativePath.Trim();

        if (trimmedPath.StartsWith(System.IO.Path.DirectorySeparatorChar))
            throw new ArgumentException("Path must be relative", nameof(relativePath));

        string formattedPath = FormatRelativePath(trimmedPath);

        if (string.IsNullOrEmpty(System.IO.Path.GetDirectoryName(formattedPath)))
        {
            foreach (DataObject dataObject in dataObjects)
            {
                if (dataObject.FullName.Equals(formattedPath))
                    return dataObject;
            }

            return null;
        }
        else
        {
            string[] pathParts = formattedPath.Split(System.IO.Path.DirectorySeparatorChar);
            string currentDirectoryPath = System.IO.Path.Combine(pathParts.Take(1).ToArray());
            string restPath = System.IO.Path.Combine(pathParts.Skip(1).ToArray());

            foreach (DataObject currentDataObject in dataObjects)
            {
                if (currentDataObject is Directory currentDirectory && currentDataObject.FullName.Equals(currentDirectoryPath))
                    return currentDirectory.FindDataObject(restPath);
            }

            return null;
        }
    }

    public virtual void CreateDirectory(Directory directory)
    {
        if (directory is null)
            throw new ArgumentNullException(nameof(directory), "Invalid directory");

        if (dataObjects.Contains(directory))
            throw new DuplicateDataObjectException();

        foreach (DataObject dataObject in dataObjects)
        {
            if (directory.Equals(dataObject))
                throw new DuplicateDataObjectException();
        }

        dataObjects.Add(directory);
    }

    public virtual void CreateFile(BaseFile file)
    {
        if (file is null)
            throw new ArgumentNullException(nameof(file), "Invalid file");

        if (dataObjects.Contains(file))
            throw new DuplicateDataObjectException();

        foreach (DataObject dataObject in dataObjects)
        {
            if (file.Equals(dataObject))
                throw new DuplicateDataObjectException();
        }

        dataObjects.Add(file);
    }

    public virtual void RemoveDirectory(Directory directory)
    {
        if (directory is null)
            throw new ArgumentNullException(nameof(directory), "Invalid directory");

        if (!dataObjects.Contains(directory))
            throw new NotExistingDataObjectException();

        if (!dataObjects.Any(dataObject => dataObject.Equals(directory)))
            throw new NotExistingDataObjectException();

        dataObjects.Remove(directory);
    }

    public virtual void RemoveFile(BaseFile file)
    {
        if (file is null)
            throw new ArgumentNullException(nameof(file), "Invalid file");

        if (!dataObjects.Contains(file))
            throw new NotExistingDataObjectException();

        if (!dataObjects.Any(dataObject => dataObject.Equals(file)))
            throw new NotExistingDataObjectException();

        dataObjects.Remove(file);
    }

    private string FormatRelativePath(string relativePath)
    {
        string formattedPath = System.IO.Path.TrimEndingDirectorySeparator(relativePath);

        if (relativePath.StartsWith("." + System.IO.Path.DirectorySeparatorChar))
            formattedPath = relativePath.Substring(2);

        return formattedPath;
    }
}
