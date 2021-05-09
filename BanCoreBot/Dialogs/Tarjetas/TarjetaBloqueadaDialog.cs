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

namespace BanCoreBot.Dialogs.Tarjetas
{
    public class TarjetaBloqueadaDialog : CancelDialog
    {
        private BotState _userState;
        private ILuisService _luisService;
        private const string _ShowOptions = "ShowOptions";
        private const string _ValidateShowOptions = "ValidateShowOptions";
        private UserPersonalData ClientData;

        public TarjetaBloqueadaDialog(ILuisService luisService, PrivateConversationState userState)
            : base(nameof(TarjetaBloqueadaDialog))
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
            if (!String.IsNullOrEmpty(ClientData.request))
            {
                if (ClientData.request.Contains("Natural") || ClientData.request.Contains("Juridico"))
                {
                    if ((luisResult.Entities.tdc != null && luisResult.Entities.tdd is null && luisResult.Entities.tarjetasegura is null) ||
                       luisResult.Entities.opcion1List != null)
                    {//Tarjeta de Crédito
                        ClientData.request = ClientData.request + "TDC";
                        return await stepContext.BeginDialogAsync(nameof(BloquearDesbloquearTjtaDialog), cancellationToken: cancellationToken);
                    }
                    else if ((luisResult.Entities.tdd != null && luisResult.Entities.tdc is null && luisResult.Entities.tarjetasegura is null) ||
                        luisResult.Entities.opcion2List != null)
                    {// Tarjeta de Débito
                        ClientData.request = ClientData.request + "TDD";
                        return await stepContext.BeginDialogAsync(nameof(BloquearDesbloquearTjtaDialog), cancellationToken: cancellationToken);
                    }
                    else
                    {// Tarjeta Segura
                        ClientData.request = ClientData.request + "Segura";
                        return await stepContext.BeginDialogAsync(nameof(BloquearDesbloquearTjtaDialog), cancellationToken: cancellationToken);
                    }
                }
                else
                {
                    if ((luisResult.Entities.tdc != null && luisResult.Entities.tdd is null && luisResult.Entities.tarjetasegura is null) ||
                      luisResult.Entities.opcion1List != null)
                    {//Tarjeta de Crédito
                        ClientData.request = "TDC";
                        return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
                    }
                    else if ((luisResult.Entities.tdd != null && luisResult.Entities.tdc is null && luisResult.Entities.tarjetasegura is null) ||
                        luisResult.Entities.opcion2List != null)
                    {// Tarjeta de Débito
                        ClientData.request = "TDD";
                        return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
                    }
                    else
                    {// Tarjeta Segura
                        ClientData.request = "Segura";
                        return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
                    }
                }
            }
            else
            {
                if ((luisResult.Entities.tdc != null && luisResult.Entities.tdd is null && luisResult.Entities.tarjetasegura is null) ||
                      luisResult.Entities.opcion1List != null)
                {//Tarjeta de Crédito
                    ClientData.request = "TDC";
                    return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
                }
                else if ((luisResult.Entities.tdd != null && luisResult.Entities.tdc is null && luisResult.Entities.tarjetasegura is null) ||
                    luisResult.Entities.opcion2List != null)
                {// Tarjeta de Débito
                    ClientData.request = "TDD";
                    return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
                }
                else
                {// Tarjeta Segura
                    ClientData.request = "Segura";
                    return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
                }
            }
        }

        #endregion DialogCtas


        #region Validators


        private Task<bool> ValidateInputValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var result = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(promptContext.Context, cancellationToken).Result;
            if (
               result.TopIntent().intent.ToString().Equals("TipoTarjeta")
               || result.TopIntent().intent.ToString().Equals("Opciones")
               || result.Entities.tdc!= null
               || result.Entities.tdd != null
               || result.Entities.tarjetasegura != null
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
                reply = MessageFactory.Text("¿Me estás indicando que presentas un inconveniente con tu tarjeta de débito, tarjeta de crédito o tarjeta de conexión segura?");
            }
            else
            {
                reply = MessageFactory.Text("El inconveniente lo presentas con tu tarjeta de débito, tarjeta de crédito o la tarjeta de conexión segura?");
            }

            return reply as Activity;
        }

        #endregion Buttons

    }
}
