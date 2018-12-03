using CSRedis;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Builder.Redis.Storage
{
    public class RedisStorage : IStorage
    {
        public const string DateFormat = "MM-dd-yyyy";

        private static readonly JsonSerializer JsonSerializer = new JsonSerializer() { TypeNameHandling = TypeNameHandling.All };

        // Options for the Redis storage component.
        private readonly RedisStorageOptions redisStorageOptions;

        private CSRedisClient redisClient;
        private static string prefix = String.Empty;

        public RedisStorage(RedisStorageOptions redisStorageOptions)
        {
            if (redisStorageOptions == null)
            {
                throw new ArgumentNullException(nameof(redisStorageOptions), "Redis storage options is required.");
            }
            if(redisStorageOptions.RedisConnectionString == null)
            {
                throw new ArgumentNullException(nameof(redisStorageOptions.RedisConnectionString), "RedisConnectionString is required.");
            }
            if(redisStorageOptions.StorageName == null)
            {
                throw new ArgumentNullException(nameof(redisStorageOptions.StorageName), "StorageName is required.");
            }
            this.redisStorageOptions = redisStorageOptions;
            CreateClient();
        }

        public async Task DeleteAsync(string[] keys, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (keys == null || keys.Length == 0)
            {
                return;
            }

            var ks = await redisClient.KeysAsync($"*{redisStorageOptions.StorageName}*").ConfigureAwait(false);

            foreach (var k in ks)
            {
                var items = await redisClient.SMembersAsync<DocumentItem>(k.Replace(prefix, "")).ConfigureAwait(false);
                var documentItems = items
                    .Where(i => keys.Contains(i.RealId))
                    .ToArray();
                await redisClient.SRemAsync(k.Replace(prefix, ""), documentItems).ConfigureAwait(false);
            }
        }

        public async Task<IDictionary<string, object>> ReadAsync(string[] keys, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (keys == null || keys.Length == 0)
            {
                // No keys passed in, no result to return.
                return new Dictionary<string, object>();
            }

            var storeItems = new Dictionary<string, object>(keys.Length);
            var ks = await redisClient.KeysAsync($"*{redisStorageOptions.StorageName}*").ConfigureAwait(false);

            foreach (var k in ks)
            {
                var items = await redisClient.SMembersAsync<DocumentItem>(k.Replace(prefix, "")).ConfigureAwait(false);
                foreach (var key in keys)
                {
                    var doc = items
                        .OrderByDescending(i => i.Timestamp)
                        .FirstOrDefault(i => key.Equals(i.RealId) && i.Document != null);
                    if (doc != null)
                    {
                        storeItems.Add(doc.RealId, doc.Document.ToObject<object>(JsonSerializer));
                    }
                }
            }
            return storeItems;
        }

        public async Task WriteAsync(IDictionary<string, object> changes, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (changes == null || changes.Count == 0)
            {
                return;
            }
            var k = redisStorageOptions.StorageName + "-" + DateTime.Now.ToString(DateFormat);

            var documentItems = new List<DocumentItem>();
            foreach (var change in changes)
            {
                var newValue = change.Value;
                var newState = JObject.FromObject(newValue, JsonSerializer);
                documentItems.Add(new DocumentItem
                {
                    RealId = change.Key,
                    Document = newState,
                    Timestamp = DateTime.Now.ToUniversalTime()
                });
            }
            await redisClient.SAddAsync(
                    k,
                    documentItems.ToArray())
                    .ConfigureAwait(false);
        }

        /// <summary>
        /// Creates the Redis client.
        /// </summary>
        private void CreateClient()
        {
            try
            {
                redisClient = RedisHelper.Instance;
            }
            catch
            {
                RedisHelper.Initialization(new CSRedisClient(redisStorageOptions.RedisConnectionString));

                prefix = redisStorageOptions.RedisConnectionString.Split(',').First(x => x.StartsWith("prefix=")).Split('=')[1];
                redisClient = RedisHelper.Instance;
            }
            
        }
    }
}
