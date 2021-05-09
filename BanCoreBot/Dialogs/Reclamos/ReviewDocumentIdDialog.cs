using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BanCoreBot.Common.Utils;
using BanCoreBot.Dialogs.Cancel;
using BanCoreBot.Common.Models.User;
using BanCoreBot.Infrastructure.Luis;
using Luis;

namespace BanCoreBot.Dialogs.Reclamos
{
    public class ReviewDocumentIdDialog : CancelDialog
    {
        private ILuisService _luisService;
        private BotState _userState;
        private const string _SetCIPass = "SetCIPass";
        private const string _SetCI = "SetCI";
        private const string _Confirmacion = "Confirmacion";
        private const string _FinalProcess = "FinalProcess";
        private Utils utilitario = new Utils();
        private UserPersonalData ClientData;

        public ReviewDocumentIdDialog(PrivateConversationState userState, ILuisService luisService)
            : base(nameof(ReviewDocumentIdDialog))
        {
            _luisService = luisService;
            _userState = userState;

            var waterfallStep = new WaterfallStep[]
            {
                SetCIPass,
                SetCI,
                Confirmacion,
                FinalProcess,
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallStep));
            AddDialog(new TextPrompt(_SetCIPass, CIPassValidator));
            AddDialog(new TextPrompt(_SetCI, CIValidator));
            AddDialog(new TextPrompt(_Confirmacion, ConfirmValidator));
            AddDialog(new TextPrompt(_FinalProcess));
        }



        #region conversationClaim
        private async Task<DialogTurnResult> SetCIPass(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var option = await stepContext.PromptAsync(
                _SetCIPass,
                new PromptOptions
                {
                    Prompt = CreateButtonsCIPass()
                }, cancellationToken
            );
            return option;
        }

        private async Task<DialogTurnResult> SetCI(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(stepContext.Context, cancellationToken).Result;

            if (luisResult.Entities.venezolano != null)
            {
                ClientData.typeDoc = "V";
            }
            else if (luisResult.Entities.Extranjero != null)
            {
                ClientData.typeDoc = "E";
            }
            else if (luisResult.Entities.juridico != null)
            {
                ClientData.typeDoc = "J";
            }
            else if (luisResult.Entities.rifgobierno != null)
            {
                ClientData.typeDoc = "G";
            }
            else
            {
                ClientData.typeDoc = "P";
            }
            
            var auxText = "";
            if (luisResult.Entities.venezolano != null || luisResult.Entities.Extranjero != null)
            {
                auxText = "¿Cuál es tu número de cédula?";
            }
            else if (luisResult.Entities.juridico != null || luisResult.Entities.rifgobierno != null)
            {
                auxText = "¿Cuál es el número del rif?";
            }
            else
            {
                auxText = "¿Cuál es tu número de pasaporte?";
            }
            return await stepContext.PromptAsync(
                    _SetCI,
                    new PromptOptions { Prompt = MessageFactory.Text(auxText) },
                    cancellationToken
                    );

        }

        private async Task<DialogTurnResult> Confirmacion(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            ClientData.ci = stepContext.Context.Activity.Text;
            return await stepContext.PromptAsync(
                _Confirmacion,
                new PromptOptions
                {
                    Prompt = await CreateButtonsConfirm(stepContext, ""),
                    RetryPrompt = await CreateButtonsConfirm(stepContext, "")
                }, cancellationToken
            );
        }
        private async Task<DialogTurnResult> FinalProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Context.Activity.Text.ToLower().Equals("si"))
            {
                return await stepContext.ContinueDialogAsync(cancellationToken);
            }
            else
            {
                return await stepContext.ReplaceDialogAsync(nameof(ReviewDocumentIdDialog), stepContext.Values, cancellationToken);
            }

        }

        #endregion conversationClaim


        #region Validators     
        private Task<bool> CIPassValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var result = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(promptContext.Context, cancellationToken).Result;
            if (
                 result.Entities.venezolano != null
                || result.Entities.Extranjero != null
                || result.Entities.pasaporte != null
                || result.Entities.juridico != null
                || result.Entities.rifgobierno != null
                )
            {
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }

        private async Task<bool> CIValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(promptContext.Context, () => new UserPersonalData());
            var Validation = utilitario.ValidateNumberCI(promptContext.Context.Activity.Text, ClientData.typeDoc);
            if (!Validation.Equals("OK"))
            {
                await promptContext.Context.SendActivityAsync(Validation, cancellationToken: cancellationToken);
                return false;
            }

            return true;
        }

        private Task<bool> ConfirmValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Context.Activity.Text.ToLower().Equals("si") ||
                promptContext.Context.Activity.Text.ToLower().Equals("no"))
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
        private Activity CreateButtonsCIPass()
        {
            var reply = MessageFactory.Text("Por favor dime si tu tipo de documento de identidad corresponde a Venezolano (V), Extranjero (E), Pasaporte( P), Jurídico (J) o Gubernamental (G)");

            return reply as Activity;
        }

        private async Task<Activity> CreateButtonsConfirm(WaterfallStepContext stepContext, string retry)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            var reply = MessageFactory.Text($"¿Confirmas que el documento de identidad ingresado es correcto?" +
                $"{Environment.NewLine} 🎫 Documento de identidad: {ClientData.typeDoc} {ClientData.ci} {Environment.NewLine}");

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() {Title = "Si", Value = "Si" , Type = ActionTypes.ImBack},
                    new CardAction() {Title = "No", Value = "No" , Type = ActionTypes.ImBack}
                }
            };
            return reply as Activity;
        }
        #endregion Buttons
    }
}

