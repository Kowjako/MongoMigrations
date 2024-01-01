using Mediatr.Api.MongoModels;
using MongoDB.Driver;

namespace Mediatr.Api.Migration.Concrete
{
    public class SampleSchools_1002 : IMongoMigration
    {
        private readonly IMongoDatabase _db;

        public SampleSchools_1002(IMongoDatabase db)
        {
            _db = db;
        }

        public Task MigrateDown()
        {
            var coll = _db.GetCollection<School>("schools");
            return coll.DeleteManyAsync(Builders<School>.Filter.Empty);
        }

        public Task MigrateUp()
        {
            var coll = _db.GetCollection<School>("schools");
            return coll.InsertOneAsync(new School()
            {
                Name = "abc",
                Street = "Prusa"
            });
        }

        public long Version() => 1002;
    }
}
