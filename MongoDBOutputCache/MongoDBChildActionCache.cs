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
        private MongoCollection<CacheItem> _collection;
        private bool _ouputCacheEnabled;

        public MongoDBChildActionCache(string name)
            : base(name)
        {
            var connectionString = ConfigurationManager.AppSettings["MongoDBOutputCacheProviderConnectionString"];
            var collectionName = ConfigurationManager.AppSettings["MongoDBOutputCacheProviderCollection"];
            _collection = MongoDBHelper.GetDatabase(connectionString).GetCollection<CacheItem>(collectionName);
            _ouputCacheEnabled = ((OutputCacheSection)ConfigurationManager.GetSection("system.web/caching/outputCache")).EnableOutputCache;
        }

        public override bool Add(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            if (!_ouputCacheEnabled)
            {
                return false;
            }

            var item = _collection.FindOne(Query<CacheItem>.EQ(p => p.Id, key));

            if (item != null)
            {
                item.Item = BinarySerializer.Serialize(value);
                item.Expiration = absoluteExpiration.UtcDateTime;
                _collection.Save(item);
            }
            else
            {
                _collection.Insert(new CacheItem
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
    }
}