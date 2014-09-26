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
        private MongoCollection<CacheItem> _collection;

        public MongoDBOutputCacheProvider()
        {
            var connectionString = ConfigurationManager.AppSettings["MongoDBOutputCacheProviderConnectionString"];
            var collectionName = ConfigurationManager.AppSettings["MongoDBOutputCacheProviderCollection"];
            _collection = MongoDBHelper.GetDatabase(connectionString).GetCollection<CacheItem>(collectionName);
        }

        public override object Add(string key, object entry, DateTime utcExpiry)
        {
            var item = _collection.FindOne(Query<CacheItem>.EQ(p => p.Id, key));

            if (item != null)
            {
                if (item.Expiration <= DateTime.UtcNow)
                {
                    _collection.Remove(Query<CacheItem>.EQ(p => p.Id, item.Id));
                }
                else
                {
                    return BinarySerializer.Deserialize(item.Item);
                }
            }

            _collection.Insert(new CacheItem
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
            var cacheItem = _collection.FindOne(Query<CacheItem>.EQ(p => p.Id, key));

            if (cacheItem != null)
            {
                if (cacheItem.Expiration <= DateTime.UtcNow)
                {
                    _collection.Remove(Query<CacheItem>.EQ(p => p.Id, cacheItem.Id));
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
            _collection.Remove(Query<CacheItem>.EQ(p => p.Id, key));
        }

        public override void Set(string key, object entry, DateTime utcExpiry)
        {
            var item = _collection.FindOne(Query<CacheItem>.EQ(p => p.Id, key));

            if (item != null)
            {
                item.Item = BinarySerializer.Serialize(entry);
                item.Expiration = utcExpiry;
                _collection.Save(item);
            }
            else
            {
                _collection.Insert(new CacheItem
                {
                    Id = key,
                    Item = BinarySerializer.Serialize(entry),
                    Expiration = utcExpiry,
                    CreatedDate = DateTime.UtcNow
                });
            }
        }
    }
}