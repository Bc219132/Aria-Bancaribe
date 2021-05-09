using BanCoreBot.Dialogs.Cancel;
using BanCoreBot.Infrastructure.Luis;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Threading;
using System.Threading.Tasks;
using BanCoreBot.Common.Models.User;
using BanCoreBot.Dialogs.UsuarioClave;

namespace BanCoreBot.Dialogs.Tarjetas
{
    public class OlvidoClaveDialog : CancelDialog
    {
        private BotState _userState;
        private ILuisService _luisService;
        private const string _ShowOptions = "ShowOptions";
        private const string _ValidateShowOptions = "ValidateShowOptions";
        private UserPersonalData ClientData;

        public OlvidoClaveDialog(ILuisService luisService, PrivateConversationState userState) 
            : base(nameof(OlvidoClaveDialog))
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
            if (ClientData.request != null)
            {
                if (luisResult.Entities.tdc != null || luisResult.Entities.opcion1List != null)
                {//Tarjeta de Crédito
                    ClientData.request = "olvidoclavetdc";
                    return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
                }
                else if (luisResult.Entities.tdd != null || luisResult.Entities.opcion2List != null)
                {// Tarjeta de Débito
                    ClientData.request = "olvidoclave";
                    return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
                }
                else
                {// Conexión Bancaribe
                    return await stepContext.BeginDialogAsync(nameof(ClaveDialog), cancellationToken: cancellationToken);
                }

            }
            else
            {
                if (luisResult.Entities.tdc != null || luisResult.Entities.opcion1List != null)
                {//Tarjeta de Crédito
                    ClientData.request = "olvidoclavetdc";
                    return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
                }
                else if (luisResult.Entities.tdd != null || luisResult.Entities.opcion2List != null)
                {// Tarjeta de Débito
                    ClientData.request = "olvidoclave";
                    return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
                }
                else
                {// Conexión Bancaribe
                    return await stepContext.BeginDialogAsync(nameof(ClaveDialog), cancellationToken: cancellationToken);
                }
            }
        }

        #endregion DialogCtas


        #region Validators


        private Task<bool> ValidateInputValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(promptContext.Context, cancellationToken).Result;
            if (
                luisResult.TopIntent().intent.ToString().Equals("TipoTarjeta")
               || luisResult.TopIntent().intent.ToString().Equals("Opciones")
               || luisResult.TopIntent().intent.ToString().Equals("conexionBancaribe")

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
                reply = MessageFactory.Text("¿La clave a la que haces referencia es la de una tarjeta de crédito, tarjeta de débito o mi conexión en línea?");
            }
            else
            {
                reply = MessageFactory.Text("Por favor me especificas ¿a que clave te refieres, la de una tarjeta de crédito, tarjeta de débito o mi conexión en línea?");
            }

            return reply as Activity;
        }

        #endregion Buttons

    }
}
