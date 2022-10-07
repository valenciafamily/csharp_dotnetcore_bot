// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;


namespace Microsoft.BotBuilderSamples
{
    // Represents a bot that processes incoming activities.
    // For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
    // This is a Transient lifetime service. Transient lifetime services are created
    // each time they're requested. For each Activity received, a new instance of this
    // class is created. Objects that are expensive to construct, or have a lifetime
    // beyond the single turn, should be carefully managed.
    // For example, the "MemoryStorage" object and associated
    // IStatePropertyAccessor{T} object are created with a singleton lifetime.
    public class WelcomeUserBot : ActivityHandler
    {
        // Messages sent to the user.
        private const string WelcomeMessage = "This is a simple Welcome Bot sample. This bot will introduce you " +
                                                "to welcoming and greeting users. You can say 'intro' to see the " +
                                                "introduction card. If you are running this bot in the Bot Framework " +
                                                "Emulator, press the 'Start Over' button to simulate user joining " +
                                                "a bot or a channel";

        //private const string InfoMessage = "You are seeing this message because the bot received at least one " +
        //                                    "'ConversationUpdate' event, indicating you (and possibly others) " +
        //                                    "joined the conversation. If you are using the emulator, pressing " +
        //                                    "the 'Start Over' button to trigger this event again. The specifics " +
        //                                    "of the 'ConversationUpdate' event depends on the channel. You can " +
        //                                    "read more information at: " +
        //                                    "https://aka.ms/about-botframework-welcome-user";

        //private const string LocaleMessage = "You can use the activity's 'GetLocale()' method to welcome the user " +
        //                                     "using the locale received from the channel. " + 
        //                                     "If you are using the Emulator, you can set this value in Settings.";

        //private const string PatternMessage = "It is a good pattern to use this event to send general greeting" +
        //                                      "to user, explaining what your bot can do. In this example, the bot " +
        //                                      "handles 'hello', 'hi', 'help' and 'intro'. Try it now, type 'hi'";

        private readonly BotState _userState;

        // Initializes a new instance of the "WelcomeUserBot" class.
        public WelcomeUserBot(UserState userState)
        {
            _userState = userState;
        }

        // Greet when users are added to the conversation.
        // Note that all channels do not send the conversation update activity.
        // If you find that this bot works in the emulator, but does not in
        // another channel the reason is most likely that the channel does not
        // send this activity.
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    //await turnContext.SendActivityAsync($"Hi there - {member.Name}. {WelcomeMessage}", cancellationToken: cancellationToken);
                    //await turnContext.SendActivityAsync(InfoMessage, cancellationToken: cancellationToken);
                    //await turnContext.SendActivityAsync($"{LocaleMessage} Current locale is '{turnContext.Activity.GetLocale()}'.", cancellationToken: cancellationToken);
                    //await turnContext.SendActivityAsync(PatternMessage, cancellationToken: cancellationToken);


               
                    var welcomeUserStateAccessor = _userState.CreateProperty<WelcomeUserState>(nameof(WelcomeUserState));
                    var didBotWelcomeUser = await welcomeUserStateAccessor.GetAsync(turnContext, () => new WelcomeUserState(), cancellationToken);




