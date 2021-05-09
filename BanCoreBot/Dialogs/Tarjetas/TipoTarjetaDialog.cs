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
using BanCoreBot.Dialogs.BloqueadoSuspendido;

namespace BanCoreBot.Dialogs.Tarjetas
{
    public class TipoTarjetaDialog : CancelDialog
    {
        private BotState _userState;
        private ILuisService _luisService;
        private const string _ShowOptions = "ShowOptions";
        private const string _ValidateShowOptions = "ValidateShowOptions";
        private UserPersonalData ClientData;

        public TipoTarjetaDialog(ILuisService luisService, PrivateConversationState userState)
            : base(nameof(TipoTarjetaDialog))
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
            if (!string.IsNullOrEmpty(ClientData.request))
            {
                if (ClientData.request.Equals("olvidoclavetjta") || ClientData.request.Equals("renovaciontjta"))
                {
                    var option = await stepContext.PromptAsync(
                        _ShowOptions,
                        new PromptOptions
                        {
                            Prompt = CreateButtonsTDDTDC(""),
                            RetryPrompt = CreateButtonsTDDTDC("Reintento")
                        }, cancellationToken
                    );
                    return option;
                }

                if (ClientData.request.Equals("TjtaSusp"))
                {
                    var option = await stepContext.PromptAsync(
                        _ShowOptions,
                        new PromptOptions
                        {
                            Prompt = CreateButtonsTjtaSusp(""),
                            RetryPrompt = CreateButtonsTjtaSusp("Reintento")
                        }, cancellationToken
                    );
                    return option;
                }
                else
                {
                    var option = await stepContext.PromptAsync(
                        _ShowOptions,
                        new PromptOptions
                        {
                            Prompt = CreateButtonsTjtaBloq(""),
                            RetryPrompt = CreateButtonsTjtaBloq("Reintento")
                        }, cancellationToken
                    );
                    return option;
                }
            }

