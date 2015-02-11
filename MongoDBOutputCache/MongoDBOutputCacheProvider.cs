using System;
using System.Configuration;
using System.Web.Caching;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace MongoDBOutputCache
{
    /// <summary>
    /// Output cache provider.
    /// </summary>
    /// <remarks>http://msdn.microsoft.com/en-us/magazine/gg650661.aspx</remarks>
    public class MongoDBOutputCacheProvider : OutputCacheProvider
    {
        private readonly string _collectionName;
        private readonly string _connectionString;
        private readonly string _databaseName;

        public MongoDBOutputCacheProvider()
        {
            _connectionString = ConfigurationManager.AppSettings[Constants.MongoDBOutputCacheProviderConnectionString];
            _collectionName = ConfigurationManager.AppSettings[Constants.MongoDBOutputCacheProviderCollectionName];
            _databaseName = ConfigurationManager.AppSettings[Constants.MongoDBOutputCacheProviderDatabaseName];
        }

        public override object Add(string key, object entry, DateTime utcExpiry)
        {
            var collection = GetCollection();
            var item = collection.FindOne(Query<CacheItem>.EQ(p => p.Id, key));

            if (item != null)
            {
                if (item.Expiration <= DateTime.UtcNow)
                {
                    collection.Remove(Query<CacheItem>.EQ(p => p.Id, item.Id));
                }
                else
                {
                    return BinarySerializer.Deserialize(item.Item);
                }
            }

            collection.Insert(new CacheItem
            {
                Id = key,
                Item = BinarySerializer.Serialize(entry),
                Expiration = utcExpiry,
                CreatedDate = DateTime.UtcNow
            });

            return entry;
        }

        public override object Get(string key)
        {
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

        public override void Remove(string key)
        {
            GetCollection().Remove(Query<CacheItem>.EQ(p => p.Id, key));
        }

        public override void Set(string key, object entry, DateTime utcExpiry)
        {
            var collection = GetCollection();
            var item = collection.FindOne(Query<CacheItem>.EQ(p => p.Id, key));

            if (item != null)
            {
                item.Item = BinarySerializer.Serialize(entry);
                item.Expiration = utcExpiry;
                collection.Save(item);
            }
            else
            {
                collection.Insert(new CacheItem
                {
                    Id = key,
                    Item = BinarySerializer.Serialize(entry),
                    Expiration = utcExpiry,
                    CreatedDate = DateTime.UtcNow
                });
            }
        }

        private MongoCollection<CacheItem> GetCollection()
        {
            return MongoDBHelper.GetCollection<CacheItem>(_connectionString, _databaseName, _collectionName);
        }
    }
}