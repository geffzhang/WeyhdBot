using Newtonsoft.Json;
using System;

namespace WeyhdBot.Core.Wechat
{
    [Serializable]
    public class WechatArticle
    {
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("picurl")]
        public string PicUrl { get; set; }
    }
}
