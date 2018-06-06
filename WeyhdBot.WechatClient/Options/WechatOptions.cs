using System;
using System.Collections.Generic;
using System.Text;

namespace WeyhdBot.WechatClient
{
    [Serializable]
    public class WechatOptions
    {
        public string AppId { get; set; }
        public string AppSecret { get; set; }
        public string TokenURI { get; set; }
        public string CustomerEndpoint { get; set; }
        public string MediaUploadEndpoint { get; set; }
        public string MenuUploadEndpoint { get; set; }
        public bool UpdateMenuOnRun { get; set; }
        public string DefaultMenu { get; set; }
    }
}
