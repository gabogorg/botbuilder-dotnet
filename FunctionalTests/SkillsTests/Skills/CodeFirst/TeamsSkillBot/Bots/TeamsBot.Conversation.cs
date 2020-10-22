// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples.TeamsSkillBot.Bots
{
    /// <summary>
    /// Partial portion of the TeamsBot class that handles conversations (messages, update and delete).
    /// </summary>
    public partial class TeamsBot
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            turnContext.Activity.RemoveRecipientMention();
            var text = turnContext.Activity.Text.Trim().ToLower();

            if (text.Contains("mention"))
            {
                await MentionActivityAsync(turnContext, cancellationToken);
            }
            else if (text.Contains("who"))
            {
                await GetSingleMemberAsync(turnContext, cancellationToken);
            }
            else if (text.Contains("update"))
            {
                await CardActivityAsync(turnContext, true, cancellationToken);
            }
            else if (text.Contains("message"))
            {
                await MessageAllMembersAsync(turnContext, cancellationToken);
            }
            else if (text.Contains("delete"))
            {
                await DeleteCardActivityAsync(turnContext, cancellationToken);
            }
            else
            {
                await CardActivityAsync(turnContext, false, cancellationToken);
            }
        }

        private async Task CardActivityAsync(ITurnContext<IMessageActivity> turnContext, bool update, CancellationToken cancellationToken)
        {
            var card = new HeroCard
            {
                Buttons = new List<CardAction>
                {
                    new CardAction
                    {
                        Type = ActionTypes.MessageBack,
                        Title = "Message all members",
                        Text = "MessageAllMembers",
                        Value = new JObject { { "count", 0 } }
                    },
                    new CardAction
                    {
                        Type = ActionTypes.MessageBack,
                        Title = "Who am I?",
                        Text = "whoami",
                        Value = new JObject { { "count", 0 } }
                    },
                    new CardAction
                    {
                        Type = ActionTypes.MessageBack,
                        Title = "Delete card",
                        Text = "Delete",
                        Value = new JObject { { "count", 0 } }
                    }
                }
            };

            if (update)
            {
                await SendUpdatedCard(turnContext, card, cancellationToken);
            }
            else
            {
                await SendWelcomeCard(turnContext, card, cancellationToken);
            }
        }

        private async Task GetSingleMemberAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            TeamsChannelAccount member;

            try
            {
                member = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, cancellationToken);
            }
            catch (ErrorResponseException e)
            {
                if (e.Body.Error.Code.Equals("MemberNotFoundInConversation"))
                {
                    await turnContext.SendActivityAsync("Member not found.");
                    return;
                }

                throw e;
            }

            var message = MessageFactory.Text($"You are: {member.Name}.");
            var res = await turnContext.SendActivityAsync(message);
        }

        private async Task DeleteCardActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.DeleteActivityAsync(turnContext.Activity.ReplyToId, cancellationToken);

            // Send EoC when the card is deleted
            var eocActivity = Activity.CreateEndOfConversationActivity();
            eocActivity.Code = EndOfConversationCodes.CompletedSuccessfully;
            await turnContext.SendActivityAsync(eocActivity, cancellationToken);
        }

        // If you encounter permission-related errors when sending this message, see
        // https://aka.ms/BotTrustServiceUrl
        private async Task MessageAllMembersAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var teamsChannelId = turnContext.Activity.TeamsGetChannelId();
            var serviceUrl = turnContext.Activity.ServiceUrl;
            var credentials = new MicrosoftAppCredentials(_appId, _appPassword);
            ConversationReference conversationReference = null;

            var members = await GetPagedMembers(turnContext, cancellationToken);

            foreach (var teamMember in members)
            {
                var proactiveMessage = MessageFactory.Text($"Hello {teamMember.GivenName} {teamMember.Surname}. I'm a Teams conversation bot.");

                var conversationParameters = new ConversationParameters
                {
                    IsGroup = false,
                    Bot = turnContext.Activity.Recipient,
                    Members = new ChannelAccount[] { teamMember },
                    TenantId = turnContext.Activity.Conversation.TenantId,
                };

                await ((BotFrameworkAdapter)turnContext.Adapter).CreateConversationAsync(
                    teamsChannelId,
                    serviceUrl,
                    credentials,
                    conversationParameters,
                    async (t1, c1) =>
                    {
                        conversationReference = t1.Activity.GetConversationReference();
                        await ((BotFrameworkAdapter)turnContext.Adapter).ContinueConversationAsync(
                            _appId,
                            conversationReference,
                            async (t2, c2) =>
                            {
                                await t2.SendActivityAsync(proactiveMessage, c2);
                            },
                            cancellationToken);
                    },
                    cancellationToken);
            }

            await turnContext.SendActivityAsync(MessageFactory.Text("All messages have been sent."), cancellationToken);
        }

        private static async Task<List<TeamsChannelAccount>> GetPagedMembers(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var members = new List<TeamsChannelAccount>();
            string continuationToken = null;

            do
            {
                var currentPage = await TeamsInfo.GetPagedMembersAsync(turnContext, 100, continuationToken, cancellationToken);
                continuationToken = currentPage.ContinuationToken;
                members = members.Concat(currentPage.Members).ToList();
            } while (continuationToken != null);

            return members;
        }

        private static async Task SendWelcomeCard(ITurnContext<IMessageActivity> turnContext, HeroCard card, CancellationToken cancellationToken)
        {
            var initialValue = new JObject { { "count", 0 } };
            card.Title = "Welcome!";
            card.Buttons.Add(new CardAction
            {
                Type = ActionTypes.MessageBack,
                Title = "Update Card",
                Text = "UpdateCardAction",
                Value = initialValue
            });

            var activity = MessageFactory.Attachment(card.ToAttachment());

            await turnContext.SendActivityAsync(activity, cancellationToken);
        }

        private static async Task SendUpdatedCard(ITurnContext<IMessageActivity> turnContext, HeroCard card, CancellationToken cancellationToken)
        {
            card.Title = "I've been updated";

            var data = turnContext.Activity.Value as JObject;
            data = JObject.FromObject(data);
            data["count"] = data["count"].Value<int>() + 1;
            card.Text = $"Update count - {data["count"].Value<int>()}";

            card.Buttons.Add(new CardAction
            {
                Type = ActionTypes.MessageBack,
                Title = "Update Card",
                Text = "UpdateCardAction",
                Value = data
            });

            var activity = MessageFactory.Attachment(card.ToAttachment());
            activity.Id = turnContext.Activity.ReplyToId;

            await turnContext.UpdateActivityAsync(activity, cancellationToken);
        }

        private async Task MentionActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var mention = new Mention
            {
                Mentioned = turnContext.Activity.From,
                Text = $"<at>{XmlConvert.EncodeName(turnContext.Activity.From.Name)}</at>",
            };

            var replyActivity = MessageFactory.Text($"Hello {mention.Text}.");
            replyActivity.Entities = new List<Entity> { mention };

            await turnContext.SendActivityAsync(replyActivity, cancellationToken);
        }
    }
}
