using System;
using System.Threading.Tasks;
using WeyhdBot.Dispatch;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using WeyhdBot.Core.Connector;

namespace WeyhdBot.Extensions
{
    public static class DialogContextExtensions
    {
        public static bool IsWechat(this ITurnContext context)
        {
            var channelData = context.GetChannelData();
            return "wechat".Equals(channelData?.Subchannel, StringComparison.InvariantCultureIgnoreCase);
        }

        public static string GetChannelUserId(this ITurnContext context)
            => context.GetChannelData()?.UserId;

        public static ChannelData GetChannelData(this ITurnContext context)
        {
            ChannelData channelData = null;
            context.Activity.TryGetChannelData(out channelData);
            return channelData;
        }

        public static Activity MakeMessage(this ITurnContext context, string message)
        {
            var activity = new Activity();
            activity.Type = ActivityTypes.Message;
            activity.Text = message;
            return activity;
         }
      
    }
}