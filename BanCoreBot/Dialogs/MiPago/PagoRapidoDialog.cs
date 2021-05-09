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
    public class PagoRapidoDialog : CancelDialog
    {
        private ILuisService _luisService;
        private const string _ShowOptions = "ShowOptions";
        private const string _ValidateShowOptions = "ValidateShowOptions";
        public PagoRapidoDialog(ILuisService luisService) : base(nameof(PagoRapidoDialog))
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
                await stepContext.Context.SendActivityAsync($"Para realizar un Pago Bancaribe tienes que estar ya afiliado a este servicio, ya afiliado puedes hacer realizar el Pago por la Aplicación \"Mi pago Bancaribe\" o por SMS ", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
            else
            {
                var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(stepContext.Context, cancellationToken).Result;
                if (luisResult.TopIntent().intent.ToString().Equals("afirmacion"))
                {
                    await stepContext.Context.SendActivityAsync($"Para realizar un Pago Bancaribe tienes que estar ya afiliado a este servicio, ya afiliado puedes hacer realizar el Pago por la Aplicación \"Mi pago Bancaribe\" o por SMS ", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync("Entiendo, brindame más información de lo que deseas para ayudarte", cancellationToken: cancellationToken);
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
                reply = MessageFactory.Text("Quieres saber ¿Cómo hacer Pagos Bancaribe?");
            }
            else
            {
                reply = MessageFactory.Text("Por favor, me confirmas si deseas conocer como hacer Pagos Bancaribe");
            }

            return reply as Activity;
        }

        #endregion Buttons

    }
}
