using Newtonsoft.Json;
using System;

namespace WeyhdBot.Core.Wechat
{
    [Serializable]
    public class WechatAccessToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonIgnore]
        public DateTime IssueTime { get; set; }
    }
}
