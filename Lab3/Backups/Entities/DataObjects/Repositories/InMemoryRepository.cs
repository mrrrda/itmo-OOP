namespace Backups.Entities.DataObjects.Repositories;

public class InMemoryRepository : Repository
{
    public InMemoryRepository(string name)
        : base(name)
    { }
}
