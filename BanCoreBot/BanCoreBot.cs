using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanCoreBot.Infrastructure;
using Microsoft.ApplicationInsights;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace BanCoreBot
{
    public class BanCoreBot<T> : ActivityHandler where T:Dialog
    {
        private readonly TelemetryClient telemetryClient;
        private readonly BotState _userState;
        private readonly BotState _privateconversationState;
        private readonly Dialog _dialog;
        protected readonly ILogger Logger;

        public BanCoreBot(UserState userState, PrivateConversationState privateconversationState, T dialog, ILogger<BanCoreBot<T>> logger, TelemetryClient telemetryClient)
        {
            _userState = userState;
            _privateconversationState = privateconversationState;
            _dialog = dialog;
            Logger = logger;
            this.telemetryClient = telemetryClient;
        }
        
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                  
                }
            }
        }


        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate && turnContext.Activity.MembersAdded.FirstOrDefault()?.Id == turnContext.Activity.Recipient.Id)
            {
               await turnContext.SendActivityAsync("Hola soy ARIA, estoy aquí para ayudarte a través de una experiencia interactiva, ¿en qué te puedo servir?", cancellationToken: cancellationToken);
               // await turnContext.SendActivityAsync("Hola soy ARIA, desde Bancaribe deseamos que pases unas Felices Fiestas en compañía de tus seres queridos, ¿en qué te puedo ayudar?", cancellationToken: cancellationToken);

            }
            
            await base.OnTurnAsync(turnContext, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _privateconversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            Dictionary<string, string> Telemetry = new Dictionary<string, string>();
            Telemetry.Add("From", turnContext.Activity.From.Id);

            if (turnContext.Activity.ChannelId.Equals("directline"))
            {
                telemetryClient.TrackEvent("DirectLine", Telemetry);
            }
            if (turnContext.Activity.ChannelId.Equals("twitter"))
            {
                telemetryClient.TrackEvent("Twitter", Telemetry);
            }
            await _dialog.RunAsync(
                turnContext,
                _privateconversationState.CreateProperty<DialogState>(nameof(DialogState)),
                cancellationToken
                );
        }
    }
}
