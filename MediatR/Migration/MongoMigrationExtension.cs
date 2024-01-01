using Mediatr.Api.Context;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Mediatr.Api.Migration
{
    public record Configuration
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        public long DbVersion { get; set; }
    }

    public static class MongoMigrationExtension
    {
        public static async Task<IApplicationBuilder> MigrateMongoDbAsync(this IApplicationBuilder app)
        {
            var settings = app.ApplicationServices.GetRequiredService<IOptions<MongoSettings>>();
            var client = new MongoClient(settings.Value.ConnectionString);

            var db = client.GetDatabase(settings.Value.DatabaseName);
            var configCollection = db.GetCollection<Configuration>("configuration");
            var latestMigration = await configCollection.Find(Builders<Configuration>.Filter.Empty).FirstAsync();

            // Get Migration Scripts
            var mongoMigration = typeof(IMongoMigration);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                                               .SelectMany(x => x.GetTypes())
                                               .Where(p => mongoMigration.IsAssignableFrom(p) && !p.IsInterface);

            var migrations = new List<IMongoMigration>();

            // Create migration classes via reflection
            foreach (var migration in types)
            {
                migrations.Add((IMongoMigration)Activator.CreateInstance(migration, new[] { db })!);
            }

            var filteredMigrations = migrations.Where(x => x.Version() > latestMigration.DbVersion)
                                               .OrderBy(x => x.Version());

            // Apply needed migrations
            foreach(var migrationToApply in filteredMigrations)
            {
                await migrationToApply.MigrateUp();
            }

            // Up db version
            var latestMigrationVersion = filteredMigrations.Last().Version();
            await configCollection.UpdateOneAsync(
                Builders<Configuration>.Filter.Empty,
                Builders<Configuration>.Update.Set(x => x.DbVersion, latestMigrationVersion));

            return app;
        }
    }
}
