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
    public class AfiliarmeMiPago : CancelDialog
    {
        private readonly ILuisService _luisService;
        private const string _ValidateInput = "ValidateInput";
        private const string _ValidateOption = "ValidateOption";
        private const string _ShowOptionsTlf = "ShowOptionsTlf";
        private const string _ValidateShowOptionsTlf = "ValidateShowOptionsTlf";
        public AfiliarmeMiPago(ILuisService luisService) : base(nameof(AfiliarmeMiPago))
        {
            _luisService = luisService;
            var waterfallSteps = new WaterfallStep[]
            {
                ValidateInput,
                ValidateOption,
                ShowOptionsTlf,
                ValidateShowOptionsTlf
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(_ValidateInput, ValidateInputValidator));
            AddDialog(new TextPrompt(_ValidateOption));
            AddDialog(new TextPrompt(_ShowOptionsTlf, ValidateInputValidator));
            AddDialog(new TextPrompt(_ValidateShowOptionsTlf));
        }


        #region DialogCtas
        private async Task<DialogTurnResult> ValidateInput(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(
                _ValidateInput,
                new PromptOptions
                {
                    Prompt = CreateButtonsOptions(""),
                    RetryPrompt = CreateButtonsOptions("Reintento")
                }, cancellationToken
            );
        }



        private async Task<DialogTurnResult> ValidateOption(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var option = stepContext.Context.Activity.Text;
            if (option.ToLower().Equals("si"))
            {
                return await stepContext.NextAsync(stepContext, cancellationToken);
            }
            else
            {
                var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(stepContext.Context, cancellationToken).Result;
                if (luisResult.TopIntent().intent.ToString().Equals("afirmacion"))
                {
                    return await stepContext.NextAsync(stepContext, cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync("Entiendo, si hay algo en lo que te pueda ayudar solo me lo debes indicar y con gusto te ayudaré", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
            }
        }


        private async Task<DialogTurnResult> ShowOptionsTlf(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var option = await stepContext.PromptAsync(
                _ShowOptionsTlf,
                new PromptOptions
                {
                    Prompt = CreateButtonsTlf(""),
                    RetryPrompt = CreateButtonsTlf("Reintento")
                }, cancellationToken
            );
            return option;
        }

        private async Task<DialogTurnResult> ValidateShowOptionsTlf(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var option = stepContext.Context.Activity.Text;
            if (option.Equals("Si"))
            {
                await stepContext.Context.SendActivityAsync("En este caso, descarga la aplicación \"Mi Pago Bancaribe\", tilda el icono que te indica \"Afiliación\", completa los datos de registro y confirma los datos pulsando la opción \"Aceptar\"", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
            else
            {
                var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(stepContext.Context, cancellationToken).Result;
                if (luisResult.TopIntent().intent.ToString().Equals("afirmacion"))
                {
                    await stepContext.Context.SendActivityAsync("En este caso, descarga la aplicación \"Mi Pago Bancaribe\", tilda el icono que te indica \"Afiliación\", completa los datos de registro y confirma los datos pulsando la opción \"Aceptar\"", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync("En este caso, Ingresa a \"Mi Conexión Bancaribe\", tilda la opción \"Servicio al Cliente\", luego Mi pago Bancaribe, completa los datos de registro y por último confirma los datos pulsando la opción \"Aceptar\" ", cancellationToken: cancellationToken);
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

        private Activity CreateButtonsOptions(string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                reply = MessageFactory.Text("Me quieres decir  ¿Qué deseas Afiliarte a Mi Pago Bancaribe? ");
            }
            else
            {
                reply = MessageFactory.Text("Por favor, me confirmas si deseas conocer como afiliarte a mi Pago Bancaribe");
            }

            return reply as Activity;
        }



        private Activity CreateButtonsTlf(string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                reply = MessageFactory.Text("¿Posees teléfono inteligente?");
            }
            else
            {
                reply = MessageFactory.Text("Por favor, me confirmas si posees teléfono inteligente");
            }

            return reply as Activity;
        }

        #endregion Buttons

    }
}
