using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WeyhdBot.MongoDb.Model
{
    public class MongoDBEntry<T>
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public T Data { get; set; }

        public MongoDBEntry(string id, T data)
        {
            Id = id;
            Data = data;
        }
        public MongoDBEntry()
        {
        }
    }
}
