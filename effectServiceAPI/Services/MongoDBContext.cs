using effectServiceAPI.Model;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace auctionServiceAPI.Services
{
    /// <summary>
    /// MongoDB database context for effect service.
    /// </summary>
    public class MongoDBContext
    {
        public IMongoDatabase Database { get; set; }
        public IMongoCollection<Effect> Collection { get; set; }

        /// <summary>
        /// Create an instance of the effect context class.
        /// </summary>
        /// <param name="logger">Global logging facility.</param>
        /// <param name="config">System configuration instance.</param>
        public MongoDBContext(ILogger<MongoDBContext> logger, IConfiguration config)
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));

            var client = new MongoClient(config["EffectMongoConnectionString"] ?? config["MongoConnectionString"]);
            Database = client.GetDatabase(config["EffectDatabase"]);
            Collection = Database.GetCollection<Effect>(config["EffectCollection"]);

            logger.LogInformation($"Connected to database {config["EffectDatabase"]}");
            logger.LogInformation($"Using collection {config["EffectCollection"]}");
        }
    }
}