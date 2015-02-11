using System;
using MongoDB.Driver;

namespace MongoDBOutputCache
{
    internal static class MongoDBHelper
    {
        public static MongoCollection<T> GetCollection<T>(string connectionString, string databaseName, string collectionName)
        {
            var url = new MongoUrl(connectionString);
            var client = new MongoClient(url);
            var server = client.GetServer();
            var database = server.GetDatabase(databaseName);
            return database.GetCollection<T>(collectionName);
        }
    }
}