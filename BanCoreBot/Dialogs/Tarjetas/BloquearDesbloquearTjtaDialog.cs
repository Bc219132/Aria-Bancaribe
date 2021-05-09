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

namespace BanCoreBot.Dialogs.Tarjetas
{
    public class BloquearDesbloquearTjtaDialog : CancelDialog
    {
        private BotState _userState;
        private ILuisService _luisService;
        private const string _ShowOptions = "ShowOptions";
        private const string _ValidateShowOptions = "ValidateShowOptions";
        private UserPersonalData ClientData;

        public BloquearDesbloquearTjtaDialog(ILuisService luisService, PrivateConversationState userState)
            : base(nameof(BloquearDesbloquearTjtaDialog))
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
                    Prompt = await CreateButtons("", stepContext),
                    RetryPrompt = await CreateButtons("Reintento", stepContext)
                }, cancellationToken
            );
            return option;
        }

        private async Task<DialogTurnResult> ValidateShowOptions(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());

            var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(stepContext.Context, cancellationToken).Result;
            if (luisResult.Entities.Bloqueo != null)
            {//Bloqueo 
                if (ClientData.request.Contains("TDC"))
                {
                    await stepContext.Context.SendActivityAsync("Para realizar el **Bloqueo** de tu **Tarjeta de Crédito** debes comunicarte al número **0500-Bancaribe (0500-2262274)** o, " +
                        "si te encuentras en el exterior, al **+58-212-9545777**, selecciona la **opción 1** y sigue las instrucciones " +
                        "del sistema automatizado.", cancellationToken: cancellationToken);
                    await stepContext.Context.SendActivityAsync("Recuerda que una vez bloqueada tu Tarjeta de Crédito se emitirá de manera automática la reposición de tu nuevo plástico.", cancellationToken: cancellationToken);
                }
                else if (ClientData.request.Contains("TDD"))
                {
                    await stepContext.Context.SendActivityAsync("Para realizar el **Bloqueo** de tu **Tarjeta de Débito** debes comunicarte al número **0500-Bancaribe (0500-2262274)** o, " +
                           "si te encuentras en el exterior, al **+58-212-9545777**, selecciona la **opción 1** y sigue las instrucciones " +
                           "del sistema automatizado.", cancellationToken: cancellationToken);
                    await stepContext.Context.SendActivityAsync("Recuerda que una vez bloqueada tu Tarjeta de Débito, debes de dirigirte a la Agencia Bancaribe" +
                        " más cercana para realizar la reposición de tu nuevo plástico.", cancellationToken: cancellationToken);
                }
                else
                {
                    if (ClientData.request.Contains("Natural"))
                    {
                        await stepContext.Context.SendActivityAsync("Si deseas bloquear o desactivar tu Tarjeta Conexión Segura, " +
                            "sólo debes comunicarte al 0500-Bancaribe (0500-2262274) o, si te encuentras en el exterior al +58-212-9545777 " +
                            "por las opciones 2 / 1 o 2 (dependiendo de la tarjeta que poseas) / 6 / 4. Debes tener a la mano la información de tus " +
                            "productos financieros con el banco para bloquearla o desactivarla de manera correcta", cancellationToken: cancellationToken);
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync($"Tienes dos opciones para proceder con tu solicitud: {Environment.NewLine}" +
                            $"1.- Ingresa en Mi Conexión Bancaribe Jurídica a través de \"Recuperar tarjeta Conexión Segura: [Click aquí](https://www4.bancaribe.com.ve/bcj/)\" " +
                            $"coloca los datos solicitados y obtén tu nueva Tarjeta Conexión Segura Jurídica.{Environment.NewLine}" +
                            "2.- Si persiste el inconveniente con tu Tarjeta Conexión Segura Jurídica, sólo debes comunicarte al 0500 - Bancaribe (0500 - 2262274) o, " +
                            "si te encuentras en el exterior al + 58 - 212 - 9545777 por las opciones 3 / 1 o 2 (si tienes o no tarjeta de débito) / 6 / 1. " +
                            "Debes tener a la mano la información de tus productos financieros con el banco.", cancellationToken: cancellationToken);
                    }
                }
            }
            else
            {// Desbloqueo 
                if (ClientData.request.Contains("TDD"))
                {
                    if (ClientData.request.Contains("Natural"))
                    {
                        await stepContext.Context.SendActivityAsync("Si presentas un bloqueo, es muy sencillo.", cancellationToken: cancellationToken);
                        await stepContext.Context.SendActivityAsync("Sólo debes comunicarte al 0500-Bancaribe (0500-2262274) o, si te encuentras en el exterior al +58-212-9545777," +
                            " necesitas tener a la mano tu Tarjeta de Débito, esto para identificar que tipo de bloqueo presentas en tu  instrumento, escucha atentamente al sistema " +
                            "automatizado y vas a marcar la opción 2 Persona Natural.", cancellationToken: cancellationToken);
                        await stepContext.Context.SendActivityAsync("Por ese medio se te indicará cual es la manera adecuada para realizar el Desbloqueo.", cancellationToken: cancellationToken);
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync("Si presentas un bloqueo, es muy sencillo.", cancellationToken: cancellationToken);
                        await stepContext.Context.SendActivityAsync("Sólo debes comunicarte al 0500-Bancaribe (0500-2262274) o, si te encuentras en el exterior al +58-212-9545777," +
                            " necesitas tener a la mano tu Tarjeta de Débito, esto para identificar que tipo de bloqueo presentas en tu  instrumento, escucha atentamente al sistema " +
                            "automatizado y vas a marcar la opción 3 Persona Jurídica.", cancellationToken: cancellationToken);
                        await stepContext.Context.SendActivityAsync("Por ese medio se te indicará cual es la manera adecuada para realizar el Desbloqueo.", cancellationToken: cancellationToken);
                    }
                }
                else if (ClientData.request.Contains("TDC"))
                {
                    await stepContext.Context.SendActivityAsync("Si presentas un bloqueo, es muy sencillo.", cancellationToken: cancellationToken);
                    await stepContext.Context.SendActivityAsync("Sólo debes comunicarte al 0500-Bancaribe (0500-2262274) o, si te encuentras en el exterior al +58-212-9545777, " +
                        "necesitas tener a la mano tu Tarjeta de Crédito, esto para identificar que tipo de bloqueo presentas en tu  instrumento, escucha atentamente al sistema automatizado " +
                        "y vas a marcar la opción 5 seguida de la opción 3.", cancellationToken: cancellationToken);
                    await stepContext.Context.SendActivityAsync("Por ese medio se te indicará cual es la manera adecuada para realizar el Desbloqueo.", cancellationToken: cancellationToken);
                }
                else
                {
                    if (ClientData.request.Contains("Natural"))
                    {
                        await stepContext.Context.SendActivityAsync("Para desbloquear tu Tarjeta de Conexión Segura, solo ingresa a Mi Conexión Bancaribe, " +
                            "persona natural, accede con tu login y contraseña. Una vez dentro de tu cuenta tilda las opciones Servicio al Cliente, " +
                            "Administración de Seguridad y ¿Se Bloqueó su Perfil de Seguridad? recuerda que para realizar esta transacción debe conocer las respuestas " +
                            "de seguridad y poseer la tarjeta conexión segura", cancellationToken: cancellationToken);
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync($"Tienes dos opciones para proceder con tu solicitud: {Environment.NewLine}" +
                            $"1.- Ingresa en Mi Conexión Bancaribe Jurídica a través de \"Recuperar tarjeta Conexión Segura: [Click aquí](https://www4.bancaribe.com.ve/bcj/)\" " +
                            $"coloca los datos solicitados y obtén tu nueva Tarjeta Conexión Segura Jurídica.{Environment.NewLine}" +
                            "2.- Si persiste el inconveniente con tu Tarjeta Conexión Segura Jurídica, sólo debes comunicarte al 0500 - Bancaribe (0500 - 2262274) o, " +
                            "si te encuentras en el exterior al + 58 - 212 - 9545777 por las opciones 3 / 1 o 2 (si tienes o no tarjeta de débito) / 6 / 1. " +
                            "Debes tener a la mano la información de tus productos financieros con el banco.", cancellationToken: cancellationToken);
                    }
                }
            }
            ClientData.request = "";
            return await stepContext.ContinueDialogAsync(cancellationToken:cancellationToken);
        
        }

        #endregion DialogCtas


        #region Validators


        private Task<bool> ValidateInputValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var result = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(promptContext.Context, cancellationToken).Result;
            if (
               result.Entities.Bloqueo != null
               || result.Entities.Desbloqueo != null
               || result.Entities.Bloqueado != null
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
            if(ClientData.request.Contains("TDC"))
            {
                if (String.IsNullOrEmpty(retry))
                {
                    reply = MessageFactory.Text("¿Quieres bloquear o desbloquear tu tarjeta de crédito?");
                }
                else
                {
                    reply = MessageFactory.Text("Me confirmas por favor si quieres bloquear o desbloquear tu tarjeta de crédito");
                }
            }
            if (ClientData.request.Contains("TDD"))
            {
                if (String.IsNullOrEmpty(retry))
                {
                    reply = MessageFactory.Text("¿Quieres bloquear o desbloquear tu tarjeta de débito?");
                }
                else
                {
                    reply = MessageFactory.Text("Me confirmas por favor si quieres bloquear o desbloquear tu tarjeta de débito");
                }
            }
            if (ClientData.request.Contains("Segura"))
            {
                if (String.IsNullOrEmpty(retry))
                {
                    reply = MessageFactory.Text("¿Quieres bloquear o desbloquear tu tarjeta de conexión segura?");
                }
                else
                {
                    reply = MessageFactory.Text("Me confirmas por favor si quieres bloquear o desbloquear tu tarjeta de conexión segura");
                }
            }

            return reply as Activity;
        }

        #endregion Buttons

    }
}
