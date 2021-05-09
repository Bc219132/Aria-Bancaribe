using BanCoreBot.Common.Models.User;
using BanCoreBot.Dialogs.Cancel;
using BanCoreBot.Infrastructure.Luis;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Threading;
using BanCoreBot.Common.Utils;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BanCoreBot.Dialogs.Reclamos
{
    public class FechaTransferenciaReclamo : CancelDialog
    {
        private BotState _userState;
        private ILuisService _luisService;
        private const string _ShowOptions = "ShowOptions";
        private const string _ValidateShowOptions = "ValidateShowOptions";
        private const string _ShowOptionsTransf = "ShowOptionsTransf";
        private const string _ValidateShowOptionsTransf = "ValidateShowOptionsTransf";
        private Utils utilitario = new Utils();
        private UserPersonalData ClientData;

        public FechaTransferenciaReclamo(PrivateConversationState userState, ILuisService luisService) : base(nameof(FechaTransferenciaReclamo))
        {
            _userState = userState;
            _luisService = luisService;
            var waterfallSteps = new WaterfallStep[]
            {
                ShowOptions,
                ValidateShowOptions,
                ShowOptionsTransf,
                ValidateShowOptionsTransf
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(_ShowOptions, ValidateInputValidator));
            AddDialog(new TextPrompt(_ValidateShowOptions));
            AddDialog(new TextPrompt(_ShowOptionsTransf, ValidateInputValidatorTransf));
            AddDialog(new TextPrompt(_ValidateShowOptionsTransf));
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
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());

            var validate = utilitario.ValidateDate72h(stepContext.Context.Activity.Text);
            if (validate == 1)
            {
                ClientData.controlFecha72h = 1;
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
            else
            {
                ClientData.controlFecha72h = 0;
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ShowOptionsTransf(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var option = await stepContext.PromptAsync(
                _ShowOptionsTransf,
                new PromptOptions
                {
                    Prompt = CreateButtonsTransf(""),
                    RetryPrompt = CreateButtonsTransf("Reintento")
                }, cancellationToken
            );
            return option;
        }

        private async Task<DialogTurnResult> ValidateShowOptionsTransf(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            if(stepContext.Context.Activity.Text.ToLower().Equals("yo la envié"))
            {
                if(ClientData.controlFecha72h == 0)
                {

                    await stepContext.Context.SendActivityAsync($"Puedes realizar el reporte en nuestra página principal a través del \"Formulario de Atención\" y luego validar los " +
                        $"datos que te solicitan. Recuerda dar la mayor información de lo sucedido y seleccionar de manera correcta el tipo de reclamo. " +
                        $"Este es el [enlace](https://www.bancaribe.com.ve/informacion-vital/atencion-al-cliente/formulario-unico-de-reclamos-de-atencion-al-cliente) del formulario de atención.");
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync($"Recuerda que para que se haga efectiva una transferencia a otro banco, el plazo es de " +
                        $"24 a 72 horas hábiles. Si la transferencia se realizó antes de la 9:00 am, suele ser abonado el mismo día o al día hábil siguiente, " +
                        $"en el caso contrario puede tardar hasta 72 horas hábiles");
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
            }
            else
            {
                if (ClientData.controlFecha72h == 0)
                {
                    await stepContext.Context.SendActivityAsync($"Valida si la transferencia fue enviada de manera correcta con la persona que la realizó. " +
                        $"De haber sido exitosa, te recomiendo que esta persona valide la transacción con su entidad Bancaria, " +
                        $"del mismo modo verifica en tus movimientos y asegúrate de que no fue abonada. ");
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync($"Recuerda que una transferencia de otro banco para ser abonada en la cuenta, debe esperar " +
                        $"un plazo promedio de 24 a 72 horas hábiles. Si se realizó la transferencia antes de la 9:00 am, suele ser abonado el mismo día o " +
                        $"al día siguiente, en el caso contrario puede tardar hasta 72 horas hábiles");
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }


            }
        }


        #endregion DialogCtas


        #region Validators


        private Task<bool> ValidateInputValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var validate = utilitario.ValidateDate72h(promptContext.Context.Activity.Text);
            if (validate == 0 || validate == 1)
            {
                return Task.FromResult(true);
            }
            else if (validate == 2)
            {
                promptContext.Context.SendActivityAsync($"El formato de la fecha ingresada es incorrecto");
                return Task.FromResult(false);
            }
            else if (validate == 3)
            {
                promptContext.Context.SendActivityAsync($"ingresaste una fecha que corresponde al futuro");
                return Task.FromResult(false);
            }
            else
            {
                return Task.FromResult(false);
            }

        }


        private Task<bool> ValidateInputValidatorTransf(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            if ( promptContext.Context.Activity.Text.ToLower().Equals("yo la envié") || promptContext.Context.Activity.Text.ToLower().Equals("yo recibo") )
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
                reply = MessageFactory.Text("Por favor ingresa la fecha en la cual se realizó la transferencia en el formato \"dd/mm/aaaa \"");
            }
            else
            {
                reply = MessageFactory.Text("Requiero que ingreses la fecha en la cual fue realizada la transferencia en el formato \"dd/mm/aaaa \"  ejemplo: 10/12/2019");
            }

            return reply as Activity;
        }


        private Activity CreateButtonsTransf(string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                reply = MessageFactory.Text("¿Enviaste la transferencia o esperas recibirla?");
            }
            else
            {
                reply = MessageFactory.Text("Por favor me confirmas si fuiste tú quien envió la transferencia o eres quien va a recibir la transferencia");
            }
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() {Title = "Yo la envié", Value = "Yo la envié" , Type = ActionTypes.ImBack},
                    new CardAction() {Title = "Yo recibo", Value = "Yo recibo" , Type = ActionTypes.ImBack}
                }
            };
            return reply as Activity;
        }

        

        #endregion Buttons

    }
}
