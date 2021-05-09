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

namespace BanCoreBot.Dialogs.Reclamos.Review
{
    public class ReviewPhoneRefillDialog : CancelDialog
    {
        private BotState _userState;
        private const string _SetTlfRecarga = "SetTlfRecarga";
        private const string _Confirmacion = "Confirmacion";
        private const string _FinalProcess = "FinalProcess";
        private Utils utilitario = new Utils();
        private UserPersonalData ClientData;

        public ReviewPhoneRefillDialog(PrivateConversationState userState)
            : base(nameof(ReviewPhoneRefillDialog))
        {
            _userState = userState;

            var waterfallStep = new WaterfallStep[]
            {
                SetTlfRecarga,
                Confirmacion,
                FinalProcess,
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallStep));
            AddDialog(new TextPrompt(_SetTlfRecarga, SetSecondPhoneValidator));
            AddDialog(new TextPrompt(_Confirmacion, ConfirmValidator));
            AddDialog(new TextPrompt(_FinalProcess));
        }



        #region conversationClaim

        private async Task<DialogTurnResult> SetTlfRecarga(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(
                    _SetTlfRecarga,
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Por favor ingresa tu número celular a recargar incluyendo el código de la operadora (0424, 0414, 0412, 0426, 0416):"),
                        RetryPrompt = MessageFactory.Text("Por favor ingresa el número celular a recargar sin caracteres especiales:")
                    },
                    cancellationToken
                    );
        }

        private async Task<DialogTurnResult> Confirmacion(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            ClientData.phoneRefill = stepContext.Context.Activity.Text;
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
                return await stepContext.ReplaceDialogAsync(nameof(ReviewPhoneRefillDialog), stepContext.Values, cancellationToken);
            }
        }

        #endregion conversationClaim


        #region Validators     
        private async Task<bool> SetSecondPhoneValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            if (utilitario.ValidateNumberPhone(promptContext.Context.Activity.Text))
            {
                return await Task.FromResult(true);
            }
            else
            {
                return await Task.FromResult(false);
            }
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
        private async Task<Activity> CreateButtonsConfirm(WaterfallStepContext stepContext, string retry)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            var reply = MessageFactory.Text($"¿Confirmas que el celular ingresado es correcto?" +
                $"{Environment.NewLine} 📱 Celular a recargar: {ClientData.phoneRefill}");

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


