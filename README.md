# Mongo Migrations 🍀
Repozytorium przedstawia prosta implementacje migracji z baza danych NoSQL - MongoDB.
W obecnych czasach, gdy mamy do czynienia z bazami SQL (np. SQL Server) to sprawa jest prosta, migracje są generowane
za pomoca Entity Framework, albo gdy nie mamy Entity Framework, to mozemy pisac swoje skrypty .sql i jakis prosty skrypt
ktory by je odpalal na starcie aplikacji.  
  
Z MongoDb zrobilem podobna implementacje, tylko skoro MongoDb nie ma czegos takiego jak SQL, wiec podejscie z pisaniem
raw-skryptow nie jest akceptowalne. Wiec w jaki sposob to zrobilem:

Sidenote: Rowniez w repozytrorium sa rozne zabawki implementacyjne, takie jak CQRS oraz pipeline'y (logowania, walidacji) za pomocą MediatR + FluentValidation.
A takze rozne ASP.NET filtry: Resource Filter (do Correlation-Id), Action Filter (Cachowanie, Ostatnia aktywnosc uzytkownika).
Ale najwazniejsze co tu jest, to podejscie do migracji bazy MongoDB.

# Implementacja
1. Interfejs ktory przedstawia pojedyncza migracje:
```csharp
namespace Mediatr.Api.Migration;

public interface IMongoMigration
{
    long Version();
    Task MigrateUp();
    Task MigrateDown();
}

```
2. Przykladowe skrypty migracji:
```csharp
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
```
```csharp
namespace Mediatr.Api.Migration.Concrete;

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
```
```csharp
namespace Mediatr.Api.Migration.Concrete;

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
```
3. Extension do aplikowania migracji
```csharp
namespace Mediatr.Api.Migration;

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
        foreach (var migrationToApply in filteredMigrations)
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
```
4. Uzycie w glownym pipeline ASP.NET Web API:
```csahrp
builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("MongoSettings"));
await app.MigrateMongoDbAsync();
```
