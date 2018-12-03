using Microsoft.Bot.Connector.DirectLine;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WeyhdBot.Core.Connector;
using WeyhdBot.Wechat.Options;

namespace WeyhdBot.Wechat.Connector
{
    public class DirectLineConnector : IDirectLineConnector
    {
        private readonly DirectLineConnectorOptions options;
        private readonly DirectLineClient _directLineClient;

        public DirectLineConnector(IOptions<DirectLineConnectorOptions> options)
        {
            this.options = options?.Value;
            _directLineClient = BuildClientFromOptions(this.options);
        }

        public async Task<Conversation> StartConversationAsync()
            => await _directLineClient?.Conversations.StartConversationAsync();

        public async Task<Conversation> GenerateTokenAsync()
            => await _directLineClient.Tokens.GenerateTokenForNewConversationAsync();

        public async Task<Conversation> RefreshTokenAsync()
            => await _directLineClient.Tokens.RefreshTokenAsync();

        public async Task PostAsync(string conversationId, string message, string userId = null, string subchannel = null)
        {
            Activity activity = new Activity
            {
                From = new ChannelAccount(userId ?? options.BotId),
                Text = message,
                Type = ActivityTypes.Message,
                ChannelData = new ChannelData
                {
                    Subchannel = subchannel,
                    UserId = userId
                }
            };

            await PostActivityAsync(conversationId, activity);
        }

        public async Task JoinConversationAsync(string conversationId, string userId, string subchannel = null)
        {
            Activity activity = new Activity
            {
                From = new ChannelAccount(userId),
                Type = ActivityTypes.ConversationUpdate,
                ChannelData = new ChannelData
                {
                    Subchannel = subchannel,
                    UserId = userId
                }
            };

            await PostActivityAsync(conversationId, activity);
        }

        public async Task PostActivityAsync(string conversationId, Activity activity)
            => await _directLineClient?.Conversations.PostActivityAsync(conversationId, activity);

        private Microsoft.Bot.Connector.DirectLine.DirectLineClient BuildClientFromOptions(DirectLineConnectorOptions options)
        {
            var directLineClient = new DirectLineClient(options.BotSecret);
            return directLineClient;
        }
    }
}
