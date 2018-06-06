using System;

namespace WeyhdBot.WechatClient
{
    [Serializable]
    public class DirectLineConnectorOptions
    {
        public string BotSecret { get; set; }
        public string BotId { get; set; }
    }
}
