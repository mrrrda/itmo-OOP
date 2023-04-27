using System.Text;
using System.Text.RegularExpressions;

namespace Backups.Entities.DataObjects;

public abstract class BaseFile : DataObject
{
    public BaseFile(string name, string extension)
        : base(name)
    {
        if (string.IsNullOrEmpty(extension) || !Regex.IsMatch(extension, RegexExtension))
            throw new ArgumentException("Invalid file extension", nameof(extension));

        Extension = extension;
    }

    public override string FullName
    {
        get => new StringBuilder()
            .Append(Name)
            .Append(".")
            .Append(Extension)
            .ToString();
    }

    public override string Extension { get; }
}
