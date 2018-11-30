using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WeyhdBot.Wechat.Client
{
    public interface IWechatClient
    {
        /// <summary>
        /// 通过公众号发送微信文本消息
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        Task PostTextMessage(string appId, string openId, string msg);
    }
}
