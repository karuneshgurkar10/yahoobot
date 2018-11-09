using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;
using Microsoft.Bot.Connector;
using System.Linq;

namespace Microsoft.Bot.Sample.QnABot
{
    [Serializable]
    public class RootDialog : IDialog<object>
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

            var qnaAuthKey = "b3100a4e-c6ce-4c2d-970c-b37d156a7cae";
            var qnaKBId = "b10d8530-90ed-4c70-8df7-363abb76a708";
            var endpointHostName = "https://faqyahoochatbot.azurewebsites.net/qnamaker";

            // QnA Subscription Key and KnowledgeBase Id null verification
            if (!string.IsNullOrEmpty(qnaAuthKey) && !string.IsNullOrEmpty(qnaKBId))
            {
                // Forward to the appropriate Dialog based on whether the endpoint hostname is present
                if (string.IsNullOrEmpty(endpointHostName))
                    await context.Forward(new BasicQnAMakerPreviewDialog(), AfterAnswerAsync, message, CancellationToken.None);
                else
                    await context.Forward(new BasicQnAMakerDialog(), AfterAnswerAsync, message, CancellationToken.None);
            }
            else
            {
                await context.PostAsync("Please set QnAKnowledgebaseId, QnAAuthKey and QnAEndpointHostName (if applicable) in App Settings. Learn how to get them at https://aka.ms/qnaabssetup.");
            }

        }

        private async Task AfterAnswerAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            // wait for the next user message


            context.Wait(MessageReceivedAsync);
        }

        public static string GetSetting(string key)
        {
            var value = Utils.GetAppSetting(key);
            if (String.IsNullOrEmpty(value) && key == "QnAAuthKey")
            {
                value = Utils.GetAppSetting("QnASubscriptionKey"); // QnASubscriptionKey for backward compatibility with QnAMaker (Preview)
            }
            return value;
        }
    }

    // Dialog for QnAMaker Preview service
    [Serializable]
    public class BasicQnAMakerPreviewDialog : QnAMakerDialog
    {
        // Go to https://qnamaker.ai and feed data, train & publish your QnA Knowledgebase.
        // Parameters to QnAMakerService are:
        // Required: subscriptionKey, knowledgebaseId, 
        // Optional: defaultMessage, scoreThreshold[Range 0.0 – 1.0]
        public BasicQnAMakerPreviewDialog() : base(new QnAMakerService(new QnAMakerAttribute(RootDialog.GetSetting("QnAAuthKey"), Utils.GetAppSetting("QnAKnowledgebaseId"), "No good match in FAQ.", 0.5)))
        { }
    }

    // Dialog for QnAMaker GA service
    [Serializable]
    public class BasicQnAMakerDialog : QnAMakerDialog
    {
        // Go to https://qnamaker.ai and feed data, train & publish your QnA Knowledgebase.
        // Parameters to QnAMakerService are:
        // Required: qnaAuthKey, knowledgebaseId, endpointHostName
        // Optional: defaultMessage, scoreThreshold[Range 0.0 – 1.0]
        public BasicQnAMakerDialog() : base(new QnAMakerService(new QnAMakerAttribute("b3100a4e-c6ce-4c2d-970c-b37d156a7cae", "b10d8530-90ed-4c70-8df7-363abb76a708", "No good match in FAQ.", 0.5, 3, "https://faqyahoochatbot.azurewebsites.net/qnamaker")))
        {


        }




    }

   
}