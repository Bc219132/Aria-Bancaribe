using BanCoreBot.Dialogs.Cancel;
using BanCoreBot.Dialogs.Solicitudes;
using BanCoreBot.Infrastructure.Luis;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Threading;
using System.Threading.Tasks;
using BanCoreBot.Common.Models.User;

namespace BanCoreBot.Dialogs.Tarjetas
{
    public class NaturalJuridicoDialog : CancelDialog
    {
        private BotState _userState;
        private ILuisService _luisService;
        private const string _ShowOptions = "ShowOptions";
        private const string _ValidateShowOptions = "ValidateShowOptions";
        private UserPersonalData ClientData;

        public NaturalJuridicoDialog(ILuisService luisService, PrivateConversationState userState) 
            : base(nameof(NaturalJuridicoDialog))
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
                if (ClientData.request.Equals("usuariosuspendido") || ClientData.request.Equals("usuariobloqueado") ||
                    ClientData.request.Equals("cambiousuario") || ClientData.request.Equals("claveconexbancaribe") ||
                    ClientData.request.Equals("cambioclave") || ClientData.request.Equals("perfilseguridad") ||
                    ClientData.request.Equals("perfilseguridadsusp") || ClientData.request.Equals("TjtaseguraBloq") ||
                    ClientData.request.Equals("TjtaseguraSusp") || ClientData.request.Equals("cambiocontraseñabancaribe") ||
                    ClientData.request.Equals("problemaonline"))

                {
                    var option = await stepContext.PromptAsync(
                           _ShowOptions,
                           new PromptOptions
                           {
                               Prompt = CreateButtonsUserSuspend(""),
                               RetryPrompt = CreateButtonsUserSuspend("Reintento")
                           }, cancellationToken
                       );
                    return option;
                }

                else if(ClientData.request.Equals("consultareclamo"))
                {
                    if (!String.IsNullOrEmpty(ClientData.typeDoc))
                    {
                        if (ClientData.typeDoc.Equals("E") || ClientData.typeDoc.Equals("V") || ClientData.typeDoc.Equals("J"))
                        {
                            return await stepContext.BeginDialogAsync(nameof(SolicitudesClientes), cancellationToken: cancellationToken);
                        }
                        else
                        {
                            return await stepContext.BeginDialogAsync(nameof(SolicitudesClientesEmpresas), cancellationToken: cancellationToken);
                        }
                    }
                    else
                    {
                        var option = await stepContext.PromptAsync(
                            _ShowOptions,
                            new PromptOptions
                            {
                                Prompt = CreateButtonsQueryClaim(""),
                                RetryPrompt = CreateButtonsQueryClaim("Reintento")
                            }, cancellationToken
                        );
                        return option;
                    }
                }

