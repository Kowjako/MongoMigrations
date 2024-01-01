using Mediatr.Api.Context;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Mediatr.Api.MongoModels;

public record People : MongoModel
{
    public string Name { get; set; }
    public int Age { get; set; }
    public string Hobby { get; set; }
    public int Hash { get; set; }

    public bool IsProcessed { get; set; }

    public Address Address { get; set; }
    public Invoice[] Invoices { get; set; }
    public Job Job { get; set; }
}

public record Address
{
    public string City { get; set; }
    public string Street { get; set; }
    public int Building { get; set; }
}

public record Invoice : MongoModel
{
    public static string MongoCollectionName => "invoices";
    public string Name { get; set; }
    [BsonRepresentation(BsonType.ObjectId)]
    public string PeopleId { get; set; }
}

public record Job : MongoModel
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string PeopleId { get; set; }
    public string Title { get; set; }
}