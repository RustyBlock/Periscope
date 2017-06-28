using CryptoHarbour.Periscope.Contracts;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace CryptoHarbour.Periscope.RigDataCollector
{
    public class MongoDbStorage : IStorage
    {
        private IMongoCollection<BsonDocument> _collection;

        public MongoDbStorage(string dbName, string dbCollection, string connectionString)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(dbName);
            _collection = database.GetCollection<BsonDocument>(dbCollection);
        }

        public void Save(string json)
        {
            using (var jsonReader = new JsonReader(json))
            {
                var context = BsonDeserializationContext.CreateRoot(jsonReader);
                var document = _collection.DocumentSerializer.Deserialize(context);
                _collection.InsertOne(document);
            }
        }
    }
}
