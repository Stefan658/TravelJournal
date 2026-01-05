using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelJournal.Services.Interfaces
{
    public interface ICache
    {
        T Get<T>(string key);
        void Set(string key, object data, int? cacheTime = null);
        bool IsSet(string key);
        void Remove(string key);
        void RemoveByPattern(string pattern);
        void Clear();
    }
}
