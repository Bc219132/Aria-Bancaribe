using BanCoreBot.Dialogs.Cancel;
using BanCoreBot.Dialogs.DatosIniciales;
using BanCoreBot.Dialogs.Reclamos;
using BanCoreBot.Infrastructure.Luis;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BanCoreBot.Dialogs.Pos
{
    public class InitReclamoPOS : CancelDialog
    {
        private ILuisService _luisService;
        private const string _ShowOptions = "ShowOptions";
        private const string _ValidateShowOptions = "ValidateShowOptions";
        private const string _Next = "Next";
        public InitReclamoPOS(ILuisService luisService) : base(nameof(InitReclamoPOS))
        {
            _luisService = luisService;
            var waterfallSteps = new WaterfallStep[]
            {
                ShowOptions,
                ValidateShowOptions,
                NextCall
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
            if (luisResult.TopIntent().intent.ToString().Equals("afirmacion"))
            {
                await stepContext.Context.SendActivityAsync("Me gustaría tener mayor información, por ello te consulto lo siguiente", cancellationToken: cancellationToken);
                return await stepContext.BeginDialogAsync(nameof(DatosInic), cancellationToken: cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync("Entiendo, brindame más información de lo que deseas para ayudarte", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
        }


        private async  Task<DialogTurnResult> NextCall(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(ReclamoPOSFromAllClaims), cancellationToken: cancellationToken);
        }

        #endregion DialogCtas


        #region Validators


        private Task<bool> ValidateInputValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(promptContext.Context, cancellationToken).Result;
            if (
                luisResult.TopIntent().intent.ToString().Equals("afirmacion")
               || luisResult.TopIntent().intent.ToString().Equals("negación")
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
                reply = MessageFactory.Text("¿Me quieres decir que presentas algún problema con tu tarjeta de débito o Tarjeta de crédito al momento de hacer un consumo en punto de venta?");
            }
            else
            {
                reply = MessageFactory.Text("¿Presentaste problemas para realizar un pago en un punto de venta?");
            }

            return reply as Activity;
        }

        #endregion Buttons

    }
}
