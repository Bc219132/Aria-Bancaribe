using BanCoreBot.Dialogs.Cancel;
using BanCoreBot.Infrastructure.Luis;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BanCoreBot.Dialogs.MiPago
{
    public class PagoSMSDialog : CancelDialog
    {
        private ILuisService _luisService;
        private const string _ShowOptions = "ShowOptions";
        private const string _ValidateShowOptions = "ValidateShowOptions";
        public PagoSMSDialog(ILuisService luisService) : base(nameof(PagoSMSDialog))
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
            var option = stepContext.Context.Activity.Text;
            if (option.ToLower().Equals("si"))
            {
                await stepContext.Context.SendActivityAsync($"En el cuerpo del mensaje escribirás en este orden: Mipago + Cédula del beneficiario(Incluyendo V, E o P) + Los 4 primeros dígitos de la cuenta del beneficiario + Monto del pago(con sus centimos) + Número de teléfono del beneficiario. Por último envía el SMS al número(22741)" +
                    $"{Environment.NewLine}Ejemplo: " +
                    $"{Environment.NewLine}Mipago V12345678 0114 5000,00 04141234567", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
            else
            {
                var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(stepContext.Context, cancellationToken).Result;
                if (luisResult.TopIntent().intent.ToString().Equals("afirmacion"))
                {
                    await stepContext.Context.SendActivityAsync($"En el cuerpo del mensaje escribirás en este orden: Mipago + Cédula del beneficiario(Incluyendo V, E o P) + Los 4 primeros dígitos de la cuenta del beneficiario + Monto del pago(con sus centimos) + Número de teléfono del beneficiario. Por último envía el SMS al número(22741)" +
                    $"{Environment.NewLine}Ejemplo: " +
                    $"{Environment.NewLine}Mipago V12345678 0114 5000,00 04141234567", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync("¿Puedes ser un poco más específico? No logro entender que me estas solicitando", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
            }
        }

        #endregion DialogCtas


        #region Validators


        private Task<bool> ValidateInputValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var result = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(promptContext.Context, cancellationToken).Result;
            if (
                result.TopIntent().intent.ToString().Equals("afirmacion")
               || result.TopIntent().intent.ToString().Equals("negación")
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
                reply = MessageFactory.Text("Quieres saber ¿Cómo hacer Pago Bancaribe por SMS?");
            }
            else
            {
                reply = MessageFactory.Text("Por favor, me confirmas si deseas conocer como realizar un Pago Bancaribe por SMS");
            }

            return reply as Activity;
        }

        #endregion Buttons

    }
}
