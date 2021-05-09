using BanCoreBot.Dialogs.Cancel;
using BanCoreBot.Dialogs.Reclamos;
using BanCoreBot.Infrastructure.Luis;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BanCoreBot.Dialogs.Transferencia
{
    public class TransferenciaDialog : CancelDialog
    {
        private ILuisService _luisService;
        private const string _ShowOptions = "ShowOptions";
        private const string _ValidateShowOptions = "ValidateShowOptions";
        public TransferenciaDialog(ILuisService luisService) : base(nameof(TransferenciaDialog))
        {
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
            var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(stepContext.Context, cancellationToken).Result;
            if ((luisResult.Entities.opcion1List != null || luisResult.Entities.operar != null || luisResult.Entities.transferir  != null  || luisResult.TopIntent().ToString().Equals("transferencia"))
               && luisResult.Entities.problema is null && luisResult.Entities.nopuedo is null)
            {
                await stepContext.Context.SendActivityAsync($"Para realizar una Transferencia, debes poseer tu Tarjeta Conexión Segura Activa al igual que tu Perfil de Seguridad y ya debes poseer la cuenta afiliada. Si posees esta serie de requisitos tienes que ingresar simplemente a  Mi conexión Bancaribe, persona natural, ingresa tus datos de acceso. Una vez dentro de tu cuenta, dirigirte a la sección de \"Transferencias\", selecciona si es \"Cuenta Propia\", a \"Terceros Bancaribe\" o \"Otro Banco\" y elige a que cuentas deseas realizar la transferencia. ", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
            else
            {

                return await stepContext.BeginDialogAsync(nameof(FechaTransferenciaReclamo), cancellationToken: cancellationToken); 
                //return await stepContext.BeginDialogAsync(nameof(ReclamoTransferencia), cancellationToken: cancellationToken);
            }

        }

        #endregion DialogCtas


        #region Validators


        private Task<bool> ValidateInputValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var result = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(promptContext.Context, cancellationToken).Result;

            if (
                result.TopIntent().intent.ToString().Equals("transferencia")
                ||
                result.TopIntent().intent.ToString().Equals("Opciones")
                ||
                result.TopIntent().intent.ToString().Equals("ReclamoTransferencia")
                ||
                result.Entities.problema != null
                ||
                result.Entities.transferir != null
                ||
                result.Entities.nopuedo != null
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
                reply = MessageFactory.Text("¿Deseas realizar una Transferencia? O ¿Presentas problemas para Transferir?");
            }
            else
            {
                reply = MessageFactory.Text("¿Necesitas conocer cómo realizar una transferencia o tienes problemas al realizarla?");
            }

            return reply as Activity;
        }

        #endregion Buttons

    }
}
