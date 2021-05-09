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
    public class ReviewDateDialog : CancelDialog
    {
        private BotState _userState;
        private const string _SetFecha = "SetFecha";
        private const string _Confirmacion = "Confirmacion";
        private const string _FinalProcess = "FinalProcess";
        private Utils utilitario = new Utils();
        private UserPersonalData ClientData;

        public ReviewDateDialog(PrivateConversationState userState)
            : base(nameof(ReviewDateDialog))
        {
            _userState = userState;

            var waterfallStep = new WaterfallStep[]
            {
                SetFecha,
                Confirmacion,
                FinalProcess,
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallStep));
            AddDialog(new TextPrompt(_SetFecha, SetFechaValidator));
            AddDialog(new TextPrompt(_Confirmacion, ConfirmValidator));
            AddDialog(new TextPrompt(_FinalProcess));
        }



        #region conversationClaim
        private async Task<DialogTurnResult> SetFecha(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            
            return await stepContext.PromptAsync(
                _SetFecha,
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Por favor ingrese la fecha de la incidencia en el formato \"dd/mm/aaaa \":"),
                    RetryPrompt = MessageFactory.Text("Ingrese una fecha válida, Por favor ingrese la fecha de la incidencia en el formato \"dd/mm/aaaa \":")
                },
                cancellationToken
                );
        }

        private async Task<DialogTurnResult> Confirmacion(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            ClientData.date = stepContext.Context.Activity.Text;
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
                return await stepContext.ReplaceDialogAsync(nameof(ReviewDateDialog), stepContext.Values, cancellationToken);
            }

        }

        #endregion conversationClaim


        #region Validators     
        private Task<bool> SetFechaValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var validate = utilitario.ValidateDate0a120(promptContext.Context.Activity.Text);
            if (validate == 0)
            {
                return Task.FromResult(true);
            }
            else if (validate == 1)
            {
                promptContext.Context.SendActivityAsync($"La fecha del reclamo no puede superar los 120 días de acuerdo al tiempo máximo establecido en la ley.");
                return Task.FromResult(false);
            }
            else
            {
                return Task.FromResult(false);
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
            var reply = MessageFactory.Text($"¿Confirmas que la fecha ingresada es correcta?" +
                $"{Environment.NewLine} 📆 Fecha: {ClientData.date}");

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

