using Microsoft.Bot.Connector.DirectLine;
using System.Threading.Tasks;

namespace WeyhdBot.WechatClient.Connector
{
    /// <summary>
    /// Encapsulates connection to a bot via direct line
    /// </summary>
    public interface IDirectLineConnector
    {
        /// <summary>
        /// Generate a new conversation by alerting the bot
        /// </summary>
        Task<Conversation> StartConversationAsync();

        /// <summary>
        /// Generates a new token but does not alert the bot to the start of a conversation
        /// </summary>
        Task<Conversation> GenerateTokenAsync();

        /// <summary>
        /// Refreshes a token so it can be reused until it expires
        /// </summary>
        Task<Conversation> RefreshTokenAsync();

        /// <summary>
        /// Adds a ConversationUpdate activity to allow this user to join the conversation
        /// </summary>
        Task JoinConversationAsync(string conversationId, string userId, string subchannel = null);
        /// <summary>
        /// Posts message to the bot within the active conversation
        /// </summary>
        Task PostAsync(string conversationId, string message, string userId = null, string subchannel = null);

        /// <summary>
        /// Posts message to the bot within the active conversation
        /// </summary>
        Task PostActivityAsync(string conversationId, Activity activity);
    }
}