            else
            {
                ClientData.request = "";
                var option = await stepContext.PromptAsync(
                    _ShowOptions,
                    new PromptOptions
                    {
                        Prompt = CreateButtonsTjtaBloq(""),
                        RetryPrompt = CreateButtonsTjtaBloq("Reintento")
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
            
            if (ClientData.request.Equals("olvidoclavetjta"))
            {
                if ((luisResult.Entities.tdc != null && luisResult.Entities.tdd is null && luisResult.Entities.tarjetasegura is null) ||
                luisResult.Entities.opcion1List != null)
                {//Tarjeta de Crédito
                    ClientData.request = "olvidoclavetdc";
                    return await stepContext.BeginDialogAsync(nameof(OlvidoClaveTDCDialog), cancellationToken: cancellationToken);
                }
                else 
                {// Tarjeta de Débito
                    ClientData.request = "olvidoclave";
                    return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
                }
            }
            else if (ClientData.request.Equals("renovaciontjta"))
            {
                if (luisResult.Entities.tdd != null && luisResult.Entities.tdc == null)
                {
                    return await stepContext.BeginDialogAsync(nameof(RenovacionTDDDialog), cancellationToken: cancellationToken);
                }
                else
                {
                    ClientData.request = "renovacionTDC";
                    return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
                }
            }
            else if (ClientData.request.Equals("TjtaSusp"))
            {
                if (luisResult.Entities.usuario != null || luisResult.Entities.suspendido != null ||
                luisResult.Entities.opcion1List != null || luisResult.Entities.OnlineBanking != null)
                {//Usuario Suspendido
                    ClientData.request = "usuariosusponline";
                    return await stepContext.BeginDialogAsync(nameof(BloqueadoSuspendidoDialog), cancellationToken: cancellationToken);
                }
                else if ((luisResult.Entities.tdd != null && luisResult.Entities.suspendido is null &&
                luisResult.Entities.tdc is null && luisResult.Entities.tarjetasegura is null) || luisResult.Entities.opcion2List != null)
                {// Tarjeta de Débito
                    await stepContext.Context.SendActivityAsync($"La tarjeta de débito no presenta el estatus de suspensión, esta solo se puede bloquear, ya sea por robo o extravió o por ingreso de la clave de manera incorrecta. ", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
                else if ((luisResult.Entities.tdc != null && luisResult.Entities.suspendido is null &&
                luisResult.Entities.tdd is null && luisResult.Entities.tarjetasegura is null) || luisResult.Entities.opcion3List != null)
                {//Tarjeta de Crédito
                    ClientData.request = "olvidoclavetdc";
                    return await stepContext.BeginDialogAsync(nameof(OlvidoClaveTDCDialog), cancellationToken: cancellationToken);
                }
                else 
                {// Tarjeta de Segura
                    await stepContext.Context.SendActivityAsync($"La tarjeta de conexión segura o coordenadas no se bloquean, ni se suspende. Si el sistema le indica algún problema al momento de transferir y esta se encuentra activa, lo más probable es que el inconveniente lo presenta el perfil de seguridad", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
            }

            else 
            {
                if (luisResult.Entities.natural != null && (luisResult.Entities.usuario != null || luisResult.Entities.Bloqueo != null ||
                luisResult.Entities.opcion1List != null || luisResult.Entities.Bloqueado != null ||
                luisResult.Entities.OnlineBanking != null))
                {//Usuario Bloqueado Natural
                    ClientData.request = "usuariobloqueadonat";
                    return await stepContext.BeginDialogAsync(nameof(BloqueadoSuspendidoDialog), cancellationToken: cancellationToken);
                }
                else if (luisResult.Entities.juridico != null && (luisResult.Entities.usuario != null || luisResult.Entities.Bloqueo != null ||
                luisResult.Entities.opcion1List != null || luisResult.Entities.Bloqueado != null ||
                luisResult.Entities.OnlineBanking != null))
                {//Usuario Bloqueado Juridico
                    ClientData.request = "usuariobloqueadojur";
                    return await stepContext.BeginDialogAsync(nameof(BloqueadoSuspendidoDialog), cancellationToken: cancellationToken);
                }
                else if (luisResult.Entities.usuario != null || luisResult.Entities.Bloqueo != null ||
                luisResult.Entities.opcion1List != null || luisResult.Entities.Bloqueado != null ||
                luisResult.Entities.OnlineBanking != null)
                {//Usuario Bloqueado
                    ClientData.request = "usuariobloqueado";
                    return await stepContext.BeginDialogAsync(nameof(BloqueadoSuspendidoDialog), cancellationToken: cancellationToken);
                }
                else if ((luisResult.Entities.tdd != null && luisResult.Entities.suspendido is null &&
                luisResult.Entities.tdc is null && luisResult.Entities.tarjetasegura is null) || luisResult.Entities.opcion2List != null)
                {// Tarjeta de Débito
                    await stepContext.Context.SendActivityAsync($"Si presentas un bloqueo, accede a este servicio, es muy sencillo. Sólo necesitas tu tarjeta de débito y llamar al 0500-Bancaribe (0500-2262274) o, si te encuentras en el exterior, al 58-212-9545777." +
                        $" Vas a escuchar atentamente al sistema automatizado y vas a marcar la opción correspondiente si es Persona Natural(opción 2) o Jurídica(opción 3) y el sistema te indicara paso a paso como desbloquearla para que puedas seguir disfrutando de nuestros servicios.", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
                else if ((luisResult.Entities.tdc != null && luisResult.Entities.suspendido is null &&
                luisResult.Entities.tdc is null && luisResult.Entities.tarjetasegura is null) || luisResult.Entities.opcion3List != null)
                {//Tarjeta de Crédito
                    ClientData.request = "olvidoclavetdc";
                    return await stepContext.BeginDialogAsync(nameof(OlvidoClaveTDCDialog), cancellationToken: cancellationToken);
                }
                else
                {// Tarjeta de Segura
                    await stepContext.Context.SendActivityAsync($"La tarjeta de conexión segura o coordenadas no se bloquean, ni se suspende. Si el sistema le indica algún problema al momento de transferir y esta se encuentra activa, lo más probable es que el inconveniente lo presenta el perfil de seguridad", cancellationToken: cancellationToken);
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
                luisResult.TopIntent().intent.ToString().Equals("TipoTarjeta")
               || luisResult.TopIntent().intent.ToString().Equals("Opciones")
               || luisResult.Entities.OnlineBanking != null
               || luisResult.Entities.Bloqueado != null
               || luisResult.Entities.Bloqueo != null
               || luisResult.Entities.suspendido != null
               || luisResult.Entities.usuario != null

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

        private Activity CreateButtonsTDDTDC(string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                reply = MessageFactory.Text("¿La tarjeta a la que haces referencia es tarjeta de débito o tarjeta de crédito?");
            }
            else
            {
                reply = MessageFactory.Text("Para ayudarte necesito que me respondas ¿de qué tipo de tarjeta me hablas, débito o crédito?");
            }

            return reply as Activity;
        }
        //

        private Activity CreateButtonsTjtaBloq(string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                reply = MessageFactory.Text("¿Me quieres decir qué tu usuario se encuentra bloqueado? o ¿presentas algún problema con la tarjeta de débito, tarjeta de crédito o tarjeta conexión segura? ");
            }
            else
            {
                reply = MessageFactory.Text("Para ayudarte necesito que me respondas si ¿tu usuario se encuentra bloqueado? o ¿presentas algún problema con la tarjeta de débito, tarjeta de crédito o tarjeta conexión segura? ");
            }

            return reply as Activity;
        }

        private Activity CreateButtonsTjtaSusp(string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                reply = MessageFactory.Text("¿Me quieres decir qué tu usuario se encuentra suspendido? o ¿presentas algún problema con la tarjeta de débito, tarjeta de crédito o tarjeta conexión segura? ");
            }
            else
            {
                reply = MessageFactory.Text("Para ayudarte necesito que me respondas si ¿tu usuario se encuentra suspendido? o ¿presentas algún problema con la tarjeta de débito, tarjeta de crédito o tarjeta conexión segura? ");
            }

            return reply as Activity;
        }

        #endregion Buttons

    }
}
