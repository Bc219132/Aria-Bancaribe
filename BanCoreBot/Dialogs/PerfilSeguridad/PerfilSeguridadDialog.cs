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

namespace BanCoreBot.Dialogs.PerfilSeguridad
{
    public class PerfilSeguridadDialog : CancelDialog
    {
        private BotState _userState;
        private ILuisService _luisService;
        private const string _ShowOptions = "ShowOptions";
        private const string _ValidateShowOptions = "ValidateShowOptions";
        private UserPersonalData ClientData;

        public PerfilSeguridadDialog(ILuisService luisService, PrivateConversationState userState) : base(nameof(PerfilSeguridadDialog))
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
            if (ClientData.request.Equals("perfilseguridadsusp"))
            {
                var option = await stepContext.PromptAsync(
                    _ShowOptions,
                    new PromptOptions
                    {
                        Prompt = CreateButtonsSusp(""),
                        RetryPrompt = CreateButtonsSusp("Reintento")
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
            if (luisResult.TopIntent().intent.ToString().Equals("afirmacion"))
            {
                return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync("Entiendo, brindame más información de lo que deseas para ayudarte", cancellationToken: cancellationToken);
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

        private Activity CreateButtons(string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                reply = MessageFactory.Text("¿Me quieres decir que se bloqueó tu Perfil de Seguridad? ");
            }
            else
            {
                reply = MessageFactory.Text("Por favor, me confirmas si ¿lo que intentas decirme es que tu perfil de seguridad se bloqueó?");
            }

            return reply as Activity;
        }


        private Activity CreateButtonsSusp(string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                reply = MessageFactory.Text("¿Me quieres decir que tu perfil de seguridad está suspendido? ");
            }
            else
            {
                reply = MessageFactory.Text("Por favor, me confirmas si ¿lo que intentas decirme es que tu perfil de seguridad está suspendido?");
            }

            return reply as Activity;
        }

        #endregion Buttons

    }
}
