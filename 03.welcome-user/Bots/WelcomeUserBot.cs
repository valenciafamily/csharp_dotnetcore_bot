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
                    await SuggestedActions(turnContext, cancellationToken, "");
                    break;
                case "create expense":
                    
                    var createExpense = AdaptiveCardAttachment("createExpense.json", "");
                    var createExpenseresponse = MessageFactory.Attachment(createExpense);
                    await turnContext.SendActivityAsync(createExpenseresponse, cancellationToken);
                    await SuggestedActions(turnContext, cancellationToken, "expenses");

                    break;
                case "edit expense":
                    var editExpense = AdaptiveCardAttachment("editExpense.json", "");
                    var editExpensereponse = MessageFactory.Attachment(editExpense);
                    await turnContext.SendActivityAsync(editExpensereponse, cancellationToken);
                    await SuggestedActions(turnContext, cancellationToken, "expenses");
                    break;

                case "timesheet":
                    await SendTimesheetQuestionAsync(turnContext, cancellationToken);
                    await SuggestedActions(turnContext, cancellationToken, "");
                    break;
                case "contact support":
                    var contactSupport = AdaptiveCardAttachment("contactSupport.json", "");
                    var contactSupportreponse = MessageFactory.Attachment(contactSupport);
                    await turnContext.SendActivityAsync(contactSupportreponse, cancellationToken);
                    await SuggestedActions(turnContext, cancellationToken, "");
                    break;
                case "add/edit time":
                    var addEditTime = AdaptiveCardAttachment("addEditTimesheet.json", "timesheet");
                    var addEditTimereponse = MessageFactory.Attachment(addEditTime);
                    await turnContext.SendActivityAsync(addEditTimereponse, cancellationToken);
                    await SuggestedActions(turnContext, cancellationToken, "timesheet");
                    break;
                case "report incident":
                    var reportIncident = AdaptiveCardAttachment("reportIncident.json", "timesheet");
                    var reportIncidentreponse = MessageFactory.Attachment(reportIncident);
                    await turnContext.SendActivityAsync(reportIncidentreponse, cancellationToken);
                    await SuggestedActions(turnContext, cancellationToken, "timesheet");
                    break;
                case "submit timesheet":
                    var submitTimesheet = AdaptiveCardAttachment("submitTimesheet.json", "timesheet");
                    var submitTimesheetresponse = MessageFactory.Attachment(submitTimesheet);
                    await turnContext.SendActivityAsync(submitTimesheetresponse, cancellationToken);
                    await SuggestedActions(turnContext, cancellationToken, "timesheet");
                    break;
                case "preference":
                    await SendPreferenceQuestionAsync(turnContext, cancellationToken);
                    await SuggestedActions(turnContext, cancellationToken, "");
                    break;
                case "work preference":
                    var wotkPreference = AdaptiveCardAttachment("workPreference.json", "preference");
                    var wotkPreferenceresponse = MessageFactory.Attachment(wotkPreference);
                    await turnContext.SendActivityAsync(wotkPreferenceresponse, cancellationToken);
                    await SuggestedActions(turnContext, cancellationToken,"preference");
                    break;
                case "specialties":
                    var specialties = AdaptiveCardAttachment("specialties.json", "preference");
                    var specialtiesresponse = MessageFactory.Attachment(specialties);
                    await turnContext.SendActivityAsync(specialtiesresponse, cancellationToken);
                    await SuggestedActions(turnContext, cancellationToken, "preference");
                    break;
                case "licenses":
                    var licenses = AdaptiveCardAttachment("licenses.json", "preference");
                    var licensesresponse = MessageFactory.Attachment(licenses);
                    await turnContext.SendActivityAsync(licensesresponse, cancellationToken);
                    await SuggestedActions(turnContext, cancellationToken, "preference");
                    break;

                case "reset password":
                    var resetpassword = AdaptiveCardAttachment("resetPassword.json", "preference");
                    var resetpasswordresponse = MessageFactory.Attachment(resetpassword);
                    await turnContext.SendActivityAsync(resetpasswordresponse, cancellationToken);
                    await SuggestedActions(turnContext, cancellationToken, "preference");
                    break;

                case "edit email address":
                    var editemail = AdaptiveCardAttachment("editEmail.json", "preference");
                    var editemailresponse = MessageFactory.Attachment(editemail);
                    await turnContext.SendActivityAsync(editemailresponse, cancellationToken);
                    await SuggestedActions(turnContext, cancellationToken, "preference");
                    break;

                default:
                    await turnContext.SendActivityAsync(WelcomeMessage, cancellationToken: cancellationToken);
                    break;
            }
                        // Save any state changes.
                        await _userState.SaveChangesAsync(turnContext, cancellationToken: cancellationToken);
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

        private static Attachment AdaptiveCardAttachment(string filename, string page)
        {
            //combine path for cross platform support
            string loc = "";
            if (page == "timesheet")
            {
                loc = "Cards/Timesheet";
            }
            else if (page == "preference")
            {
                loc = "Cards/Preference";
            }
            else
            {
                loc = "Cards";
            }

            string[] paths = { ".", loc, filename };
           

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
                Text = @"Hi " + turnContext.Activity.From.Name +  ", I am JACOB How can I help you? Select any options from below or type your questions.",
                Images = new List<CardImage>() { new CardImage("https://lh3.googleusercontent.com/pw/AL9nZEVdMJT_WpDlJSD0VrzEx-vEtN36NYuKxobObkeTHkxKm52oIOmYVeRLMTZKSW0AgnHwZ_mF6QCDifr_lJZMZAIB4QYBxm09yN1D5EbbQIcvX_XqDR4VHWPvzibJVoHtM6Q_dNXY2YdKgL-QpDAxpaX1tQ=w497-h409-no?authuser=1") },
                Buttons = new List<CardAction>()
                {

                   new CardAction(ActionTypes.MessageBack, "Expenses", null, "Expenses", "Expenses", "timesheet"),
                    new CardAction(ActionTypes.MessageBack, "Timesheet", null, "Timesheet", "Timesheet", "Timesheet"),
                    new CardAction(ActionTypes.MessageBack, "Preference", null, "Preference", "Preference", "Preference"),
                    new CardAction(ActionTypes.MessageBack, "Contact Support", null, "Contact Support", "Contact Support"),
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
                    new CardAction(ActionTypes.MessageBack, "Submit Timesheet", null, "Submit Timesheet", "Submit Timesheet", ""),
                    new CardAction(ActionTypes.MessageBack, "Report Incident ", null, "Report Incident", "Report Incident", ""),
                    new CardAction(ActionTypes.MessageBack, "Contact Support", null, "Contact Support", "Contact Support", ""),

                },




            };



            var response = MessageFactory.Attachment(card.ToAttachment());



            await turnContext.SendActivityAsync(response, cancellationToken);



        }

        private static async Task SendPreferenceQuestionAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var card = new HeroCard
            {
                Title = "Peference Page",
                Text = @"How can I help you? We have options listed for you!",
                Buttons = new List<CardAction>()                {

                    new CardAction(ActionTypes.MessageBack, "Work Preference", null, "Work Preference", "Work Preference", ""),
                    new CardAction(ActionTypes.MessageBack, "Specialties", null, "Specialties", "Specialties", ""),
                    new CardAction(ActionTypes.MessageBack, "Licenses", null, "Licenses", "Licenses", ""),
                    new CardAction(ActionTypes.MessageBack, "Reset Password ", null, "Reset password", "Reset password", ""),                    
                    new CardAction(ActionTypes.MessageBack, "Edit Email Address", null, "Edit Email Address", "Edit Email Address", ""),
                    new CardAction(ActionTypes.MessageBack, "Contact Support", null, "Contact Support", "Contact Support", ""),
                },

            };



            var response = MessageFactory.Attachment(card.ToAttachment());



            await turnContext.SendActivityAsync(response, cancellationToken);



        }

        private static async Task SuggestedActions(ITurnContext turnContext, CancellationToken cancellationToken, string menuOption)
        {
            var menuopt = "";

            switch (menuOption)
            {
                case "preference":
                    menuopt = "Preference Menu";
                    break;
                case "timesheet":
                    menuopt = "Timesheet Menu";
                    break;
                case "expenses":
                    menuopt = "Expense Menu";
                    break;
                default:
                    menuopt = "";
                    break;
            }


           var reply =  MessageFactory.Text("");

            if (!string.IsNullOrEmpty(menuopt))
            {

                reply.SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                        {
                            new CardAction() { Title = "Main Menu", Type = ActionTypes.MessageBack, Text = "intro", Value = "intro" },
                            new CardAction() { Title = menuopt, Type = ActionTypes.MessageBack,Text = menuOption , Value = menuOption },
                        },
                };

            }
            else
            {

                reply.SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                        {
                           new CardAction() { Title = "Main Menu", Type = ActionTypes.MessageBack, Text = "intro", Value = "intro" }
                           
                        },
                };

            }

            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

    }
}
