using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Sample.QnABot
{
    [Serializable]
    public class RootDialog :  IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            /* Wait until the first message is received from the conversation and call MessageReceviedAsync 
            *  to process that message. */
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            /* When MessageReceivedAsync is called, it's passed an IAwaitable<IMessageActivity>. To get the message,
            *  await the result. */
            var message = await result;
            
            var qnaSubscriptionKey = Utils.GetAppSetting("QnASubscriptionKey");
            var qnaKBId = Utils.GetAppSetting("QnAKnowledgebaseId");

            // QnA Subscription Key and KnowledgeBase Id null verification
            if (!string.IsNullOrEmpty(qnaSubscriptionKey) && !string.IsNullOrEmpty(qnaKBId))
            {
                await context.Forward(new BasicQnAMakerDialog(), AfterAnswerAsync, message, CancellationToken.None);
            }
            else
            {
                await context.PostAsync("Please set QnAKnowledgebaseId and QnASubscriptionKey in App Settings. Get them at https://qnamaker.ai.");
            }
            
        }

        private async Task AfterAnswerAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            // wait for the next user message
            context.Wait(MessageReceivedAsync);
        }
    }

    // For more information about this template visit http://aka.ms/azurebots-csharp-qnamaker
    [Serializable]
    public class BasicQnAMakerDialog : QnAMakerDialog
    {
        // Go to https://qnamaker.ai and feed data, train & publish your QnA Knowledgebase.        
        // Parameters to QnAMakerService are:
        // Required: subscriptionKey, knowledgebaseId, 
        // Optional: defaultMessage, scoreThreshold[Range 0.0 – 1.0]
        public BasicQnAMakerDialog() : base(new QnAMakerService(new QnAMakerAttribute(Utils.GetAppSetting("QnASubscriptionKey"), Utils.GetAppSetting("QnAKnowledgebaseId"), "No good match in FAQ.", 0.5)))
        {}
    }
}