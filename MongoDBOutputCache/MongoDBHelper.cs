using MongoDB.Driver;

namespace MongoDBOutputCache
{
    internal static class MongoDBHelper
    {
        public static MongoDatabase GetDatabase(string connectionString)
        {
            var url = new MongoUrl(connectionString);
            var client = new MongoClient(url);
            var server = client.GetServer();
            return server.GetDatabase(url.DatabaseName);
        }
    }
}