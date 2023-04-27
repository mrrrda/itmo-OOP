namespace Backups.Entities.DataObjects.Storages;

public abstract class Storage : BaseFile
{
    protected Storage(string name, string extension, object data)
        : base(name, extension)
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data), "Invalid data");

        Data = data;
    }

    public object Data { get; }

    public abstract FileStream CreateFile();
}
