using BanCoreBot.Dialogs.Cancel;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BanCoreBot.Dialogs.Ctas
{
    public class ErrorConexDialog : CancelDialog
    {

        private const string _ShowOptionsType = "ShowOptionsType";
        private const string _ValidateOptionType = "ValidateOptionType";
        public ErrorConexDialog() : base(nameof(ErrorConexDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                ShowOptionsType,
                ValidateOptionType
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(_ShowOptionsType, ShowOptionsTypeValidator));
            AddDialog(new TextPrompt(_ValidateOptionType));
        }


        #region DialogError
        private async Task<DialogTurnResult> ShowOptionsType(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var option = await stepContext.PromptAsync(
                _ShowOptionsType,
                new PromptOptions
                {
                    Prompt = CreateButtonsType(""),
                    RetryPrompt = CreateButtonsType("Reintento")
                }, cancellationToken
            );
            return option;
        }

        private async Task<DialogTurnResult> ValidateOptionType(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Context.Activity.Text.ToLower().Equals("si"))
            {
                await stepContext.Context.SendActivityAsync("Debe comunicarse al siguiente número: 0501.999.99.99 marcando las opciones  para hablar con un operador y presentar el inconveniente que tiene al ingresar.", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync($"Le recomendamos eliminar los cookies o archivos temporales de tu navegador  o en su defecto intente el ingreso  {Environment.NewLine} " +
                $"por un navegador distinto, en dado caso que persista el error, debe enviarnos el print de pantalla a la siguiente  {Environment.NewLine}" +
                $"dirección de correo electrónico: ventastelefonicas@bancaribe.com.ve, con sus datos personales y números telefónicos  {Environment.NewLine}" +
                $"de fácil contacto para validar la información.", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
        }

        #endregion DialogError


        #region Validators

        private Task<bool> ShowOptionsTypeValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Context.Activity.Text.ToLower().Equals("si") ||
                promptContext.Context.Activity.Text.ToLower().Equals("no"))
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



        private Activity CreateButtonsType(string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                reply = MessageFactory.Text(" Para ayudarte necesito que me respondas lo siguiente: ¿Ústed posee solo tarjetas de crédito con la institución?");
            }
            else
            {
                reply = MessageFactory.Text("Por favor indique una respuesta,  ¿ústed posee solo tarjeta de crédito con la institución?:");
            }

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() {Title = "Si", Value = "Si" , Type = ActionTypes.ImBack},
                    new CardAction() {Title = "No", Value = "No" , Type = ActionTypes.ImBack}
                }
            };
            return reply as Activity;
        }

        #endregion Buttons

    }
}
