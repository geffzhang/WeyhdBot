using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Builder.Redis.Storage
{
    public class RedisStorageOptions
    {
        public string StorageName { get; set; }

        /// <summary>
        /// "127.0.0.1:6379,pass=123,defaultDatabase=13,poolsize=50,ssl=false,writeBuffer=10240,prefix=key前辍\"
        /// </summary>
        public string RedisConnectionString { get; set; }
    }
}
