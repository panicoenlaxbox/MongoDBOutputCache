using MongoDB.Driver;

namespace MongoDBOutputCache
{
    internal static class MongoDBHelper
    {
        public static MongoDatabase GetDatabase(string connectionString)
        {
            var position = connectionString.LastIndexOf("/");
            var databaseName = connectionString.Substring(position + 1);
            var url = new MongoUrl(connectionString.Substring(0, position));
            var client = new MongoClient(url);
            var server = client.GetServer();
            var database = server.GetDatabase(databaseName);
            return database;
        }
    }
}