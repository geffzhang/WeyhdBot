// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace WeyhdBot
{
    /// <summary>
    /// Represents a bot that processes incoming activities.
    /// For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
    /// This is a Transient lifetime service.  Transient lifetime services are created
    /// each time they're requested. For each Activity received, a new instance of this
    /// class is created. Objects that are expensive to construct, or have a lifetime
    /// beyond the single turn, should be carefully managed.
    /// For example, the <see cref="MemoryStorage"/> object and associated
    /// <see cref="IStatePropertyAccessor{T}"/> object are created with a singleton lifetime.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
    public class EchoWithCounterBot : IBot
    {
        private readonly EchoBotAccessors _accessors;
        private readonly ILogger _logger;
        private DialogSet _dialogs;

        private LuisRecognizer Recognizer { get; } = null;

        private QnAMaker QnA { get; } = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="EchoWithCounterBot"/> class.
        /// </summary>
        /// <param name="accessors">A class containing <see cref="IStatePropertyAccessor{T}"/> used to manage state.</param>
        /// <param name="luis">LuisRecognizer.</param>
        /// <param name="qna">QnAMaker.</param>
        /// <param name="loggerFactory">A <see cref="ILoggerFactory"/> that is hooked to the Azure App Service provider.</param>
        /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1#windows-eventlog-provider"/>
        public EchoWithCounterBot(EchoBotAccessors accessors, LuisRecognizer luis, QnAMaker qna, ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<EchoWithCounterBot>();
            _logger.LogTrace("EchoBot turn start.");

            // The incoming luis variable is the LUIS Recognizer we added above.
            this.Recognizer = luis ?? throw new System.ArgumentNullException(nameof(luis));

            // The incoming QnA variable is the QnAMaker we added above.
            this.QnA = qna ?? throw new System.ArgumentNullException(nameof(qna));

            _accessors = accessors ?? throw new System.ArgumentNullException(nameof(accessors));

            // The DialogSet needs a DialogState accessor, it will call it when it has a turn context.
            _dialogs = new DialogSet(accessors.ConversationDialogState);

            // This array defines how the Waterfall will execute.
            var waterfallSteps = new WaterfallStep[]
            {
                NameStepAsync,
                NameConfirmStepAsync,
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            _dialogs.Add(new WaterfallDialog("details", waterfallSteps));
            _dialogs.Add(new TextPrompt("name"));
        }

        /// <summary>
        /// Every conversation turn for our Echo Bot will call this method.
        /// There are no dialogs used, since it's "single turn" processing, meaning a single
        /// request and response.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        /// <seealso cref="BotStateSet"/>
        /// <seealso cref="ConversationState"/>
        /// <seealso cref="IMiddleware"/>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Handle Message activity type, which is the main activity
            // type for shown within a conversational interface
            // Message activities may contain text, speech, interactive cards,
            // and binary or unknown attachments.
            // see https://aka.ms/about-bot-activity-message to learn more about
            // the message and other activity types
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                // Check LUIS model
                var recognizerResult = await this.Recognizer.RecognizeAsync(turnContext, cancellationToken);
                var topIntent = recognizerResult?.GetTopScoringIntent();

                // Get the Intent as a string
                string strIntent = (topIntent != null) ? topIntent.Value.intent : string.Empty;

                // Get the IntentScore as a double
                double dblIntentScore = (topIntent != null) ? topIntent.Value.score : 0.0;

                // Only proceed with LUIS if there is an Intent
                // and the score for the Intent is greater than 90
                if (strIntent != string.Empty && (dblIntentScore > 0.90))
                {
                    switch (strIntent)
                    {
                        case "None":
                            await turnContext.SendActivityAsync("Sorry, I don't understand.");
                            break;
                        case "Utilities_Help":
                            await turnContext.SendActivityAsync("<here's some help>");
                            break;
                        default:
                            // Received an intent we didn't expect, so send its name and score.
                            await turnContext.SendActivityAsync(
                                $"Intent: {topIntent.Value.intent} ({topIntent.Value.score}).");
                            break;
                    }
                }
                else
                {
                    // Get the conversation state from the turn context.
                    var state = await _accessors.CounterState.GetAsync(turnContext, () => new CounterState());

                    // Bump the turn count for this conversation.
                    state.TurnCount++;
                    if (!state.SaidHello)
                    {
                        // Handle the Greeting
                        string strMessage = $"Hello World! {System.Environment.NewLine}";
                        strMessage += "Talk to me and I will repeat it back!";
                        await turnContext.SendActivityAsync(strMessage);

                        // Set SaidHello
                        state.SaidHello = true;
                    }
                    else
                    {
                        // Get the user state from the turn context.
                        var user = await _accessors.UserProfile.GetAsync(turnContext, () => new UserProfile());
                        if (user.Name == null)
                        {
                            // Run the DialogSet - let the framework identify the current state of the dialog from
                            // the dialog stack and figure out what (if any) is the active dialog.
                            var dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
                            var results = await dialogContext.ContinueDialogAsync(cancellationToken);

                            // If the DialogTurnStatus is Empty we should start a new dialog.
                            if (results.Status == DialogTurnStatus.Empty)
                            {
                                await dialogContext.BeginDialogAsync("details", null, cancellationToken);
                            }
                        }
                        else
                        {
                            // Give QnA Maker a chance to answer
                            var answers = await this.QnA.GetAnswersAsync(turnContext);
                            if (answers.Any())
                            {
                                // If the service produced one or more answers, send the first one.
                                await turnContext.SendActivityAsync(answers[0].Answer);
                            }
                            else
                            {
                                // Echo back to the user whatever they typed.
                                var responseMessage =
                                    $"Turn {state.TurnCount}: {user.Name} you said '{turnContext.Activity.Text}'\n";
                                await turnContext.SendActivityAsync(responseMessage);
                            }
                        }

                        // Echo back to the user whatever they typed.
                        // var responseMessage = $"Turn {state.TurnCount}: You sent '{turnContext.Activity.Text}'\n";
                        // await turnContext.SendActivityAsync(responseMessage);
                    }

                    // Set the property using the accessor.
                    await _accessors.CounterState.SetAsync(turnContext, state);

                    // Save the new turn count into the conversation state.
                    await _accessors.ConversationState.SaveChangesAsync(turnContext);

                    // Save the user profile updates into the user state.
                    await _accessors.UserState.SaveChangesAsync(turnContext, false, cancellationToken);
                }
            }
            else
            {
                await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected");
            }
        }

        private static async Task<DialogTurnResult> NameStepAsync(
        WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Running a prompt here means the next WaterfallStep will be
            // run when the users response is received.
            return await stepContext.PromptAsync(
                "name", new PromptOptions { Prompt = MessageFactory.Text("What is your name?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> NameConfirmStepAsync(
            WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // We can send messages to the user at any point in the WaterfallStep.
            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text($"Hello {stepContext.Result}!"), cancellationToken);

            // Get the current profile object from user state.
            var userProfile = await _accessors.UserProfile.GetAsync(
                stepContext.Context, () => new UserProfile(), cancellationToken);

            // Update the profile.
            userProfile.Name = (string)stepContext.Result;

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog, 
            // here it is the end.
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
