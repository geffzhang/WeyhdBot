using Newtonsoft.Json;
using System;

namespace WeyhdBot.Core.Wechat
{
    [Serializable]
    public class WechatMediaUploadResponse
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("media_id")]
        public string MediaId { get; set; }
        [JsonProperty("created_at")]
        public int CreatedAt { get; set; }
        //{"type":"TYPE","media_id":"MEDIA_ID","created_at":123456789}
        [JsonProperty("errcode")]
        public int ErrorCode { get; set; }
        [JsonProperty("errmsg")]
        public string ErrorMessage { get; set; }
    }
}
