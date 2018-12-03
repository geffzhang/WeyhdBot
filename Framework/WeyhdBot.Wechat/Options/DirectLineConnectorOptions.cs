using System;
using System.Collections.Generic;
using System.Text;

namespace WeyhdBot.Wechat.Options
{
    [Serializable]
    public class DirectLineConnectorOptions
    {
        public string BotSecret { get; set; }
        public string BotId { get; set; }
    }
}
