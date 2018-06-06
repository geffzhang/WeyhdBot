using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace WeyhdBot.Core.Wechat
{
    [Serializable]
    public class MenuItem
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("key")]
        public string Key { get; set; }
        [JsonProperty("sub_button")]
        IList<MenuItem> SubItems { get; set; }

        public static class ItemType
        {
            public static string Click = "click";
        }
    }
}
