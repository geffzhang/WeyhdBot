using Newtonsoft.Json;
using System;

namespace WeyhdBot.Core.Wechat
{
    [Serializable]
    public class WechatGenericResponse
    {
        [JsonProperty("errcode")]
        public int ErrorCode { get; set; }
        [JsonProperty("errmsg")]
        public string ErrorMessage { get; set; }
    }
}
