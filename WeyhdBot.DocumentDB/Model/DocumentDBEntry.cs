using Newtonsoft.Json;

namespace WeyhdBot.DocumentDB.Model
{
    /// <summary>
    /// Wraps a DocumentDB Document
    /// </summary>
    public class DocumentDBEntry<T>
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public T Data { get; set; }

        public DocumentDBEntry(string id, T data)
        {
            Id = id;
            Data = data;
        }
    }
}
