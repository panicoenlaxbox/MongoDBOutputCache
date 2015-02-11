using System;
using System.Configuration;
using System.Runtime.Caching;
using System.Web.Configuration;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace MongoDBOutputCache
{
    /// <summary>
    /// Output cache provider for child actions.
    /// </summary>
    /// <remarks>http://www.haneycodes.net/custom-output-caching-with-mvc3-and-net-4-0-done-right/</remarks>
    public class MongoDBChildActionCache : MemoryCache
    {
        private readonly string _collectionName;
        private readonly string _connectionString;
        private readonly string _databaseName;
        private readonly bool _ouputCacheEnabled;

        public MongoDBChildActionCache(string name)
            : base(name)
        {
            _connectionString = ConfigurationManager.AppSettings[Constants.MongoDBOutputCacheProviderConnectionString];
            _collectionName = ConfigurationManager.AppSettings[Constants.MongoDBOutputCacheProviderCollectionName];
            _databaseName = ConfigurationManager.AppSettings[Constants.MongoDBOutputCacheProviderDatabaseName];
            _ouputCacheEnabled = ((OutputCacheSection)ConfigurationManager.GetSection("system.web/caching/outputCache")).EnableOutputCache;
        }

        public override bool Add(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            if (!_ouputCacheEnabled)
            {
                return false;
            }

            var collection = GetCollection();
            var item = collection.FindOne(Query<CacheItem>.EQ(p => p.Id, key));

            if (item != null)
            {
                item.Item = BinarySerializer.Serialize(value);
                item.Expiration = absoluteExpiration.UtcDateTime;
                collection.Save(item);
            }
            else
            {
                collection.Insert(new CacheItem
                {
                    Id = key,
                    Item = BinarySerializer.Serialize(value),
                    Expiration = absoluteExpiration.UtcDateTime,
                    CreatedDate = DateTime.UtcNow,
                    ChildAction = true
                });
            }
            return true;
        }

        public override object Get(string key, string regionName = null)
        {
            if (!_ouputCacheEnabled)
            {
                return null;
            }

            var collection = GetCollection();
            var cacheItem = collection.FindOne(Query<CacheItem>.EQ(p => p.Id, key));

            if (cacheItem != null)
            {
                if (cacheItem.Expiration <= DateTime.UtcNow)
                {
                    collection.Remove(Query<CacheItem>.EQ(p => p.Id, cacheItem.Id));
                }
                else
                {
                    return BinarySerializer.Deserialize(cacheItem.Item);
                }
            }

            return null;
        }

        private MongoCollection<CacheItem> GetCollection()
        {
            return MongoDBHelper.GetCollection<CacheItem>(_connectionString, _databaseName, _collectionName);
        }
    }
}