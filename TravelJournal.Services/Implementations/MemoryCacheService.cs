using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Caching;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.AccessControl;
using TravelJournal.Services.Interfaces;

namespace TravelJournal.Services.Implementations
{

    public class MemoryCacheService : ICache
    {
        private readonly int _cacheTime;

        public MemoryCacheService(int cacheTime = 60)
        {
            _cacheTime = cacheTime;
        }

        protected ObjectCache Cache
        {
            get { return MemoryCache.Default; }
        }

        public T Get<T>(string key)
        {
            BinaryFormatter deserializer = new BinaryFormatter();
            using (MemoryStream memStream = new MemoryStream((byte[])Cache[key]))
            {
                return (T)deserializer.Deserialize(memStream);
            }
        }

        public void Set(string key, object data, int? cacheTime = null)
        {
            if (data == null) return;

            if (!cacheTime.HasValue)
                cacheTime = _cacheTime;

            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(cacheTime.Value)
            };

            BinaryFormatter serializer = new BinaryFormatter();
            using (MemoryStream memStream = new MemoryStream())
            {
                serializer.Serialize(memStream, data);
                Cache.Add(new CacheItem(key, memStream.ToArray()), policy);
            }
        }

        public bool IsSet(string key)
        {
            return Cache.Contains(key);
        }

        public void Remove(string key)
        {
            Cache.Remove(key);
        }

        public void RemoveByPattern(string pattern)
        {
            foreach (var item in Cache)
            {
                if (item.Key.StartsWith(pattern))
                    Remove(item.Key);
            }
        }

        public void Clear()
        {
            foreach (var item in Cache)
            {
                Remove(item.Key);
            }
        }
    }

}