                    if (didBotWelcomeUser.DidBotWelcomeUser == false)
                    {
                        didBotWelcomeUser.DidBotWelcomeUser = true;
                          await SendIntroQuestionAsync(turnContext, cancellationToken);
                        // the channel should sends the user name in the 'From' object
                        // var userName = turnContext.Activity.From.Name;
                        //   await SendIntroQuestionAsync(turnContext, cancellationToken);
                        //await turnContext.SendActivityAsync("You are seeing this message because this was your first message ever to this bot.", cancellationToken: cancellationToken);
                        //await turnContext.SendActivityAsync($"It is a good practice to welcome the user and provide personal greeting. For example, welcome {userName}.", cancellationToken: cancellationToken);

                        //var welcomeCard = WelcomeCardAdaptiveCardAttachment();
                        //var response = MessageFactory.Attachment(welcomeCard);
                        //await turnContext.SendActivityAsync(response, cancellationToken);

                    }

                }
            }
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {

            // This example hardcodes specific utterances. You should use LUIS or QnA for more advance language understanding.

            var text = turnContext.Activity.Text.ToLowerInvariant();
            switch (text)
            {
                case "hello":
                case "hi":
                    await turnContext.SendActivityAsync($"You said {text}.", cancellationToken: cancellationToken);
                    break;
                case "intro":
                case "help":
                    await SendIntroQuestionAsync(turnContext, cancellationToken);
                    break;
                case "expenses":
                    await SendExpenseQuestionAsync(turnContext, cancellationToken);
                    break;
                case "create expense":
                    //  await SendCreateExpensesCardAsync(turnContext, cancellationToken);
                    var createExpense = CreateExpenseAdaptiveCardAttachment();
                    var createExpenseresponse = MessageFactory.Attachment(createExpense);
                    await turnContext.SendActivityAsync(createExpenseresponse, cancellationToken);

                    break;
                case "edit expense":
                    var editExpense = EditExpenseAdaptiveCardAttachment();
                    var editExpensereponse = MessageFactory.Attachment(editExpense);
                    await turnContext.SendActivityAsync(editExpensereponse, cancellationToken);
                    break;

                case "timesheet":
                    await SendTimesheetQuestionAsync(turnContext, cancellationToken);
                    break;
                case "contact support":
                    var contactSupport = ContactSupportAdaptiveCardAttachment();
                    var contactSupportreponse = MessageFactory.Attachment(contactSupport);
                    await turnContext.SendActivityAsync(contactSupportreponse, cancellationToken);
                    break;
                    ;
                default:
                    await turnContext.SendActivityAsync(WelcomeMessage, cancellationToken: cancellationToken);
                    break;
            }


                        // Save any state changes.
                        await _userState.SaveChangesAsync(turnContext, cancellationToken: cancellationToken);
        }

        private static async Task SendIntroCardAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var card = new HeroCard
            {
                Title = "Welcome to Bot Framework!",
                Text = @"Welcome to Welcome Users bot sample! This Introduction card
                         is a great way to introduce your Bot to the user and suggest
                         some things to get them started. We use this opportunity to
                         recommend a few next steps for learning more creating and deploying bots.",
                Images = new List<CardImage>() { new CardImage("https://aka.ms/bf-welcome-card-image") },
                Buttons = new List<CardAction>()
                {
                    new CardAction(ActionTypes.OpenUrl, "Get an overview", null, "Get an overview", "Get an overview", "https://docs.microsoft.com/en-us/azure/bot-service/?view=azure-bot-service-4.0"),
                    new CardAction(ActionTypes.OpenUrl, "Ask a question", null, "Ask a question", "Ask a question", "https://stackoverflow.com/questions/tagged/botframework"),
                    new CardAction(ActionTypes.OpenUrl, "Learn how to deploy", null, "Learn how to deploy", "Learn how to deploy", "https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-howto-deploy-azure?view=azure-bot-service-4.0"),
                    new CardAction(ActionTypes.MessageBack, "", null, "Learn how to deploy", "Learn how to deploy", "https://photos.google.com/share/AF1QipPQPs6NpeLjOZEunG9rrDdQqpiEoLxMmDWshpuU_5QXp8WD9Nm7qhqIg8trAO8yAw/photo/AF1QipO_4VuCK5cjhrezn56qHTx9uBCs0fmzO7zayNuz?key=Mk9xUWp5dVpicFZueEh2aU9mY09mVTF5M1Y2M2dn"),
                }
            };

            var response = MessageFactory.Attachment(card.ToAttachment());
            await turnContext.SendActivityAsync(response, cancellationToken);
        }

        // Load attachment from file.

        private static Attachment WelcomeCardAdaptiveCardAttachment()
        {
            //combine path for cross platform support

            string[] paths = { ".", "Cards", "welcomeCard.json" };
            var fullPath = Path.Combine(paths);
            var pt = fullPath;
            var adaptiveCard = File.ReadAllText(fullPath);
            var p = adaptiveCard;
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };


        }

        private static Attachment CreateExpenseAdaptiveCardAttachment()
        {
            //combine path for cross platform support

            string[] paths = { ".", "Cards", "createExpense.json" };
            var fullPath = Path.Combine(paths);
            var pt = fullPath;
            var adaptiveCard = File.ReadAllText(fullPath);
            var p = adaptiveCard;
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };


        }

        private static Attachment ContactSupportAdaptiveCardAttachment()
        {
            //combine path for cross platform support

            string[] paths = { ".", "Cards", "contactSupport.json" };
            var fullPath = Path.Combine(paths);
            var pt = fullPath;
            var adaptiveCard = File.ReadAllText(fullPath);
            var p = adaptiveCard;
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };


        }

        private static Attachment EditExpenseAdaptiveCardAttachment()
        {
            //combine path for cross platform support

            string[] paths = { ".", "Cards", "editExpense.json" };
            var fullPath = Path.Combine(paths);
            var pt = fullPath;
            var adaptiveCard = File.ReadAllText(fullPath);
            var p = adaptiveCard;
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };


        }



        private static async Task SendIntroQuestionAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var card = new HeroCard
            {
                Title = "Welcome to Atlas Solution!",
                Text = @"Welcome! How can I help you? We have options listed for you!",
                Images = new List<CardImage>() { new CardImage("https://lh3.googleusercontent.com/pw/AL9nZEVdMJT_WpDlJSD0VrzEx-vEtN36NYuKxobObkeTHkxKm52oIOmYVeRLMTZKSW0AgnHwZ_mF6QCDifr_lJZMZAIB4QYBxm09yN1D5EbbQIcvX_XqDR4VHWPvzibJVoHtM6Q_dNXY2YdKgL-QpDAxpaX1tQ=w497-h409-no?authuser=1") },
                Buttons = new List<CardAction>()
                {

                   new CardAction(ActionTypes.MessageBack, "Expenses", null, "Expenses", "Expenses", "timesheet"),
                    new CardAction(ActionTypes.MessageBack, "Timesheet", null, "Timesheet", "Timesheet", "Timesheet"),
                    new CardAction(ActionTypes.OpenUrl, "Preference", null, "Learn how to deploy", "Learn how to deploy", "https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-howto-deploy-azure?view=azure-bot-service-4.0"),
                    new CardAction(ActionTypes.OpenUrl, "Other", null, "Learn how to deploy", "Learn how to deploy", "https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-howto-deploy-azure?view=azure-bot-service-4.0"),
                },

            };

            var response = MessageFactory.Attachment(card.ToAttachment());

            await turnContext.SendActivityAsync(response, cancellationToken);

        }

        private static async Task SendExpenseQuestionAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var card = new HeroCard
            {
                Title = "Expense Page",
                Text = @"How can I help you? We have options listed for you!",
                Buttons = new List<CardAction>()                {

                    new CardAction(ActionTypes.MessageBack, "Create Expense", null, "Create Expense", "Create Expense", ""),
                    new CardAction(ActionTypes.MessageBack, "Edit Expense", null, "Edit Expense", "Edit Expense", ""),
                    new CardAction(ActionTypes.MessageBack, "Contact Support", null, "Contact Support", "Contact Support", ""),

                },




            };



            var response = MessageFactory.Attachment(card.ToAttachment());



            await turnContext.SendActivityAsync(response, cancellationToken);



        }


        private static async Task SendTimesheetQuestionAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var card = new HeroCard
            {
                Title = "Timesheet Page",
                Text = @"How can I help you? We have options listed for you!",
                Buttons = new List<CardAction>()                {

                    new CardAction(ActionTypes.MessageBack, "Add/Edit Time", null, "Add/Edit Time", "Add/Edit Time", ""),
                    new CardAction(ActionTypes.MessageBack, "Submit Timesheet ", null, "Submit Timesheet ", "Submit Timesheet ", ""),
                    new CardAction(ActionTypes.MessageBack, "Report Incident ", null, "Report Incident", "Report Incident", ""),
                    new CardAction(ActionTypes.MessageBack, "Contact Support", null, "Contact Support", "Contact Support", ""),

                },




            };



            var response = MessageFactory.Attachment(card.ToAttachment());



            await turnContext.SendActivityAsync(response, cancellationToken);



        }

    }
}
