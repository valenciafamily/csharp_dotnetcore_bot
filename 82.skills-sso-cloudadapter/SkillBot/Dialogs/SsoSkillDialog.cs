﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;

namespace Microsoft.BotBuilderSamples.SSOSkillBot.Dialogs
{
    public class SsoSkillDialog : ComponentDialog
    {
        private readonly string _connectionName;

        public SsoSkillDialog(string connectionName)
            : base(nameof(SsoSkillDialog))
        {
            _connectionName = connectionName;
            if (string.IsNullOrWhiteSpace(_connectionName))
            {
                throw new ArgumentException("\"ConnectionName\" is not set in configuration");
            }

            AddDialog(new SsoSkillSignInDialog(_connectionName));
            AddDialog(new ChoicePrompt("ActionStepPrompt"));

            var waterfallSteps = new WaterfallStep[]
            {
                PromptActionStepAsync,
                HandleActionStepAsync,
                PromptFinalStepAsync
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> PromptActionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var messageText = "What SSO action would you like to perform on the skill?";
            var repromptMessageText = "That was not a valid choice, please select a valid choice.";
            var options = new PromptOptions
            {
                Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput),
                RetryPrompt = MessageFactory.Text(repromptMessageText, repromptMessageText, InputHints.ExpectingInput),
                Choices = await GetPromptChoicesAsync(stepContext, cancellationToken)
            };

            // Prompt the user to select a skill.
            return await stepContext.PromptAsync("ActionStepPrompt", options, cancellationToken);
        }

        private async Task<List<Choice>> GetPromptChoicesAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Try to get the token for the current user to determine if it is logged in or not.
            var userId = stepContext.Context.Activity?.From?.Id;
            var userTokenClient = stepContext.Context.TurnState.Get<UserTokenClient>();
            var token = await userTokenClient.GetUserTokenAsync(userId, _connectionName, stepContext.Context.Activity?.ChannelId, null, cancellationToken);

            // Present different choices depending on the user's sign in status.
            var promptChoices = new List<Choice>();
            if (token == null)
            {
                promptChoices.Add(new Choice("Login to the skill"));
            }
            else
            {
                promptChoices.Add(new Choice("Logout from the skill"));
                promptChoices.Add(new Choice("Show token"));
            }

            promptChoices.Add(new Choice("End"));

            return promptChoices;
        }

        private async Task<DialogTurnResult> HandleActionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var action = ((FoundChoice)stepContext.Result).Value.ToLowerInvariant();
            var userId = stepContext.Context.Activity?.From?.Id;
            var userTokenClient = stepContext.Context.TurnState.Get<UserTokenClient>();

            switch (action)
            {
                case "login to the skill":
                    // The SsoSkillSignInDialog will just show the user token if the user logged on to the root bot.
                    return await stepContext.BeginDialogAsync(nameof(SsoSkillSignInDialog), null, cancellationToken);

                case "logout from the skill":
                    // This will just clear the token from the skill.
                    await userTokenClient.SignOutUserAsync(userId, _connectionName, stepContext.Context.Activity?.ChannelId, cancellationToken);
                    await stepContext.Context.SendActivityAsync("You have been signed out.", cancellationToken: cancellationToken);
                    return await stepContext.NextAsync(cancellationToken: cancellationToken);

                case "show token":
                    var token = await userTokenClient.GetUserTokenAsync(userId, _connectionName, stepContext.Context.Activity?.ChannelId, null, cancellationToken);
                    if (token == null)
                    {
                        await stepContext.Context.SendActivityAsync("User has no cached token.", cancellationToken: cancellationToken);
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync($"Here is your current SSO token for the skill: {token.Token}", cancellationToken: cancellationToken);
                    }

                    return await stepContext.NextAsync(cancellationToken: cancellationToken);

                case "end":
                    // Ends the interaction with the skill.
                    return new DialogTurnResult(DialogTurnStatus.Complete);

                default:
                    // This should never be hit since the previous prompt validates the choice.
                    throw new InvalidOperationException($"Unrecognized action: {action}");
            }
        }

        private async Task<DialogTurnResult> PromptFinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Restart the dialog (we will exit when the user says "end").
            return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
        }
    }
}
