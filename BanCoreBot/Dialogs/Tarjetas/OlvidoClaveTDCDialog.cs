using BanCoreBot.Dialogs.Cancel;
using BanCoreBot.Infrastructure.Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Threading;
using System.Threading.Tasks;
using BanCoreBot.Common.Models.User;

namespace BanCoreBot.Dialogs.Tarjetas
{
    public class OlvidoClaveTDCDialog : CancelDialog
    {
        private BotState _userState;
        private ILuisService _luisService;
        private const string _ShowOptions = "ShowOptions";
        private const string _ValidateShowOptions = "ValidateShowOptions";
        private UserPersonalData ClientData;

        public OlvidoClaveTDCDialog(ILuisService luisService, PrivateConversationState userState)
            : base(nameof(OlvidoClaveTDCDialog))
        {
            _userState = userState;
            _luisService = luisService;
            var waterfallSteps = new WaterfallStep[]
            {
                ShowOptions,
                ValidateShowOptions
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(_ShowOptions));
            AddDialog(new TextPrompt(_ValidateShowOptions));
        }


        #region DialogCtas

        private async Task<DialogTurnResult> ShowOptions(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var option = await stepContext.PromptAsync(
                _ShowOptions,
                new PromptOptions
                {
                    Prompt = CreateButtons("")
                }, cancellationToken
            );
            return option;
        }

        private async Task<DialogTurnResult> ValidateShowOptions(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());

            ClientData.request = "olvidoclavetdc";
            await stepContext.Context.SendActivityAsync($"Para ayudarte con la clave de tu tarjeta de crédito, voy a solicitarte algunos datos que luego enviaré al área encargada.", cancellationToken: cancellationToken);
            return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
        }

        #endregion DialogCtas


        #region Buttons

        private Activity CreateButtons(string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                reply = MessageFactory.Text("¿Me estas queriendo decir que le deseas asignar una clave a tu tarjeta de crédito o quieres realizar el cambio de clave?");
            }

            return reply as Activity;
        }

        #endregion Buttons

    }
}
