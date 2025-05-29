using effectServiceAPI.Model;
using Mongo2Go;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Bson;

namespace effectServiceAPI.Services
{
    public class EffectServiceIntegrationMongo : IDisposable
    {
        private readonly MongoDbRunner _runner;
        private readonly IMongoCollection<Effect> _effectCollection;

        static EffectServiceIntegrationMongo()
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        }

        public EffectServiceIntegrationMongo()
        {
            _runner = MongoDbRunner.Start();
            var client = new MongoClient(_runner.ConnectionString);
            var database = client.GetDatabase("EffectServiceDB");
            _effectCollection = database.GetCollection<Effect>("Effects");
        }

        public Effect CreateEffect(Effect effect)
        {
            if (effect.EffectId == Guid.Empty)
                effect.EffectId = Guid.NewGuid();

            _effectCollection.InsertOne(effect);
            return effect;
        }

        public Effect? GetEffect(Guid id)
        {
            return _effectCollection.Find(e => e.EffectId == id).FirstOrDefault();
        }

        public IEnumerable<Effect> GetAllEffects()
        {
            return _effectCollection.Find(_ => true).ToList();
        }

        public void DeleteEffect(Guid id)
        {
            _effectCollection.DeleteOne(e => e.EffectId == id);
        }

        public void Dispose()
        {
            _runner.Dispose();
        }
    }
}