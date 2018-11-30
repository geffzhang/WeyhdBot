using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using WeyhdBot.Core.Wechat;
using WeyhdBot.Extensions;

namespace WeyhdBot.Dispatch
{
    public class MessageDispatcher : IMessageDispatcher
    {
        private readonly string _wechatOutgoingURI;

        public MessageDispatcher(string wechatOutgoingUri)
        {
            _wechatOutgoingURI = wechatOutgoingUri;
        }

        public async Task DispatchAsync(ITurnContext context, IMessageActivity activity, CancellationToken cancellationToken = default)
        {
            if (context.IsWechat())
            {
                await DispatchToWechatAsync(context, activity, cancellationToken);
            }

            await context.SendActivityAsync(activity, cancellationToken);
        }

        private async Task DispatchToWechatAsync(ITurnContext context, IMessageActivity activity, CancellationToken cancellationToken = default)
        {
            // The connector could handle this conversion instead...
            var wechatMessage = ConvertMessage(context, activity);

            using (var client = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(wechatMessage), Encoding.UTF8);
                await client.PostAsync(_wechatOutgoingURI, content, cancellationToken);
            }
        }

        private WechatMessage ConvertMessage(ITurnContext context, IMessageActivity activity)
        {
            // The connector could handle this conversion instead...
            var wechatMessage = new WechatMessage
            {
                ToUserName = context.GetChannelUserId(),
            };

            var richCards = activity.Attachments?.Where(att => att.ContentType.Equals("application/vnd.microsoft.card.hero", System.StringComparison.InvariantCultureIgnoreCase));
            if (richCards?.Count() > 0)
            {
                wechatMessage.MessageType = WechatMessageTypes.RICH_MEDIA;
                wechatMessage.Articles = richCards.Select(att =>
                {
                    var richCard = att.Content as HeroCard;
                    return new WechatArticle
                    {
                        Title = richCard.Title,
                        Description = richCard.Subtitle ?? richCard.Text,
                        PicUrl = richCard.Images?.FirstOrDefault()?.Url ?? string.Empty,
                    };
                });

                return wechatMessage;
            }

            var image = activity.Attachments?.FirstOrDefault(att => att.ContentType.Contains("image"));
            if (image != null)
            {
                wechatMessage.MessageType = WechatMessageTypes.IMAGE;
                wechatMessage.MediaId = image.ContentUrl;
                return wechatMessage;
            }

            wechatMessage.MessageType = WechatMessageTypes.TEXT;
            wechatMessage.Content = activity.Text;
            return wechatMessage;
        }
    }
}
