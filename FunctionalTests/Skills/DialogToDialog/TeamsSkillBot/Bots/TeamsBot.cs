// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.BotBuilderSamples.TeamsSkillBot.Extensions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples.TeamsSkillBot.Bots
{
    /// <summary>
    /// Main TeamsBot that starts a skill activity based on an event message.
    /// </summary>
    public partial class TeamsBot : TeamsActivityHandler
    {
        private readonly string _appId;
        private readonly string _appPassword;

        public TeamsBot(IConfiguration config)
        {
            _appId = config["MicrosoftAppId"];
            _appPassword = config["MicrosoftAppPassword"];
        }

        protected override async Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            switch (turnContext.Activity.Name)
            {
                case "TeamsTaskModule":
                    var reply = MessageFactory.Attachment(GetTaskModuleHeroCard());
                    await turnContext.SendActivityAsync(reply, cancellationToken);
                    break;

                case "TeamsCardAction":
                    var cardInvocationActivityCard = MessageFactory.Attachment(GetAdaptiveCardWithInvokeAction());
                    await turnContext.SendActivityAsync(cardInvocationActivityCard, cancellationToken);
                    break;

                case "TeamsConversation":
                    await CardActivityAsync(new DelegatingTurnContext<IMessageActivity>(turnContext), false, cancellationToken);
                    break;

                default:
                    await turnContext.SendActivityAsync($"Unknown event name {turnContext.Activity.Name}", cancellationToken: cancellationToken);
                    break;
            }
        }

        protected override async Task OnTeamsMembersAddedAsync(IList<TeamsChannelAccount> membersAdded, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var teamMember in membersAdded)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text($"Welcome to the team {teamMember.GivenName} {teamMember.Surname}."), cancellationToken);
            }
        }

        private Attachment GetTaskModuleHeroCard()
        {
            return new HeroCard
            {
                Title = "Task Module Invocation from Hero Card",
                Subtitle = "This is a hero card with a Task Module Action button.  Click the button to show an Adaptive Card within a Task Module.",
                Buttons = new List<CardAction>
                {
                    new TaskModuleAction("Adaptive Card", new { data = "adaptivecard" }),
                },
            }.ToAttachment();
        }

        private Attachment GetAdaptiveCardWithInvokeAction()
        {
            var adaptiveCard = new AdaptiveCard();
            adaptiveCard.Body.Add(new AdaptiveTextBlock("Bot Builder Invoke Action"));
            var action4 = new CardAction("invoke", "invoke", null, null, null, JObject.Parse(@"{ ""key"" : ""value"" }"));
            adaptiveCard.Actions.Add(action4.ToAdaptiveCardAction());

            return adaptiveCard.ToAttachment();
        }

        private Attachment CreateAdaptiveCardAttachment()
        {
            // combine path for cross platform support
            string[] paths =
            {
                ".",
                "Resources",
                "adaptiveCard.json"
            };
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));

            var adaptiveCardAttachment = new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }
    }
}
