using BanCoreBot.Common.Models.User;
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

namespace BanCoreBot.Dialogs.Tarjetas
{
    public class NoVeTDCDialog : CancelDialog
    {
        private BotState _userState;
        private readonly ILuisService _luisService;
        private const string _ValidateInput = "ValidateInput";
        private const string _ValidateOption = "ValidateOption";
        private UserPersonalData ClientData;

        public NoVeTDCDialog(ILuisService luisService, PrivateConversationState userState) : base(nameof(NoVeTDCDialog))
        {
            _userState = userState;
            _luisService = luisService;
            var waterfallSteps = new WaterfallStep[]
            {
                ValidateInput,
                ValidateOption,
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(_ValidateInput, ValidateInputValidator));
            AddDialog(new TextPrompt(_ValidateOption));
        }


        #region DialogCtas
        private async Task<DialogTurnResult> ValidateInput(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(
                _ValidateInput,
                new PromptOptions
                {
                    Prompt = CreateButtonsOptions(""),
                    RetryPrompt = CreateButtonsOptions("Reintento")
                }, cancellationToken
            );
        }



        private async Task<DialogTurnResult> ValidateOption(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());

            var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(stepContext.Context, cancellationToken).Result;
            if (luisResult.TopIntent().intent.ToString().Equals("afirmacion"))
            {
                await stepContext.Context.SendActivityAsync("Debido a una actualización que estamos haciendo a nuestros canales electrónicos, las tarjetas de crédito no serán visualizadas en Mi Conexión Bancaribe. Ofrecemos disculpas por las molestias ocasionadas, en los próximos días tendremos regularizada la información.", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else
            {
                if (ClientData.request.Equals("novetdcjuridico"))
                {
                    return await stepContext.BeginDialogAsync(nameof(SolicitudesClientesEmpresas), cancellationToken: cancellationToken);
                }
                else if (ClientData.request.Equals("novetdcnatural"))
                {
                    return await stepContext.BeginDialogAsync(nameof(SolicitudesClientes), cancellationToken: cancellationToken);
                }
                else
                {
                    return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
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

        private Activity CreateButtonsOptions(string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                reply = MessageFactory.Text("¿Ya podías ver tu Tarjeta de Crédito antes en Mi Conexión Bancaribe?");
            }
            else
            {
                reply = MessageFactory.Text("Por favor, me confirmas si ¿antes podías observar tu tarjeta de crédito en mi conexión bancaribe?");
            }

            return reply as Activity;
        }

        #endregion Buttons

    }
}
