using BanCoreBot.Common.Models.User;
using BanCoreBot.Dialogs.Cancel;
using BanCoreBot.Dialogs.DatosIniciales;
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
    public class AllClaims : CancelDialog
    {
        private BotState _userState;
        private ILuisService _luisService;
        private const string _CrearReclamo = "CrearReclamo";
        private const string _SetTipoReclamo = "SetTipoReclamo";
        private const string _SetDatosIniciales = "SetDatosIniciales";
        private const string _SetRoute = "SetRoute";
        private UserPersonalData ClientData;


        public AllClaims(ILuisService luisService, PrivateConversationState userState)
            : base(nameof(AllClaims))
        {
            _userState = userState;
            _luisService = luisService;
            var waterfallStep = new WaterfallStep[]
            {
                CrearReclamo,
                SetTipoReclamo,
               // SetDatosIniciales,
                SetRoute
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallStep));
            AddDialog(new TextPrompt(_CrearReclamo, CrearReclamoValidator));
            AddDialog(new TextPrompt(_SetTipoReclamo, SetTipoReclamoValidator));
            //AddDialog(new TextPrompt(_SetDatosIniciales));
            AddDialog(new TextPrompt(_SetRoute));
        }



        #region allClaims
        private async Task<DialogTurnResult> CrearReclamo(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
             var option = await stepContext.PromptAsync(
                _CrearReclamo,
                new PromptOptions
                {
                    Prompt = CreateButtonsClaims(""),
                    RetryPrompt = CreateButtonsClaims("Reintento")
                }, cancellationToken
            );
            return option;
        }

        private async Task<DialogTurnResult> SetTipoReclamo(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(stepContext.Context, cancellationToken).Result;

            if (!luisResult.TopIntent().intent.ToString().Equals("negación"))
            {
                return  await stepContext.PromptAsync(
                       _SetTipoReclamo,
                       new PromptOptions
                       {
                           Prompt = CreateButtonsOptionsClaims(""),
                           RetryPrompt = CreateButtonsOptionsClaims("Reintento")
                       }, cancellationToken
                   );
            }
            else
            {
                await stepContext.Context.SendActivityAsync($"Entiendo, ¿te puedo ayudar en algo más?", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
        }


        /*private async Task<DialogTurnResult> SetDatosIniciales(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            ClientData.typeClaim = stepContext.Context.Activity.Text.ToLower();
            //await _userState.SaveChangesAsync(stepContext.Context, false, cancellationToken);
            return await stepContext.BeginDialogAsync(nameof(DatosInic), cancellationToken: cancellationToken);
        }*/

        private async Task<DialogTurnResult> SetRoute(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            if (stepContext.Context.Activity.Text.ToLower().Equals("punto de venta"))
            {
                ClientData.request = "reclamopos";
                return await stepContext.BeginDialogAsync(nameof(IsTDCDialog), cancellationToken: cancellationToken);
            }
            else if (stepContext.Context.Activity.Text.ToLower().Equals("mi pago bancaribe") || stepContext.Context.Activity.Text.ToLower().Equals("cajero automático")
                || stepContext.Context.Activity.Text.ToLower().Equals("recarga telefónica"))
            {
                await stepContext.Context.SendActivityAsync($"Si posees algún débito o se te fue descontado de manera errada un(os) monto(s) de tu(s) cuenta(s), puedes realizar el reporte en nuestra pagina principal " +
                 $"a través del \"Formulario de Atención\" y luego validar los datos que te solicitan. Recuerda dar la mayor información de lo sucedido y seleccionar de manera correcta " +
                 $"el tipo de reclamo. Este es el [enlace](https://www.bancaribe.com.ve/informacion-vital/atencion-al-cliente/formulario-unico-de-reclamos-de-atencion-al-cliente) del formulario de atención", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            

            /*
            else if (ClientData.typeClaim.Equals("mi pago bancaribe"))
            {
                return await stepContext.BeginDialogAsync(nameof(ReclamoMiPagoFromAllClaims), cancellationToken: cancellationToken);
            }else if (ClientData.typeClaim.Equals("recarga telefónica")) 
            {
                return await stepContext.BeginDialogAsync(nameof(ReclamoRecargaFromAllClaims), cancellationToken: cancellationToken);
            }
            else if (ClientData.typeClaim.Equals("cajero automático")) 
            {
                return await stepContext.BeginDialogAsync(nameof(ReclamoCajeroFromAllClaims), cancellationToken: cancellationToken);
            }*/

            else if (stepContext.Context.Activity.Text.ToLower().Equals("tarjeta de crédito"))
            {
                return await stepContext.BeginDialogAsync(nameof(UsoPagoTDCDialog), cancellationToken: cancellationToken);
            }
            else if (stepContext.Context.Activity.Text.ToLower().Equals("transferencia"))
            {
                return await stepContext.BeginDialogAsync(nameof(FechaTransferenciaReclamo), cancellationToken: cancellationToken);
            }
            else
            {
                return await stepContext.ContinueDialogAsync();
            }
        }

        #endregion allClaims

        #region Buttons
        private Activity CreateButtonsClaims(string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                reply = MessageFactory.Text("¿Deseas crear un reclamo?");
            }
            else
            {
                reply = MessageFactory.Text("Por favor indique una respuesta, ¿Deseas crear un reclamo?");
            }
            return reply as Activity;
        }

        private Activity CreateButtonsOptionsClaims(string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                reply = MessageFactory.Text("Seleccione el tipo de reclamo que desea crear");
            }
            else
            {
                reply = MessageFactory.Text("Por favor indique una respuesta seleccionando la opción que este asociada al tipo de reclamo que desea crear:");
            }

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() {Title = "Punto de Venta", Value = "Punto de Venta" , Type = ActionTypes.ImBack},
                    new CardAction() {Title = "Mi Pago Bancaribe", Value = "Mi Pago Bancaribe" , Type = ActionTypes.ImBack},
                    new CardAction() {Title = "Recarga Telefónica", Value = "Recarga Telefónica" , Type = ActionTypes.ImBack},
                    new CardAction() {Title = "Cajero Automático", Value = "Cajero Automático" , Type = ActionTypes.ImBack},
                    new CardAction() {Title = "Tarjeta de Crédito", Value = "Tarjeta de Crédito" , Type = ActionTypes.ImBack},
                    new CardAction() {Title = "Transferencia", Value = "Transferencia" , Type = ActionTypes.ImBack}
                }
            };
            return reply as Activity;
        }

        #endregion Buttons


        #region Validators

        private Task<bool> SetTipoReclamoValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Context.Activity.Text.ToLower().Trim().Equals("punto de venta") ||
                promptContext.Context.Activity.Text.ToLower().Trim().Equals("mi pago bancaribe") ||
                promptContext.Context.Activity.Text.ToLower().Trim().Equals("recarga telefónica") ||
                promptContext.Context.Activity.Text.ToLower().Trim().Equals("cajero automático") ||
                promptContext.Context.Activity.Text.ToLower().Trim().Equals("tarjeta de crédito") ||
                promptContext.Context.Activity.Text.ToLower().Trim().Equals("transferencia")) 
            {
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }

        private Task<bool> CrearReclamoValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var result = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(promptContext.Context, cancellationToken).Result;
            if (
                result.TopIntent().intent.ToString().Equals("afirmacion")
               || result.TopIntent().intent.ToString().Equals("negación")
               || result.Entities.pagorapido != null
               || result.Entities.mipagobancaribe != null
               || result.Entities.transferir != null
               || result.Entities.pos != null
               || result.Entities.Smartphone != null
               || result.Entities.tlfbasico != null
               || result.Entities.tdd != null
               || result.Entities.tdc != null
               || result.Entities.sms != null
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

    }
}

