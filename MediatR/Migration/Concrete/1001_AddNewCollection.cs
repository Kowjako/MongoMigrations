using MongoDB.Driver;

namespace Mediatr.Api.Migration.Concrete;

public class AddNewCollection_1001 : IMongoMigration
{
    private readonly IMongoDatabase _db;

    public AddNewCollection_1001(IMongoDatabase db)
    {
        _db = db;
    }

    public Task MigrateDown()
    {
        return _db.DropCollectionAsync("schools");
    }

    public Task MigrateUp()
    {
        return _db.CreateCollectionAsync("schools");
    }

    public long Version() => 1001;
}

