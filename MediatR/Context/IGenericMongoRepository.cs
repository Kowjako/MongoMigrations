using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Mediatr.Api.Context;

public abstract record MongoModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
}

public interface IGenericMongoRepository<T> where T : MongoModel
{
    IMongoCollection<T> CollectionRef { get; }
    Task<IReadOnlyList<T>> GetAll();
    Task<IReadOnlyList<T>> GetAll(FilterDefinition<T>? filter,
                                  SortDefinition<T>? sort,
                                  int skip, int take);
}

public class GenericMongoRepository<T> : IGenericMongoRepository<T> where T : MongoModel
{
    private readonly MongoContext _context;
    private readonly IMongoCollection<T> _collection;

    public IMongoCollection<T> CollectionRef => _collection;

    public GenericMongoRepository(MongoContext context)
    {
        _context = context;
        _collection = _context.GetCollectionReference<T>();
    }

    public async Task<IReadOnlyList<T>> GetAll()
    {
        var collectionRef = _context.GetCollectionReference<T>();
        return await collectionRef.Find(Builders<T>.Filter.Empty).ToListAsync();
    }

    public async Task<T> GetById(string objectId)
    {
        var collectionRef = _context.GetCollectionReference<T>();
        return await collectionRef.Find(Builders<T>.Filter.Eq(x => x.Id, objectId)).FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<T>> GetAll(FilterDefinition<T>? filter,
                                               SortDefinition<T>? sort,
                                               int skip, int take)
    {
        var collectionRef = _context.GetCollectionReference<T>();
        return await collectionRef.Find(filter ?? Builders<T>.Filter.Empty)
                                  .Sort(sort ?? Builders<T>.Sort.Ascending(x => x.Id))
                                  .Skip(skip)
                                  .Limit(take)
                                  .ToListAsync();
    }

    public Task AddAsync(T item) => _collection.InsertOneAsync(item);

    public Task AddAsync(List<T> item) => _collection.InsertManyAsync(item);

    public async Task<bool> UpdatePropertyAsync(FilterDefinition<T> filter,
                                                Expression<Func<T, object>> property,
                                                object value)
    {
        var result = await _collection.UpdateManyAsync(filter, Builders<T>.Update.Set(property, value));
        return result.IsAcknowledged;
    }

    public async Task<bool> DeleteByIdAsync(string id)
    {
        var result = await _collection.DeleteOneAsync(Builders<T>.Filter.Eq(x => x.Id, id));
        return result.IsAcknowledged;
    }

    public async Task<bool> DeleteByCriteriaAsync(FilterDefinition<T> filter)
    {
        var result = await _collection.DeleteManyAsync(filter);
        return result.IsAcknowledged;
    }

    public async Task<bool> ReplaceModelAsync(string id, T newModel)
    {
        var result = await _collection.ReplaceOneAsync(Builders<T>.Filter.Eq(x => x.Id, id), newModel);
        return result.IsAcknowledged;
    }

    public async Task<List<TProjection>> GetAllWithProjection<TProjection>()
    {
        return await _collection.Find(Builders<T>.Filter.Empty)
                                .Project(Builders<T>.Projection.As<TProjection>())
                                .ToListAsync();
    }

    public async Task<List<T>> FullTextSearch(string query)
    {
        return await _collection.Find(Builders<T>.Filter.Text(query)).ToListAsync();
    }
}



public class MongoUnitOfWork
{
    private readonly List<Func<Task>> _operations = new List<Func<Task>>();
    private readonly IMongoClient _client;

    public MongoUnitOfWork(IOptions<MongoSettings> config)
    {
        _client = new MongoClient(config.Value.ConnectionString);
    }

    public void AddOperation(Func<Task> operation) => _operations.Add(operation);
    public void CleanOperations() => _operations.Clear();

    public async Task CommitAsync()
    {
        using var session = await _client.StartSessionAsync();
        session.StartTransaction();

        foreach (var operation in _operations)
        {
            await operation();
        }

        await session.CommitTransactionAsync();
    }

    // How to use
    // Our Repository should return only tasks:

    //public sealed class ProductRepository : IProductRepository
    //{
    //    private readonly IMongoDatabase _database;
    //    private readonly IMongoCollection<ProductMongoEntity> _productsCollection;
    //    private readonly IUnitOfWork _unitOfWork;

    //    public ProductRepository(IUnitOfWork unitOfWork)
    //    {
    //        _unitOfWork = unitOfWork;

    //        var mongoClient = new MongoClient("connectionString");
    //        _database = mongoClient.GetDatabase("databaseName");
    //        _productsCollection = this._database.GetCollection<ProductMongoEntity>("collectionName");
    //    }

    //    public void Add(IProduct product)
    //    {
    //        Action operation = () => this._productsCollection.InsertOne(this._unitOfWork.Session as IClientSessionHandle, product);
    //        this._unitOfWork.AddOperation(operation);
    //    }

    //    public void Update(IProduct product)
    //    {
    //        Action operation = () => this._productsCollection.ReplaceOne(this._unitOfWork.Session as IClientSessionHandle, x => x.Id == product.Id, product);
    //        this._unitOfWork.AddOperation(operation);
    //    }

    //    public void Remove(ProductId id)
    //    {
    //        Action operation = () => this._productsCollection.DeleteOne(this._unitOfWork.Session as IClientSessionHandle, x => x.Id == id.Id);
    //        this._unitOfWork.AddOperation(operation);
    //    }
    //}
}