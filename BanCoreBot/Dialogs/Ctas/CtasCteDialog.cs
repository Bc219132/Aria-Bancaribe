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

    public class CtasCteDialog : CancelDialog
    {

        private const string _ShowOptionsCte = "ShowOptionsCte";
        private const string _ValidateOptionCte = "ValidateOptionCte";
        public CtasCteDialog() : base(nameof(CtasCteDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                ShowOptionsCte,
                ValidateOptionCte
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(_ShowOptionsCte, ShowOptionsCteValidator));
            AddDialog(new TextPrompt(_ValidateOptionCte));
        }


        #region DialogCtas
        private async Task<DialogTurnResult> ShowOptionsCte(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var option = await stepContext.PromptAsync(
                _ShowOptionsCte,
                new PromptOptions
                {
                    Prompt = CreateButtonsCtasCte(""),
                    RetryPrompt = CreateButtonsCtasCte("Reintento")
                }, cancellationToken
            );
            return option;
        }

        private async Task<DialogTurnResult> ValidateOptionCte(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Context.Activity.Text.Equals("Mi Cuenta Corriente Remunerada"))
            {
                await stepContext.Context.SendActivityAsync("Deberá reunir todos los recaudos, ingresar al [link](https://www.bancaribe.com.ve/zona-de-informacion-para-cada-mercado/personas/cuentas-personas/cuentas-corrientes/mi-cuenta-corriente-remunerada) y hacer clic en ¡Solicítala ya en línea!.", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync("Deberá reunir todos los recaudos, ingresar al [link](https://www.bancaribe.com.ve/zona-de-informacion-para-cada-mercado/personas/solicitud-mi-cuenta-corriente-bancaribe) y hacer clic en ¡Solicítala ya en línea!.", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
        }

        #endregion DialogCtas


        #region Validators


        private Task<bool> ShowOptionsCteValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Context.Activity.Text.ToLower().Equals("mi cuenta corriente remunerada") ||
                promptContext.Context.Activity.Text.ToLower().Equals("mi cuenta corriente"))
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



        private Activity CreateButtonsCtasCte(string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                reply = MessageFactory.Text("Seleccione el tipo de cuenta corriente:");
            }
            else
            {
                reply = MessageFactory.Text("Por favor, seleccione una cuenta corriente de estas opciones:");
            }

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() {Title = "Mi Cuenta Corriente", Value = "Mi Cuenta Corriente" , Type = ActionTypes.ImBack},
                    new CardAction() {Title = "Mi Cuenta Corriente Remunerada", Value = "Mi Cuenta Corriente Remunerada" , Type = ActionTypes.ImBack}
                }
            };
            return reply as Activity;
        }

        #endregion Buttons

    }
}
