using Mediatr.Api.MongoModels;
using MongoDB.Driver;

namespace Mediatr.Api.Migration.Concrete
{
    public class PersonIsProcessedByDefault_1003 : IMongoMigration
    {
        private readonly IMongoDatabase _db;

        public PersonIsProcessedByDefault_1003(IMongoDatabase db)
        {
            _db = db;
        }

        public Task MigrateDown()
        {
            var personsColl = _db.GetCollection<People>("persons");
            return personsColl.UpdateManyAsync(Builders<People>.Filter.Empty,
                                               Builders<People>.Update.Unset(x => x.IsProcessed));
        }

        public Task MigrateUp()
        {
            var personsColl = _db.GetCollection<People>("persons");
            return personsColl.UpdateManyAsync(Builders<People>.Filter.Empty,
                                               Builders<People>.Update.Set(x => x.IsProcessed, true));
        }

        public long Version() => 1003;
    }
}
