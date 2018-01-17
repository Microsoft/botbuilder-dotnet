﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{


    public interface IStorage
    {
        /// <summary>
        /// Read StoreItems from storage
        /// </summary>
        /// <param name="keys">keys of the storeItems to read</param>
        /// <returns>StoreItem dictionary</returns>
        Task<StoreItems> Read(params string[] keys);

        /// <summary>
        /// Write StoreItems to storage
        /// </summary>
        /// <param name="changes"></param>
        Task Write(StoreItems changes);

        /// <summary>
        /// Delete StoreItems from storage
        /// </summary>
        /// <param name="keys">keys of the storeItems to delete</param>
        Task Delete(params string[] keys);
    }

    public class StoreItem : FlexObject
    {
        private static JsonSerializerSettings serializationSettings = new JsonSerializerSettings()
        {
            // we use all so that we get typed roundtrip out of storage, but we don't use validation because we don't know what types are valid
            TypeNameHandling = TypeNameHandling.All
        };

        /// <summary>
        /// eTag for concurrency
        /// </summary>
        public string eTag { get; set; }

        public T ToObject<T>()
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(this, serializationSettings), serializationSettings);
        }
    }

    public class StoreItems : FlexObject
    {
        public T Get<T>(string name)
        {
            if (this.TryGetValue(name, out dynamic value) && value != null)
                return value.ToObject<T>();
            return default(T);
        }
    }

    public class StoreItems<StoreItemT> : StoreItems
        where StoreItemT : StoreItem
    {
    }

    public interface IStorageSettings
    {
        bool OptimizeWrites { get; set; }
    }


    public static class StorageExtensions
    {

        /// <summary>
        /// Storage extension to Read as strong typed StoreItem objects
        /// </summary>
        /// <typeparam name="StoreItemT"></typeparam>
        /// <param name="storage"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static async Task<StoreItems<StoreItemT>> Read<StoreItemT>(this IStorage storage, params string[] keys)
            where StoreItemT : StoreItem
        {
            var storeItems = await storage.Read(keys).ConfigureAwait(false);
            var newResults = new StoreItems<StoreItemT>();
            foreach (var kv in storeItems)
                newResults[kv.Key] = kv.Value as StoreItemT;
            return newResults;
        }
    }
}
