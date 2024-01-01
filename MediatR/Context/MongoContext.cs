using Mediatr.Api.MongoModels;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Mediatr.Api.Context
{
    public static class MongoCollections
    {
        private const string Invoices = "invoices";
        private const string Jobs = "jobs";
        private const string Persons = "persons";

        public static string GetNameByType(Type type)
        {
            return type switch
            {
                var x when typeof(Invoice) == x => Invoices,
                var x when typeof(Job) == x => Jobs,
                var x when typeof(People) == x => Persons,
                _ => throw new InvalidDataException()
            };
        }
    }

    public class MongoContext
    {
        private readonly IMongoDatabase _database;

        public MongoContext(IOptions<MongoSettings> config)
        {
            var client = new MongoClient(config.Value.ConnectionString);
            _database = client.GetDatabase(config.Value.DatabaseName);
        }

        public IMongoCollection<T> GetCollectionReference<T>() 
            => _database.GetCollection<T>(MongoCollections.GetNameByType(typeof(T)));
    }
}
