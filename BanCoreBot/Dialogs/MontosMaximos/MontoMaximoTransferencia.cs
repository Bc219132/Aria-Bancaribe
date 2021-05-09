using BanCoreBot.Dialogs.Cancel;
using BanCoreBot.Infrastructure.Luis;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BanCoreBot.Dialogs.MontosMaximos
{
    public class MontoMaximoTransferencia : CancelDialog
    {
        private ILuisService _luisService;
        private const string _ShowOptions = "ShowOptions";
        private const string _ValidateShowOptions = "ValidateShowOptions";
        public MontoMaximoTransferencia(ILuisService luisService) : base(nameof(MontoMaximoTransferencia))
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
            if (luisResult.Entities.juridico !=null || luisResult.Entities.opcion2List != null || luisResult.Entities.opcionultimaList != null)
            {
                await stepContext.Context.SendActivityAsync($"Hasta la fecha el Máximo Diario para  transferencia es de:{ Environment.NewLine} " +
                    $"📌 Transferencia Propias a Bancaribe: no existe límite de monto {Environment.NewLine}" +
                    $"📌 Transferencia Terceros Bancaribe: no existe límite de monto { Environment.NewLine}" +
                    $"📌 Transferencia a otros Bancos: 7.000.000.000,00BsS", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync($"Hasta la fecha el Máximo Diario para  transferencia es de:{ Environment.NewLine} " +
                $"📌 Transferencia Propias a Bancaribe:  no existe límite de monto {Environment.NewLine}" +
                $"📌 Transferencia Terceros Bancaribe:  no existe límite de monto {Environment.NewLine}" +
                $"📌 Transferencia a otros Bancos: 1.000.000.000,00BsS, ten en cuenta que por motivos de seguridad al transferir a otros bancos el sistema no dejará transferir el máximo, te sugiero realizar tus transferencias de manera fraccionadas, c/u de  500.000.000,00 Bs o montos menores", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }

        }

        #endregion DialogCtas


        #region Validators


        private Task<bool> ValidateInputValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var luisresult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(promptContext.Context, cancellationToken).Result;

            if (
                luisresult.TopIntent().intent.ToString().Equals("TipoPersona") ||
                luisresult.TopIntent().intent.ToString().Equals("Opciones") ||
                luisresult.Entities.natural != null ||
                luisresult.Entities.juridico != null
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
                reply = MessageFactory.Text("¿Deseas saber esta información de una Cuenta Natural o una Cuenta Jurídica?");
            }
            else
            {
                reply = MessageFactory.Text("Por favor, me indicas el tipo de cuenta (natural o jurídica) para poder darte el monto máximo a transferir");
            }

            return reply as Activity;
        }

        #endregion Buttons

    }
}
