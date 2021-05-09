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
using BanCoreBot.Dialogs.Reclamos;

namespace BanCoreBot.Dialogs.BloqueadoSuspendido
{
    public class SolicitarDatosDialog : CancelDialog
    {
        private BotState _userState;
        private ILuisService _luisService;
        private const string _ShowOptions = "ShowOptions";
        private const string _ValidateShowOptions = "ValidateShowOptions";
        private UserPersonalData ClientData;

        public SolicitarDatosDialog(ILuisService luisService, PrivateConversationState userState) : base(nameof(SolicitarDatosDialog))
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
                    Prompt = await CreateButtonsAsync("", stepContext),
                    RetryPrompt = await  CreateButtonsAsync("Reintento", stepContext)
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
                if (ClientData.request.Equals("consultareclamo"))
                {
                    ClientData.request = "";
                    return await stepContext.BeginDialogAsync(nameof(ConsultarReclamoDialog), cancellationToken: cancellationToken);
                }
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync("Entiendo, ¿hay algo más en lo que te pueda ayudar?", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
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

        private async Task<Activity> CreateButtonsAsync(string retry, WaterfallStepContext stepContext)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            var reply = MessageFactory.Text("");
            if (ClientData.request.Equals("consultareclamo"))
            {
                if (String.IsNullOrEmpty(retry))
                {
                    reply = MessageFactory.Text("Puedo ayudarte a consultar el estatus de tu reporte, ¿estas de acuerdo con que te pida algunos datos para proceder con tu solicitud?");
                }
                else
                {
                    reply = MessageFactory.Text("¿deseas continuar con tu consulta de reporte?");
                }
            }
            else
            {
                
                if (String.IsNullOrEmpty(retry))
                {
                    reply = MessageFactory.Text("¿Estas de acuerdo con que te solicite algunos datos para enviarlos al área encargada?");
                }
                else
                {
                    reply = MessageFactory.Text("¿deseas que tu caso sea enviado al área encargada?");
                }
            }
            return reply as Activity;
        }

        #endregion Buttons

    }
}
