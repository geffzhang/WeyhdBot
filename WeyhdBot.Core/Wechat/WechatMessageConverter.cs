using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WeyhdBot.Core.Wechat
{
    public class WechatMessageConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
            => objectType.Equals(typeof(WechatMessage));

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var props = jsonObject.Properties();

            var message = new WechatMessage();
            message.ToUserName = (string)props.FirstOrDefault(p => p.Name.Equals("touser", StringComparison.InvariantCultureIgnoreCase))?.Value;
            message.MessageType = (string)props.FirstOrDefault(p => p.Name.Equals("msgtype", StringComparison.InvariantCultureIgnoreCase))?.Value;

            if (!string.IsNullOrWhiteSpace(message.MessageType))
            {
                var contentObject = (JObject)props.FirstOrDefault(p => p.Name.Equals(message.MessageType, StringComparison.InvariantCultureIgnoreCase)).Value;
                switch (message.MessageType)
                {
                    case WechatMessageTypes.TEXT:
                        message.Content = (string)contentObject.GetValue("content");
                        break;
                    case WechatMessageTypes.RICH_MEDIA:
                        message.Articles = contentObject.GetValue("articles").ToObject<IEnumerable<WechatArticle>>();
                        break;
                    case WechatMessageTypes.IMAGE:
                    case WechatMessageTypes.VOICE:
                        message.MediaId = (string)contentObject.GetValue("media_id");
                        break;
                    default:
                        break;
                }
            }

            return message;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var msg = value as WechatMessage;

            var obj = new JObject();
            obj.Add("touser", msg.ToUserName);
            obj.Add("msgtype", msg.MessageType);

            var contentObject = new JObject();

            switch (msg.MessageType)
            {
                case WechatMessageTypes.TEXT:
                    contentObject.Add("content", msg.Content);
                    break;
                case WechatMessageTypes.IMAGE:
                case WechatMessageTypes.VOICE:
                    contentObject.Add("media_id", msg.MediaId);
                    break;
                case WechatMessageTypes.RICH_MEDIA:
                    contentObject.Add("articles", JToken.FromObject(msg.Articles ?? Enumerable.Empty<WechatArticle>()));
                    break;
                default:
                    throw new ArgumentException($"Unsupported Message Type: {msg.MessageType}");
            }

            obj.Add(msg.MessageType, contentObject);
            obj.WriteTo(writer);
        }
    }
}
