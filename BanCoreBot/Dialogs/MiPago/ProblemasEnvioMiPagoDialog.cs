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
    public class ProblemasEnvioMiPagoDialog : CancelDialog
    {
        private ILuisService _luisService;
        private const string _ShowOptions = "ShowOptions";
        private const string _ValidateShowOptions = "ValidateShowOptions";
        public ProblemasEnvioMiPagoDialog(ILuisService luisService) : base(nameof(ProblemasEnvioMiPagoDialog))
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
            await stepContext.Context.SendActivityAsync($"Te recomiendo validar si estas afiliado a Mi Pago Bancaribe, de no estarlo no podrás gozar de este servicio. También te recuerdo que si recientemente modificaste el número de teléfono afiliado al Perfil de Seguridad, tendrás que volver hacer la afiliación al Pago Bancaribe.", cancellationToken: cancellationToken);

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
                await stepContext.Context.SendActivityAsync($"Recuerda que para afiliarte al servicio, tienes que poseer un número registrado al Perfil de Seguridad, este número es el que toma el sistema cuando te afilias a Mi Pago Bancaribe. Al modificarlo, la última afiliación al Pago Bancaribe conserva el número anterior y esto genera inconvenientes con los datos ya afiliados.", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
            else
            {
                var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(stepContext.Context, cancellationToken).Result;
                if (luisResult.TopIntent().intent.ToString().Equals("afirmacion"))
                {
                    await stepContext.Context.SendActivityAsync($"Recuerda que para afiliarte al servicio, tienes que poseer un número registrado al Perfil de Seguridad, este número es el que toma el sistema cuando te afilias a Mi Pago Bancaribe. Al modificarlo, la última afiliación al Pago Bancaribe conserva el número anterior y esto genera inconvenientes con los datos ya afiliados.", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync("Entiendo, ¿existe algo más en que te pueda ayudar?", cancellationToken: cancellationToken);
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
                reply = MessageFactory.Text("¿Quieres saber más información?");
            }
            else
            {
                reply = MessageFactory.Text("Por favor, me confirmas si ¿deseas más información?");
            }

            return reply as Activity;
        }

        #endregion Buttons

    }
}
