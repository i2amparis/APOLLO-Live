﻿using System;
using System.Threading.Tasks;

namespace Topsis.Application.Contracts.Caching
{
    public interface ICachingService
    {
        Task<TItem> GetOrCreateAsync<TItem>(string key, 
            Func<Task<TItem>> factory,
            TimeSpan? slidingExpiration = null);
        void Remove(string key);
        void Set(string key, object value, TimeSpan? slidingExpiration = null);
        object Get(string key);
    }
}
