// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.BotBuilderSamples.TeamsSkillBot.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples.TeamsSkillBot.Bots
{
    /// <summary>
    /// Partial TeamsBot that has the code related to TaskModule and CardActions. 
    /// </summary>
    public partial class TeamsBot
    {
        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("OnTeamsTaskModuleFetchAsync TaskModuleRequest: " + JsonConvert.SerializeObject(taskModuleRequest));

            await turnContext.SendActivityAsync(reply, cancellationToken);

            return new TaskModuleResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo
                    {
                        Card = CreateAdaptiveCardAttachment(),
                        Height = 200,
                        Width = 400,
                        Title = "Adaptive Card: Inputs",
                    },
                },
            };
        }

        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("OnTeamsTaskModuleSubmitAsync Value: " + JsonConvert.SerializeObject(taskModuleRequest));
            await turnContext.SendActivityAsync(reply, cancellationToken);

            // Send End of conversation at the end.
            var activity = new Activity(ActivityTypes.EndOfConversation)
            {
                Value = taskModuleRequest.Data,
                Locale = ((Activity)turnContext.Activity).Locale
            };
            await turnContext.SendActivityAsync(activity, cancellationToken);

            return new TaskModuleResponse
            {
                Task = new TaskModuleMessageResponse
                {
                    Value = "Thanks!",
                },
            };
        }

        protected override async Task<InvokeResponse> OnTeamsCardActionInvokeAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text("hello from OnTeamsCardActionInvokeAsync."), cancellationToken);

            // Send End of conversation at the end.
            var activity = new Activity(ActivityTypes.EndOfConversation) { Locale = ((Activity)turnContext.Activity).Locale };
            await turnContext.SendActivityAsync(activity, cancellationToken);

            return new InvokeResponse { Status = (int)HttpStatusCode.OK };
        }
    }
}
