using BanCoreBot.Common.Models.User;
using BanCoreBot.Dialogs.Cancel;
using BanCoreBot.Infrastructure.Luis;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BanCoreBot.Dialogs.Reclamos
{
    public class IsTDCDialog : CancelDialog
    {
        private BotState _userState;
        private ILuisService _luisService;
        private const string _ShowOptions = "ShowOptions";
        private const string _ValidateShowOptions = "ValidateShowOptions";
        private UserPersonalData ClientData;

        public IsTDCDialog(ILuisService luisService, PrivateConversationState userState) : base(nameof(IsTDCDialog))
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
            if (luisResult.TopIntent().intent.ToString().Equals("afirmacion") || luisResult.Entities.tdc != null)
            {
                return await stepContext.BeginDialogAsync(nameof(UsoPagoTDCDialog), cancellationToken: cancellationToken);
            }
            else
            {

                var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
                ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
                if (!String.IsNullOrEmpty(ClientData.request))
                {
                    if (ClientData.request.Equals("reclamopos"))
                    {
                        await stepContext.Context.SendActivityAsync($"Si posees algún débito o se te fue descontado de manera errada un(os) monto(s) de tu(s) cuenta(s), puedes realizar " +
                            $"el reporte a través del \"Formulario de Atención\" en nuestra página Bancaribe, luego validar los datos que te solicitan. Recuerda " +
                            $"dar la mayor información de lo sucedido, colocando si te lograste llevar la compra y cuántas veces fue debitado por el mismo valor y " +
                            $"seleccione de manera correcta el tipo de reclamo. " +
                            $"Haz clic [aquí](https://www.bancaribe.com.ve/informacion-vital/atencion-al-cliente/formulario-unico-de-reclamos-de-atencion-al-cliente) para ir al formulario de atención", cancellationToken: cancellationToken);
                        ClientData.request = "";
                        return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync($"Si posees algún débito o se te fue descontado de manera errada un(os) monto(s) de tu(s) cuenta(s), puedes realizar el reporte en nuestra pagina principal " +
                             $"a través del \"Formulario de Atención\" y luego validar los datos que te solicitan. Recuerda dar la mayor información de lo sucedido y seleccionar de manera correcta " +
                             $"el tipo de reclamo. Este es el [enlace](https://www.bancaribe.com.ve/informacion-vital/atencion-al-cliente/formulario-unico-de-reclamos-de-atencion-al-cliente) del formulario de atención", cancellationToken: cancellationToken);
                        ClientData.request = "";
                        return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                    }
                }
                else
                {
                    await stepContext.Context.SendActivityAsync($"Si posees algún débito o se te fue descontado de manera errada un(os) monto(s) de tu(s) cuenta(s), puedes realizar el reporte en nuestra pagina principal " +
                        $"a través del \"Formulario de Atención\" y luego validar los datos que te solicitan. Recuerda dar la mayor información de lo sucedido y seleccionar de manera correcta " +
                        $"el tipo de reclamo. Este es el [enlace](https://www.bancaribe.com.ve/informacion-vital/atencion-al-cliente/formulario-unico-de-reclamos-de-atencion-al-cliente) del formulario de atención", cancellationToken: cancellationToken);
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

        #region Buttons

        private Activity CreateButtons(string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                reply = MessageFactory.Text("¿Su reclamo está relacionado con tarjetas de crédito?");
            }
            else
            {
                reply = MessageFactory.Text("Por favor, ¿me confirmas si tu reclamo está relacionado con tarjetas de crédito?");
            }

            return reply as Activity;
        }

        #endregion Buttons

    }
}
