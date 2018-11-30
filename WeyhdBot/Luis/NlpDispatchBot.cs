using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using WeyhdBot.Dispatch;
using WeyhdBot.Extensions;

namespace WeyhdBot.Luis
{
    /// <summary>
    /// Represents a bot that processes incoming activities.
    /// For each interaction from the user, an instance of this class is called.
    /// This is a Transient lifetime service. Transient lifetime services are created
    /// each time they're requested. For each Activity received, a new instance of this
    /// class is created. Objects that are expensive to construct, or have a lifetime
    /// beyond the single Turn, should be carefully managed.
    /// </summary>
    public class NlpDispatchBot : IBot
    {
        private const string WelcomeText = "This bot will introduce you to Dispatch for QnA Maker and LUIS. Type a greeting, or a question about the weather to get started";

        /// <summary>
        /// Key in the Bot config (.bot file) for the Home Automation Luis instance.
        /// </summary>
        private const string MyLuisLuisKey = "myluis";

        /// <summary>
        /// Key in the Bot config (.bot file) for the Dispatch.
        /// </summary>
        private const string DispatchKey = "nlp-with-dispatchDispatch";

        /// <summary>
        /// Key in the Bot config (.bot file) for the QnaMaker instance.
        /// In the .bot file, multiple instances of QnaMaker can be configured.
        /// </summary>
        private const string QnAMakerKey = "sample-qna";

        /// <summary>
        /// Services configured from the ".bot" file.
        /// </summary>
        private readonly BotServices _services;

        private readonly IMessageDispatcher _messageDispatcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="NlpDispatchBot"/> class.
        /// </summary>
        /// <param name="services">Services configured from the ".bot" file.</param>
        /// <param name="messageDispatcher">messageDispatcher</param>
        public NlpDispatchBot(BotServices services, IMessageDispatcher messageDispatcher)
        {
            _messageDispatcher = messageDispatcher;
            _services = services ?? throw new System.ArgumentNullException(nameof(services));

            if (!_services.QnAServices.ContainsKey(QnAMakerKey))
            {
                throw new System.ArgumentException($"Invalid configuration. Please check your '.bot' file for a QnA service named '{DispatchKey}'.");
            }

            if (!_services.LuisServices.ContainsKey(MyLuisLuisKey))
            {
                throw new System.ArgumentException($"Invalid configuration. Please check your '.bot' file for a Luis service named '{MyLuisLuisKey}'.");
            }
        }

        /// <summary>
        /// Every conversation turn for our NLP Dispatch Bot will call this method.
        /// There are no dialogs used, since it's "single turn" processing, meaning a single
        /// request and response, with no stateful conversation.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type == ActivityTypes.Message && !turnContext.Responded)
            {
                // Get the intent recognition result
                var recognizerResult = await _services.LuisServices[DispatchKey].RecognizeAsync(turnContext, cancellationToken);
                var topIntent = recognizerResult?.GetTopScoringIntent();

                if (topIntent == null)
                {
                    var msg = "Unable to get the top intent.";
                    await SendActivityAsync(turnContext, msg, cancellationToken);
                }
                else
                {
                    await DispatchToTopIntentAsync(turnContext, topIntent, cancellationToken);
                }
            }
            else if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                // Send a welcome message to the user and tell them what actions they may perform to use this bot
                if (turnContext.Activity.MembersAdded != null)
                {
                    await SendWelcomeMessageAsync(turnContext, cancellationToken);
                }
            }
            else
            {
                var msg = $"{turnContext.Activity.Type} event detected";
                await SendActivityAsync(turnContext, msg, cancellationToken);
            }
        }

        /// <summary>
        /// On a conversation update activity sent to the bot, the bot will
        /// send a message to the any new user(s) that were added.
        /// </summary>
        /// <param name="turnContext">Provides the <see cref="ITurnContext"/> for the turn of the bot.</param>
        /// <param name="cancellationToken" >(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>>A <see cref="Task"/> representing the operation result of the Turn operation.</returns>
        private async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var msg = $"Welcome to Dispatch bot {member.Name}. {WelcomeText}";
                    await SendActivityAsync(turnContext, msg, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Depending on the intent from Dispatch, routes to the right LUIS model or QnA service.
        /// </summary>
        private async Task DispatchToTopIntentAsync(ITurnContext context, (string intent, double score)? topIntent, CancellationToken cancellationToken = default(CancellationToken))
        {
            const string homeAutomationDispatchKey = "l_Home_Automation";
            const string noneDispatchKey = "None";
            const string qnaDispatchKey = "q_sample-qna";

            switch (topIntent.Value.intent)
            {
                case homeAutomationDispatchKey:
                    await DispatchToLuisModelAsync(context, MyLuisLuisKey);

                    // Here, you can add code for calling the hypothetical home automation service, passing in any entity information that you need
                    break;
                case noneDispatchKey:
                // You can provide logic here to handle the known None intent (none of the above).
                // In this example we fall through to the QnA intent.
                case qnaDispatchKey:
                    await DispatchToQnAMakerAsync(context, QnAMakerKey);
                    break;

                default:
                    // The intent didn't match any case, so just display the recognition results.
                    var msg = $"Dispatch intent: {topIntent.Value.intent} ({topIntent.Value.score}).";
                    await SendActivityAsync(context, msg, cancellationToken);
                    break;
            }
        }

        /// <summary>
        /// Dispatches the turn to the request QnAMaker app.
        /// </summary>
        private async Task DispatchToQnAMakerAsync(ITurnContext context, string appName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!string.IsNullOrEmpty(context.Activity.Text))
            {
                var results = await _services.QnAServices[appName].GetAnswersAsync(context);
                if (results.Any())
                {
                    var msg = results.First().Answer;
                    await SendActivityAsync(context, msg, cancellationToken);
                }
                else
                {
                    var msg = $"Couldn't find an answer in the {appName}.";
                    await SendActivityAsync(context, msg, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Dispatches the turn to the requested LUIS model.
        /// </summary>
        private async Task DispatchToLuisModelAsync(ITurnContext context, string appName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var msg = $"Sending your request to the {appName} system ...";
            await SendActivityAsync(context, msg, cancellationToken);

            var result = await _services.LuisServices[appName].RecognizeAsync(context, cancellationToken);

            msg = $"Intents detected by the {appName} app:\n\n{string.Join("\n\n", result.Intents)}";
            await SendActivityAsync(context, msg, cancellationToken);

            if (result.Entities.Count > 0)
            {
                msg = $"The following entities were found in the message:\n\n{string.Join("\n\n", result.Entities)}";
                await SendActivityAsync(context, msg, cancellationToken);
            }
        }

        private async Task SendActivityAsync(ITurnContext context, string message, CancellationToken cancellationToken = default(CancellationToken))
        {
            var activity = context.MakeMessage(message);
            await _messageDispatcher.DispatchAsync(context, activity, cancellationToken);
        }
    }
}
