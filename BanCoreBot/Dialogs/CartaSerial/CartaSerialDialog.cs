using BanCoreBot.Dialogs.Cancel;
using BanCoreBot.Dialogs.Solicitudes;
using BanCoreBot.Infrastructure.Luis;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Threading;
using System.Threading.Tasks;
using BanCoreBot.Common.Models.User;

namespace BanCoreBot.Dialogs.CartaSerial
{
    public class CartaSerialDialog : CancelDialog
    {
        private BotState _userState;
        private ILuisService _luisService;
        private const string _ShowOptions = "ShowOptions";
        private const string _ValidateShowOptions = "ValidateShowOptions";
        private UserPersonalData ClientData;

        public CartaSerialDialog(ILuisService luisService, PrivateConversationState userState) : base(nameof(CartaSerialDialog))
        {
            _userState = userState;
            _luisService = luisService;
            var waterfallSteps = new WaterfallStep[]
            {
                ShowOptions,
                ValidateShowOptions
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(_ShowOptions, ValidateInputValidator));
            AddDialog(new TextPrompt(_ValidateShowOptions));
        }


        #region DialogCtas

        private async Task<DialogTurnResult> ShowOptions(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var option = await stepContext.PromptAsync(
                _ShowOptions,
                new PromptOptions
                {
                    Prompt = CreateButtons(""),
                    RetryPrompt = CreateButtons("Reintento")
                }, cancellationToken
            );
            return option;
        }

        private async Task<DialogTurnResult> ValidateShowOptions(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());

            var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(stepContext.Context, cancellationToken).Result;
            if ( luisResult.Entities.nopuedo != null || luisResult.Entities.problema != null)
            {
                await stepContext.Context.SendActivityAsync($"Si posees una Carta Serial, pero el sistema no te lo toma, te recomiendo validar si tu usuario para el ingreso no presenta algún tipo de bloqueo o asegúrate de que la Carta Serial que posees sea la actual y no una anterior.", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
            else
            {
                ClientData.request = "solicitudcartaserial";
                return await stepContext.BeginDialogAsync(nameof(SolicitudesClientesEmpresas), cancellationToken: cancellationToken);
            }
        }

        #endregion DialogCtas


        #region Validators


        private Task<bool> ValidateInputValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var result = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(promptContext.Context, cancellationToken).Result;
            if (
                (result.TopIntent().intent.ToString().Equals("negación") && result.Entities.nopuedo is null)
                || result.TopIntent().intent.ToString().Equals("CartaSerial")
                || result.Entities.problema != null
                || result.Entities.solicitud != null
                || result.Entities.nopuedo != null
                )

            {
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }

        #endregion Validators

        #region Buttons

        private Activity CreateButtons(string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                reply = MessageFactory.Text("El usuario y el código de afiliación se encuentran en la carta serial, ¿tienes problemas al usar la carta serial o no tienes la carta serial?");
            }
            else
            {
                reply = MessageFactory.Text("por favor, me confirmas si ¿tienes problemas al usar la carta serial o no tienes la carta serial?");
            }

            return reply as Activity;
        }

        #endregion Buttons

    }
}
