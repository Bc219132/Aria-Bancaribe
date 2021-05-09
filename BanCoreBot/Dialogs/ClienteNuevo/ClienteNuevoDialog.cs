using BanCoreBot.Dialogs.Cancel;
using BanCoreBot.Infrastructure.Luis;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BanCoreBot.Dialogs.ClienteNuevo
{
    public class ClienteNuevoDialog : CancelDialog
    {
        private ILuisService _luisService;
        private const string _ShowOptions = "ShowOptions";
        private const string _ValidateShowOptions = "ValidateShowOptions";
        public ClienteNuevoDialog(ILuisService luisService) : base(nameof(ClienteNuevoDialog))
        {
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
            if (luisResult.Entities.OnlineBanking != null || luisResult.Entities.opcion1List != null 
                || luisResult.Entities.Afiliacion != null || luisResult.Entities.nuevo != null)
            { //REGISTRO BANCA EN LINEA
                await stepContext.Context.SendActivityAsync($"Si no posees un usuario ni una contraseña para el ingreso a Mi conexión Bancaribe, te sugiero que ingreses " +
                   $"por las opciones Persona Natural y Cliente Nuevo. El sistema te solicitara los datos de tu tarjeta de débito (estado activa) " +
                   $"y algunos de tus datos personales, así podrás acceder sin problema", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.usuario != null || luisResult.Entities.opcion2List != null)
            {// OLVIDO USUARIO
                await stepContext.Context.SendActivityAsync($"Te puedo recomendar en ese caso que ingreses a Mi Conexión Bancaribe, Persona Natural, ¿Olvide mi Login? Valida la información que te solicitan y se hará él envió del Login al correo anexado al Perfil de Seguridad", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
            else
            {// OLVIDO CLAVE
                await stepContext.Context.SendActivityAsync($"Te recomiendo en este caso ingresa a Mi Conexión Bancaribe, Persona Natural, ¿Olvido su Contraseña? Luego responde las preguntas que te solicita el sistema y podrás modificar la contraseña sin ningún problema.", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);

            }
         
        }

        #endregion DialogCtas


        #region Validators


        private Task<bool> ValidateInputValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(promptContext.Context, cancellationToken).Result;
            if (

                luisResult.Entities.usuario != null || luisResult.Entities.clave != null
                || luisResult.Entities.OnlineBanking != null || luisResult.Entities.Afiliacion != null
                || luisResult.Entities.nuevo != null || luisResult.TopIntent().intent.ToString().Equals("Opciones")
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
                reply = MessageFactory.Text("Quieres saber ¿Cómo Ingresar por Primera vez a Mi conexión Bancaribe?, ¿Olvidaste tu Usuario? o ¿Olvidaste tu Contraseña? ");
            }
            else
            {
                reply = MessageFactory.Text("Por favor dime, si quieres saber ¿Cómo Ingresar por Primera vez a Mi conexión Bancaribe?, ¿Olvidaste tu Usuario? o ¿Olvidaste tu Contraseña? ");
            }

            return reply as Activity;
        }

        #endregion Buttons

    }
}
