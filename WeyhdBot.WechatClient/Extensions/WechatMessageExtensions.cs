
using WeyhdBot.Core.Wechat;

namespace WeyhdBot.WechatClient.Extensions
{
    public static class WechatMessageExtensions
    {
        public static WechatMessage CreateReply(this WechatMessage msg)
        {
            return new WechatMessage
            {
                MessageType = msg.MessageType,
                FromUserName = msg.ToUserName,
                ToUserName = msg.FromUserName,
                CreateTime = msg.CreateTime
            };
        }
    }
}
