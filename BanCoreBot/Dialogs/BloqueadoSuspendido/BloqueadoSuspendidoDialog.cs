using BanCoreBot.Dialogs.Cancel;
using BanCoreBot.Infrastructure.Luis;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Threading;
using System.Threading.Tasks;
using BanCoreBot.Common.Models.User;
using BanCoreBot.Dialogs.Tarjetas;

namespace BanCoreBot.Dialogs.BloqueadoSuspendido
{
    public class BloqueadoSuspendidoDialog : CancelDialog
    {
        private BotState _userState;
        private ILuisService _luisService;
        private const string _ShowOptions = "ShowOptions";
        private const string _ValidateShowOptions = "ValidateShowOptions";
        private UserPersonalData ClientData;

        public BloqueadoSuspendidoDialog(ILuisService luisService, PrivateConversationState userState) : base(nameof(BloqueadoSuspendidoDialog))
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
                    Prompt = await CreateButtons("",  stepContext),
                    RetryPrompt = await CreateButtons("Reintento",  stepContext)
                }, cancellationToken
            );
            return option;
        }

        private async Task<DialogTurnResult> ValidateShowOptions(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());

            var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(stepContext.Context, cancellationToken).Result;
            if (luisResult.TopIntent().intent.ToString().Equals("afirmacion"))
            {
                if (ClientData.request.Equals("usuariosusponline"))
                {
                    ClientData.request = "usuariosuspendido";
                    return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
                }
                else if (ClientData.request.Equals("usuariobloqueado"))
                {
                    return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
                }
                else if (ClientData.request.Equals("usuariobloqueadonat"))
                {
                    await stepContext.Context.SendActivityAsync($"De ser así, te recuerdo que puedes ingresar a mi conexión bancaribe, persona natural y por ultimo usuario bloqueado. Responde las preguntas que te solicitan y solventaras sin problemas", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
                else if (ClientData.request.Equals("usuariobloqueadojur"))
                {
                    await stepContext.Context.SendActivityAsync($"Si tienes a la mano la tarjeta de conexión segura y tu carta serial, te sugiero que ingreses a Mi Conexión Bancaribe, Persona Jurídica, Cambiar/Recuperar contraseña. Responde las pregunta que te solicita el sistema y solventaras el bloqueo", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync($"De ser así, te recuerdo que puede ingresar a Mi conexión Bancaribe , Persona Natural, colocar tu usuario, la última clave que recuerdas y tilde la opción Ingresar. Automáticamente el sistema te enviara a una nueva ventana donde tendrás que presionar la opción \"Aquí\". Esto eliminara el usuario y tendrás que ingresar por la opción Cliente Nuevo ", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
            }
            else
            {
                await stepContext.Context.SendActivityAsync("¿Puedes ser un poco más específico? No logro entender que me estas solicitando o preguntando", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
        }

        #endregion DialogCtas


        #region Validators


        private Task<bool> ValidateInputValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(promptContext.Context, cancellationToken).Result;
            if (
                luisResult.TopIntent().intent.ToString().Equals("afirmacion")
               || luisResult.TopIntent().intent.ToString().Equals("negación")

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

        private async Task<Activity> CreateButtons(string retry, WaterfallStepContext stepContext)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            var reply = MessageFactory.Text("");

            if (ClientData.request.Equals("usuariobloqueado") || ClientData.request.Equals("usuariobloqueadonat")
                || ClientData.request.Equals("usuariobloqueadojur"))
            {

                if (String.IsNullOrEmpty(retry))
                {
                    reply = MessageFactory.Text("¿Me quieres decir que tu usuario se encuentra bloqueado?");
                }
                else
                {
                    reply = MessageFactory.Text("¿Lo que intentas decirme es que tu usuario está bloqueado?");
                }
            }
            else
            {

                if (String.IsNullOrEmpty(retry))
                {
                    reply = MessageFactory.Text("¿Me quieres decir que tu usuario se encuentra suspendido?");
                }
                else
                {
                    reply = MessageFactory.Text("¿Lo que intentas decirme es que tu usuario está suspendido?");
                }
            }

            return reply as Activity;
        }

        #endregion Buttons

    }
}