                else
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
            }
            else
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
        }

        private async Task<DialogTurnResult> ValidateShowOptions(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());

            var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(stepContext.Context, cancellationToken).Result;
            if (luisResult.Entities.natural != null  || luisResult.Entities.opcion1List != null 
                || stepContext.Context.Activity.Text.ToLower().Trim().Equals("natural") )
            {
                if(ClientData.request.Equals("olvidoclave"))
                {
                    await stepContext.Context.SendActivityAsync($"Sí olvidaste tu clave, te sugiero comunicarte con el centro de contacto al número  0500-Bancaribe (0500-2262274) o si te encuentras en el exterior llama al 58-212-9545777 y toma las opciones 2/4/1. Recuerda que para realizar esta acción tienes que poseer tu tarjeta de débito activa", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }

                if (ClientData.request.Equals("consultareclamo"))
                {
                    return await stepContext.BeginDialogAsync(nameof(SolicitudesClientes), cancellationToken: cancellationToken);
                }

                if (ClientData.request.Equals("usuariosuspendido"))
                {
                    await stepContext.Context.SendActivityAsync($"De ser así, te recuerdo que puede ingresar a Mi conexión Bancaribe , Persona Natural," +
                        $" colocar tu usuario, la última clave que recuerdas y tilde la opción Ingresar. Automáticamente el sistema te enviara a una" +
                        $" nueva ventana donde tendrás que presionar la opción \"Aquí\". Esto eliminara el usuario y" +
                        $" tendrás que ingresar por la opción Cliente Nuevo", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }

                if (ClientData.request.Equals("cambiousuario"))
                {
                    await stepContext.Context.SendActivityAsync($"Disculpa pero el Login no se puede modificar, pero si lo puede recuperar", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }

                if (ClientData.request.Equals("usuariobloqueado"))
                {
                    await stepContext.Context.SendActivityAsync($"De ser así, te recuerdo que puedes ingresar a mi conexión bancaribe, persona natural y por ultimo usuario bloqueado. Responde las preguntas que te solicitan y solventaras sin problemas", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }

                if (ClientData.request.Equals("claveconexbancaribe"))
                {
                    await stepContext.Context.SendActivityAsync($"Te recomiendo en este caso ingresa a Mi Conexión Bancaribe, Persona Natural, ¿Olvido su Contraseña? " +
                       $"Luego responde las preguntas que te solicita el sistema y podrás modificar la contraseña sin ningún problema.", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }

                if (ClientData.request.Equals("cambioclave"))
                {
                    await stepContext.Context.SendActivityAsync($"Si deseas modificar la contraseña del ingreso a Mi Conexión Bancaribe, te puedo sugerir que  ingreses por la opción Persona Natural, Cambiar Contraseña. Pero para cambiar la contraseña, debes conocer la contraseña actual, con esa información podrás incluir una nueva.", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }

                if (ClientData.request.Equals("perfilseguridad") || ClientData.request.Equals("TjtaseguraBloq") || ClientData.request.Equals("TjtaseguraSusp") )
                {
                    await stepContext.Context.SendActivityAsync($"Para desbloquear tu perfil de seguridad, solo ingresa a Mi Conexión Bancaribe, persona natural, accede con tu login y contraseña. Una vez dentro de tu cuenta tilda las opciones Servicio al Cliente, Administración de Seguridad y ¿Se Bloqueó su Perfil de Seguridad? recuerda que para realizar esta transacción debe conocer las respuestas de seguridad y poseer la tarjeta conexión segura", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }

                if (ClientData.request.Equals("TjtaSegura"))
                {
                    await stepContext.Context.SendActivityAsync($"Una vez que solicites la tarjeta conexión segura, tienes un plazo de 24 horas para realizar la activación, te comunicas al " +
                    $" 0500-Bancaribe (0500-2262274) por las opciones 2 / 6 / 3. Debes tener a la mano la información de tu tarjeta de débito y poseer la numeración Serial que se " +
                    $"encuentra en la tarjeta conexión segura ya emitida y así realizar la activación", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
                if (ClientData.request.Equals("cambiocontraseñabancaribe") )
                {
                    await stepContext.Context.SendActivityAsync($"Si deseas modificar la contraseña del ingreso a Mi Conexión Bancaribe, te puedo sugerir que  ingreses por la opción Persona Natural, Cambiar Contraseña. Pero para cambiar la contraseña, debes conocer la contraseña actual, con esa información podrás incluir una nueva.", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
                
                if (ClientData.request.Equals("problemaonline"))
                {
                    await stepContext.Context.SendActivityAsync($"¿Qué error te arroja la página?", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }

                if (ClientData.request.Equals("TDC") || ClientData.request.Equals("TDD") || ClientData.request.Equals("Segura"))
                {
                    ClientData.request = "Natural" + ClientData.request; 
                    return await stepContext.BeginDialogAsync(nameof(BloquearDesbloquearTjtaDialog), cancellationToken: cancellationToken);
                }

                if (ClientData.request.Equals("tdcbloqueada") || ClientData.request.Equals("robotdc") 
                    || ClientData.request.Equals("perdidatdc") || ClientData.request.Equals("bloqueotdc")
                    || ClientData.request.Equals("olvidoclavetdc") || ClientData.request.Equals("finiquito")
                    || ClientData.request.Equals("saldocero")
                    || ClientData.request.Equals("renovacionTDC") || ClientData.request.Equals("novetdc")
                    || ClientData.request.Equals("perfilseguridadsusp")) 
                {
                    return await stepContext.BeginDialogAsync(nameof(SolicitudesClientes), cancellationToken: cancellationToken);
                }
                else
                {
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }

            }
            else
            {

                if (ClientData.request.Equals("consultareclamo"))
                {
                    return await stepContext.BeginDialogAsync(nameof(SolicitudesClientesEmpresas), cancellationToken: cancellationToken);
                }

                if (ClientData.request.Equals("olvidoclave"))
                {
                    await stepContext.Context.SendActivityAsync($"Sí olvidaste tu clave, te sugiero comunicarte con el centro de contacto al número  0500-Bancaribe (0500-2262274) o si te encuentras en el exterior llama al 58-212-9545777 y toma las opciones 3/1/4/1. Recuerda que para realizar esta acción tienes que poseer tu tarjeta de débito activa", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }

                if (ClientData.request.Equals("cambiousuario"))
                {
                    await stepContext.Context.SendActivityAsync($"¿Presentas algún problema con tu usuario? el usuario no se puede modificar, te recuerdo que este lo puedes ver en tu carta serial.", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }

                if (ClientData.request.Equals("usuariobloqueado"))
                {
                    await stepContext.Context.SendActivityAsync($"Si tienes a la mano la tarjeta de conexión segura y tu carta serial, te sugiero que ingreses a Mi Conexión Bancaribe, Persona Jurídica, Cambiar/Recuperar contraseña. Responde las pregunta que te solicita el sistema y solventaras el bloqueo", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }

                if (ClientData.request.Equals("perfilseguridad") || ClientData.request.Equals("perfilseguridadsusp"))
                {
                    await stepContext.Context.SendActivityAsync($"Te recuerdo que el usuario jurídico no posee perfil de seguridad, sin embargo, " +
                           $"si ingresas más de 3 veces algún dato errado al momento de realizar alguna transacción, el sistema por precaución no te " +
                           $"permitirá realizar operaciones.", cancellationToken: cancellationToken);
                }

                if (ClientData.request.Equals("TjtaSegura"))
                {
                    await stepContext.Context.SendActivityAsync($"Recuerda que tu tarjeta de conexión segura jurídica no requiere que sea activada, desde que la emites a través de la página " +
                    $"puede realizar operaciones con ella. ", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }

                if (ClientData.request.Equals("TDC") || ClientData.request.Equals("TDD") || ClientData.request.Equals("Segura"))
                {
                    ClientData.request = "Juridico" + ClientData.request; 
                    return await stepContext.BeginDialogAsync(nameof(BloquearDesbloquearTjtaDialog), cancellationToken: cancellationToken);
                }

                if (ClientData.request.Equals("tdcbloqueada") || ClientData.request.Equals("robotdc") 
                    || ClientData.request.Equals("perdidatdc") || ClientData.request.Equals("bloqueotdc")
                    || ClientData.request.Equals("olvidoclavetdc") || ClientData.request.Equals("finiquito")
                    || ClientData.request.Equals("saldocero") 
                    || ClientData.request.Equals("renovacionTDC") || ClientData.request.Equals("novetdc")
                    || ClientData.request.Equals("usuariosuspendido") || ClientData.request.Equals("claveconexbancaribe")
                    || ClientData.request.Equals("cambioclave") || ClientData.request.Equals("solicitudcartaserial")
                    || ClientData.request.Equals("TjtaseguraBloq") || ClientData.request.Equals("TjtaseguraSusp")
                    || ClientData.request.Equals("cambiocontraseñabancaribe") || ClientData.request.Equals("perfilseguridad") 
                    || ClientData.request.Equals("perfilseguridadsusp") || ClientData.request.Equals("problemaonline"))
                {
                    return await stepContext.BeginDialogAsync(nameof(SolicitudesClientesEmpresas), cancellationToken: cancellationToken);
                }
                else
                {
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
            }

        }

        #endregion DialogCtas


        #region Validators


        private Task<bool> ValidateInputValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(promptContext.Context, cancellationToken).Result;
            if (
                luisResult.TopIntent().intent.ToString().Equals("TipoPersona") ||
                luisResult.TopIntent().intent.ToString().Equals("Opciones") ||
                luisResult.Entities.juridico != null ||
                luisResult.Entities.natural != null
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
                reply = MessageFactory.Text("¿La tarjeta está asociada a una cuenta natural o una cuenta  jurídica?");
            }
            else
            {
                reply = MessageFactory.Text("Por favor, me indicas el tipo de cuenta(natural o jurídica) para poder ayudarte según sea el caso.");
            }

            return reply as Activity;
        }

        private Activity CreateButtonsUserSuspend(string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                reply = MessageFactory.Text("¿El inconveniente lo presentas en Cuenta Natural o  Cuenta  Jurídica?");
            }
            else
            {
                reply = MessageFactory.Text("Por favor, me indicas el tipo de cuenta(natural o jurídica) para poder ayudarte según sea el caso.");
            }

            return reply as Activity;
        }


        private Activity CreateButtonsQueryClaim(string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                reply = MessageFactory.Text("¿El reporte al que haces referencia es de una cuenta Natural o una cuenta Jurídica?");
            }
            else
            {
                reply = MessageFactory.Text("Por favor, me indicas el tipo de cuenta(natural o jurídica) para poder ayudarte según sea el caso.");
            }

            return reply as Activity;
        }
        #endregion Buttons

    }
}
