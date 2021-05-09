using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Threading;
using System.Threading.Tasks;
using BanCoreBot.Common.Utils;
using BanCoreBot.Dialogs.Cancel;
using System.Collections.Generic;
using BanCoreBot.Common.Models.User;

namespace BanCoreBot.Dialogs.Reclamos.Review
{
    public class Review4UltTjtaDialog : CancelDialog
    {
        private BotState _userState;
        private const string _Set4DigTjta = "Set4DigTjta";
        private const string _Confirmacion = "Confirmacion";
        private const string _FinalProcess = "FinalProcess";
        private Utils utilitario = new Utils();
        private UserPersonalData ClientData;

        public Review4UltTjtaDialog(PrivateConversationState userState)
            : base(nameof(Review4UltTjtaDialog))
        {
            _userState = userState;
            var waterfallStep = new WaterfallStep[]
            {
                Set4DigTjta,
                Confirmacion,
                FinalProcess,
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallStep));
            AddDialog(new TextPrompt(_Set4DigTjta, Set4DigTjtaValidator));
            AddDialog(new TextPrompt(_Confirmacion, ConfirmValidator));
            AddDialog(new TextPrompt(_FinalProcess));
        }



        #region conversationClaim

        private async Task<DialogTurnResult> Set4DigTjta(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            if (ClientData.code.Equals("2.21") || ClientData.code.Equals("2.20") || ClientData.code.Equals("2.18"))
            {
                return await stepContext.PromptAsync(
                  _Set4DigTjta,
                  new PromptOptions
                  {
                      Prompt = MessageFactory.Text("Por favor ingrese los últimos cuatro (4) dígitos de la tarjeta de crédito:"),
                      RetryPrompt = MessageFactory.Text("El dato ingresado no es válido, por favor ingrese solo los cuatro (4) últimos números de la tarjeta de crédito:")
                  },
                  cancellationToken
                  );
            }
            else
            {
                return await stepContext.PromptAsync(
                    _Set4DigTjta,
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Por favor ingrese los últimos cuatro (4) dígitos de la tarjeta de debito:"),
                        RetryPrompt = MessageFactory.Text("El dato ingresado no es válido, por favor ingrese solo los cuatro (4) últimos números de la tarjeta de debito:")
                    },
                    cancellationToken
                    );
            }
        }

        private async Task<DialogTurnResult> Confirmacion(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            ClientData.ult4DigTjta = stepContext.Context.Activity.Text;
            return await stepContext.PromptAsync(
                _Confirmacion,
                new PromptOptions
                {
                    Prompt = await CreateButtonsConfirm(stepContext, "" ),
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
                return await stepContext.ReplaceDialogAsync(nameof(Review4UltTjtaDialog), stepContext.Values, cancellationToken);
            }

        }

        #endregion conversationClaim


        #region Validators     
        private Task<bool> Set4DigTjtaValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(utilitario.ValidateNumber4Dig(promptContext.Context.Activity.Text));
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
            var reply = MessageFactory.Text($"¿Confirmas que los números ingresados corresponden  a los cuatro (4) últimos dígitos de la tarjeta?" +
                $"{Environment.NewLine} 🔢 Últimos 4 dígitos de la tarjeta: {ClientData.ult4DigTjta}");

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



