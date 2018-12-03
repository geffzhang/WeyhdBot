using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Senparc.Weixin.MP.AdvancedAPIs;

namespace WeyhdBot.Wechat.Client
{
    public class WechatClient : IWechatClient
    {
        public async Task PostTextMessage(string appId, string openId, string msg)
        {
            await CustomApi.SendTextAsync(appId,openId,msg);
        }
    }
}
