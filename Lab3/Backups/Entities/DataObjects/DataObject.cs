using System.Text.RegularExpressions;

namespace Backups.Entities.DataObjects;

public abstract class DataObject
{
    protected static readonly string InvalidNameCharacters = "\\/:*?<>|";
    protected static readonly string RegexInvalidDataObjectName = string.Format("[{0}]", InvalidNameCharacters);
    protected static readonly string RegexExtension = "^[a-zA-Z0-9]{1,5}$";

    protected static readonly int MinDataObjectNameLength = 1;
    protected static readonly int MaxDataObjectNameLength = 128;

    protected DataObject(string name)
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name) || Regex.IsMatch(name, RegexInvalidDataObjectName))
        {
            throw new ArgumentException(
                string.Format("Data object name must be non-empty string and must not contain the following characters: {0}", InvalidNameCharacters),
                nameof(name));
        }

        if (name.Length < MinDataObjectNameLength || name.Length > MaxDataObjectNameLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(name),
                string.Format("Data object name length must be from {0} to {1}", MinDataObjectNameLength, MaxDataObjectNameLength));
        }

        CreationTime = DateTime.Now;

        Name = name;
    }

    public string Name { get; }

    public abstract string FullName { get; }

    public abstract string Extension { get; }

    public string Path
    {
        get
        {
            var path = new List<string>();
            path.Add(FullName);

            DataObject? currentParent = Parent;
            while (currentParent is not null)
            {
                path.Add(currentParent.FullName);
                currentParent = currentParent.Parent;
            }

            path.Reverse();

            return System.IO.Path.Combine(path.ToArray());
        }
    }

    public DateTime CreationTime { get; }

    public DataObject? Parent { get; internal set; }

    public override bool Equals(object? obj)
    {
        return obj is DataObject other
            && Path.Equals(other.Path);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this);
    }
}
