namespace Backups.Entities.DataObjects.Repositories;

public abstract class Repository : Directory
{
    protected Repository(string name)
        : base(name)
    { }
}
