using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace WeyhdBot.Dispatch
{
    /// <summary>
    /// Abstracts away message processing for logging and/or additional dispatching requests
    /// </summary>
    public interface IMessageDispatcher
    {
        Task DispatchAsync(ITurnContext context, IMessageActivity activity, CancellationToken cancellationToken = default);
    }
}
