using BanCoreBot.Common.Models.User;
using BanCoreBot.Dialogs.Cancel;
using BanCoreBot.Infrastructure.Luis;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BanCoreBot.Dialogs.Reclamos
{
    public class UsoPagoTDCDialog : CancelDialog
    {
        private BotState _userState;
        private ILuisService _luisService;
        private const string _ShowOptions = "ShowOptions";
        private const string _ValidateShowOptions = "ValidateShowOptions";
        private UserPersonalData ClientData;

        public UsoPagoTDCDialog(ILuisService luisService, PrivateConversationState userState) : base(nameof(UsoPagoTDCDialog))
        {
            _userState = userState;
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
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            if (!String.IsNullOrEmpty(ClientData.request))
            {
                if (ClientData.request.Equals("reclamopos"))
                {
                    await stepContext.Context.SendActivityAsync("Si deseas realizar un reclamo relacionado con el uso de tu tarjeta de crédito, ingresa al siguiente [link](https://www.bancaribe.com.ve/zona-de-informacion-para-cada-mercado/personas/tarjetas-y-extrafinanciamento-personas/tarjetas-de-credito-personas/reclamos#tabs-1721-0-3) y sigue las instrucciones", cancellationToken: cancellationToken);
                    await stepContext.Context.SendActivityAsync("Posteriormente envía un correo con la documentación solicitada a servicios_conr@bancaribe.com.ve, " +
                        "una vez formalizado el reclamo, recibirás una notificación con tu número de reporte.", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
            }
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
            if (stepContext.Context.Activity.Text.ToLower().Equals("pagando la tarjeta")
                || luisResult.Entities.transferir != null)
            {
                await stepContext.Context.SendActivityAsync($"Si posees algún débito o se te fue descontado de manera errada un(os) monto(s) de tu(s) cuenta(s), puedes realizar el reporte en nuestra pagina principal " +
                   $"a través del \"Formulario de Atención\" y luego validar los datos que te solicitan. Recuerda dar la mayor información de lo sucedido y seleccionar de manera correcta " +
                   $"el tipo de reclamo. Este es el [enlace](https://www.bancaribe.com.ve/informacion-vital/atencion-al-cliente/formulario-unico-de-reclamos-de-atencion-al-cliente) del formulario de atención", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync("Si deseas realizar un reclamo relacionado con el uso de tu tarjeta de crédito, ingresa al siguiente [link](https://www.bancaribe.com.ve/zona-de-informacion-para-cada-mercado/personas/tarjetas-y-extrafinanciamento-personas/tarjetas-de-credito-personas/reclamos#tabs-1721-0-3) y sigue las instrucciones", cancellationToken: cancellationToken);
                await stepContext.Context.SendActivityAsync("Posteriormente envía un correo con la documentación solicitada a servicios_conr@bancaribe.com.ve, " +
                    "una vez formalizado el reclamo, recibirás una notificación con tu número de reporte.", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
        }

        #endregion DialogCtas


        #region Validators


        private Task<bool> ValidateInputValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var result = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(promptContext.Context, cancellationToken).Result;
            if (
                result.Entities.transferir != null
               || result.Entities.pos != null
               || promptContext.Context.Activity.Text.ToLower().Equals("al usar la tarjeta")
               || promptContext.Context.Activity.Text.ToLower().Equals("pagando la tarjeta")
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
                reply = MessageFactory.Text("¿El inconveniente lo presentas al usar tu tarjeta de crédito o al realizar pagos a la tarjeta de crédito?");
            }
            else
            {
                reply = MessageFactory.Text("Por favor, ¿me confirmas si el inconveniente lo presentas al usar tu tarjeta de crédito o al realizar pagos a la tarjeta de crédito?");
            }

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() {Title = "Al usar la Tarjeta", Value = "Al usar la Tarjeta" , Type = ActionTypes.ImBack},
                    new CardAction() {Title = "Pagando la Tarjeta", Value = "Pagando la Tarjeta" , Type = ActionTypes.ImBack}
                }
            };
            return reply as Activity;
        }



        #endregion Buttons

    }
}
