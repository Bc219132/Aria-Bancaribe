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
    public class MontoMaximoTarjeta : CancelDialog
    {
        private ILuisService _luisService;
        private const string _ShowOptions = "ShowOptions";
        private const string _ValidateShowOptions = "ValidateShowOptions";
        public MontoMaximoTarjeta(ILuisService luisService) : base(nameof(MontoMaximoTarjeta))
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
            if (luisResult.Entities.tdd != null || luisResult.Entities.opcion2List != null || luisResult.Entities.opcionultimaList != null)
            {
                await stepContext.Context.SendActivityAsync($"Hasta la fecha el Máximo Diario para hacer uso de su Tarjeta de Débito por Punto de Venta es: 1.000.000.000,00BsS", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync($"En la Tarjeta de Crédito varia el disponible que se puede usar por Punto de Venta, eso dependerá del Límite que posea tu Tarjeta. ", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }

        }
        #endregion DialogCtas


        #region Validators


        private Task<bool> ValidateInputValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var luisresult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(promptContext.Context, cancellationToken).Result;
            if (
                luisresult.TopIntent().intent.ToString().Equals("TipoTarjeta")
                ||
                luisresult.TopIntent().intent.ToString().Equals("Opciones")
                ||
                luisresult.Entities.tdd != null || luisresult.Entities.tdc != null
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
                reply = MessageFactory.Text("¿Deseas saber esta información de una tarjeta de débito o una tarjeta de crédito?");
            }
            else
            {
                reply = MessageFactory.Text("Por favor, me indicas el tipo de tarjeta (débito o crédito) para poder darte el monto máximo a consumir");
            }

            return reply as Activity;
        }

        #endregion Buttons

    }
}
