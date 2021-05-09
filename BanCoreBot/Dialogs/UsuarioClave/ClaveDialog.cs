using BanCoreBot.Dialogs.Cancel;
using BanCoreBot.Dialogs.Tarjetas;
using BanCoreBot.Infrastructure.Luis;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Threading;
using System.Threading.Tasks;
using BanCoreBot.Common.Models.User;

namespace BanCoreBot.Dialogs.UsuarioClave
{
    public class ClaveDialog : CancelDialog
    {
        private BotState _userState;
        private ILuisService _luisService;
        private const string _ShowOptions = "ShowOptions";
        private const string _ValidateShowOptions = "ValidateShowOptions";
        private UserPersonalData ClientData;

        public ClaveDialog(ILuisService luisService, PrivateConversationState userState) : base(nameof(ClaveDialog))
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
            if (ClientData.request != null)
            {
                var option = await stepContext.PromptAsync(
                    _ShowOptions,
                    new PromptOptions
                    {
                        Prompt = CreateButtonsCmbClave(""),
                        RetryPrompt = CreateButtonsCmbClave("Reintento")
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

            if (ClientData.request != null)
            {
                if (luisResult.TopIntent().intent.ToString().Equals("afirmacion"))
                {
                    ClientData.request = "cambiocontraseñabancaribe";
                    return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync("¿Puedes ser un poco más específico? No logro entender que me estas solicitando o preguntando", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
            }
            else
            {
                if (luisResult.TopIntent().intent.ToString().Equals("afirmacion"))
                {
                    ClientData.request = "claveconexbancaribe";
                    return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync("¿Puedes ser un poco más específico? No logro entender que me estas solicitando o preguntando", cancellationToken: cancellationToken);
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
        
        private Activity CreateButtonsCmbClave(string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                reply = MessageFactory.Text("¿Me quieres decir que quieres conocer como cambiar tu clave para ingresar Mi Conexión Bancaribe?");
            }
            else
            {
                reply = MessageFactory.Text("¿Intentas decirme que no conocer como cambiar tu contraseña de Mi Conexión Bancaribe?");
            }

            return reply as Activity;
        }
        private Activity CreateButtons(string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                reply = MessageFactory.Text("¿Me quieres decir que no recuerdas tu contraseña para ingresar a Mi Conexión Bancaribe?");
            }
            else
            {
                reply = MessageFactory.Text("¿Intentas decirme que no recuerdas como es tu contraseña?");
            }

            return reply as Activity;
        }

        #endregion Buttons

    }
}
