using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CryptoHarbour.Periscope.RigDataCollector.Tests
{
    [TestClass]
    public class MongoDbStorageTests
    {
        [TestMethod]
        public void TestSave()
        {
            var storg = new MongoDbStorage("farm", "test_collection", "mongodb://donkey:rx480x5@farm-shard-00-00-ivztf.mongodb.net:27017,farm-shard-00-01-ivztf.mongodb.net:27017,farm-shard-00-02-ivztf.mongodb.net:27017/farm?ssl=true&replicaSet=farm-shard-0&authSource=admin");
            storg.Save(@"{ ""field"" : ""value"" }");
        }
    }
}
