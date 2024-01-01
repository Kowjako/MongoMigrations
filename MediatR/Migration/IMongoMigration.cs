namespace Mediatr.Api.Migration
{
    public interface IMongoMigration
    {
        long Version();
        Task MigrateUp();
        Task MigrateDown();
    }
}
