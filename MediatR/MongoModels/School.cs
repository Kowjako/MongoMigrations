using Mediatr.Api.Context;

namespace Mediatr.Api.MongoModels
{
    public record School : MongoModel
    {
        public string Street { get; set; }
        public string Name { get; set; }
    }
}
