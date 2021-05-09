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
    public class ReviewNumCtaBenefDialog : CancelDialog
    {
        private BotState _userState;
        private const string _SetNumCtaBenef = "SetNumCtaBenef";
        private const string _Confirmacion = "Confirmacion";
        private const string _FinalProcess = "FinalProcess";
        private Utils utilitario = new Utils();
        private UserPersonalData ClientData;

        public ReviewNumCtaBenefDialog(PrivateConversationState userState)
            : base(nameof(ReviewNumCtaBenefDialog))
        {
            _userState = userState;

            var waterfallStep = new WaterfallStep[]
            {
                SetNumCtaBenef,
                Confirmacion,
                FinalProcess,
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallStep));
            AddDialog(new TextPrompt(_SetNumCtaBenef, SetNumCtaValidator));
            AddDialog(new TextPrompt(_Confirmacion, ConfirmValidator));
            AddDialog(new TextPrompt(_FinalProcess));
        }



        #region conversationClaim
        private async Task<DialogTurnResult> SetNumCtaBenef(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(
                _SetNumCtaBenef,
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Por favor ingrese el número de cuenta del beneficiario del otro banco:"),
                    RetryPrompt = MessageFactory.Text("El dato ingresado no es válido, por favor verifique e ingrese solo los veinte (20) dígitos correspondientes al número de cuenta del beneficiario del otro banco:")
                },
                cancellationToken
               );
        }

        private async Task<DialogTurnResult> Confirmacion(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());

            ClientData.numCtaBenef = stepContext.Context.Activity.Text;
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
                return await stepContext.ReplaceDialogAsync(nameof(ReviewNumCtaBenefDialog), stepContext.Values, cancellationToken);
            }

        }

        #endregion conversationClaim


        #region Validators     
        private Task<bool> SetNumCtaValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(utilitario.ValidateNumber20Dig(promptContext.Context.Activity.Text));
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

            var reply = MessageFactory.Text($"¿Confirmas que el número de cuenta ingresado es correcto?" +
                $"{Environment.NewLine} 🔢 Número de cuenta del beneficiario: {ClientData.numCtaBenef}");

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


