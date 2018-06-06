using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using System.Threading.Tasks;

namespace WeyhdBot.Dispatch
{
    /// <summary>
    /// Abstracts away message processing for logging and/or additional dispatching requests
    /// </summary>
    public interface IMessageDispatcher
    {
        Task DispatchAsync(ITurnContext context, IMessageActivity activity);
    }
}
