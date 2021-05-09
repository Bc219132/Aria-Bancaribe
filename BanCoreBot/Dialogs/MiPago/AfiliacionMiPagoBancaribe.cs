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
    public class AfiliacionMiPagoBancaribe : CancelDialog
    {
        private  ILuisService _luisService;
        private const string _ShowOptionsTlf = "ShowOptionsTlf";
        private const string _ValidateShowOptionsTlf = "ValidateShowOptionsTlf";
        public AfiliacionMiPagoBancaribe(ILuisService luisService) : base(nameof(AfiliacionMiPagoBancaribe))
        {
            _luisService = luisService;
            var waterfallSteps = new WaterfallStep[]
            {
                ShowOptionsTlf,
                ValidateShowOptionsTlf
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(_ShowOptionsTlf, ValidateInputValidator));
            AddDialog(new TextPrompt(_ValidateShowOptionsTlf));
        }


        #region DialogCtas

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
