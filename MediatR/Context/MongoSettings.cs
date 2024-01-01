using Mediatr.Api.MongoModels;

namespace Mediatr.Api.Context
{
    public class MongoSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}
