using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;
using BanCoreBot.Dialogs.Cancel;
using BanCoreBot.Common.Models.User;

namespace BanCoreBot.Dialogs.Reclamos
{
    public class ReviewConsultaReclamoDialog : CancelDialog
    {
        private BotState _userState;
        private const string _Confirm = "Confirm";
        private const string _ProcessConfirm = "ProcessConfirm";
        private const string _Review = "Review";
        private const string _StartOverAgain = "StartOverAgain";
        private UserPersonalData ClientData;

        public ReviewConsultaReclamoDialog(PrivateConversationState userState)
            : base(nameof(ReviewConsultaReclamoDialog))
        {
            _userState = userState;

            var waterfallStep = new WaterfallStep[]
            {
                Confirm,
                ProcessConfirm,
                Review,
                StartOverAgain
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallStep));
            AddDialog(new TextPrompt(_Confirm, ConfirmValidator));
            AddDialog(new TextPrompt(_ProcessConfirm, ProcessConfirmValidator));
            AddDialog(new TextPrompt(_Review));
            AddDialog(new TextPrompt(_StartOverAgain));
        }

        #region conversationConfirmReview
        private async Task<DialogTurnResult> Confirm(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(
                _Confirm,
                new PromptOptions
                {
                    Prompt = await CreateButtonsConfirm(stepContext),
                    RetryPrompt = await CreateButtonsConfirm(stepContext)
                }, cancellationToken
            );
        }

        private async Task<DialogTurnResult> ProcessConfirm(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Context.Activity.Text.ToLower().Equals("si"))
            {
                return await stepContext.EndDialogAsync(stepContext.Values, cancellationToken);
            }
            else
            {
                return await stepContext.PromptAsync(
                    _ProcessConfirm,
                    new PromptOptions
                    {
                        Prompt = await CreateButtonsReview(stepContext, ""),
                        RetryPrompt = await CreateButtonsReview(stepContext, "Reintento")
                    }, cancellationToken
                    );
            }
        }

        private async Task<DialogTurnResult> Review(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Context.Activity.Text.ToLower().Equals("número de reporte"))
            {
                return await stepContext.BeginDialogAsync(nameof(ReviewNroReclamoDialog), null, cancellationToken);
            }
            else if (stepContext.Context.Activity.Text.ToLower().Equals("documento de identidad"))
            {
                return await stepContext.BeginDialogAsync(nameof(ReviewDocumentIdDialog), null, cancellationToken);
            }
            else
            {
                return await stepContext.EndDialogAsync(stepContext, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> StartOverAgain(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.ReplaceDialogAsync(nameof(ReviewConsultaReclamoDialog), stepContext.Values, cancellationToken);
        }
        #endregion conversationConfirmReview



        #region Buttons
        private async Task<Activity> CreateButtonsConfirm(WaterfallStepContext stepContext)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());

            var text = $"¿Confirmas que los datos suministrados son correctos?" +
                $"{Environment.NewLine} 🎫 Documento de identidad: {ClientData.typeDoc} {ClientData.ci} ";
            if (!String.IsNullOrEmpty(ClientData.nroReclamo)) 
            {
                text = text + $"{Environment.NewLine} 🔢 Número de reporte: {ClientData.nroReclamo}";
            }
            
            var reply = MessageFactory.Text(text);
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

        private async Task<Activity> CreateButtonsReview(WaterfallStepContext stepContext, string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                reply = MessageFactory.Text($"Selecciona el campo que desea corregir");
            }
            else
            {
                reply = MessageFactory.Text($"Por favor indique una respuesta seleccionando alguna de las opciones que se presentan a continuación, ¿Que campo desea corregir?");
            }

            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            reply.SuggestedActions = new SuggestedActions()
            {

                Actions = new List<CardAction>()
                    {
                        new CardAction() { Title = "Documento de Identidad", Value = "Documento de Identidad", Type = ActionTypes.ImBack },
                        new CardAction() { Title = "Número de reporte", Value = "Número de reporte", Type = ActionTypes.ImBack }
                    }
            };
            return reply as Activity;
        }
        #endregion Buttons






        #region Validators
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
        private Task<bool> ProcessConfirmValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Context.Activity.Text.ToLower().Trim().Equals("documento de identidad") ||
                promptContext.Context.Activity.Text.ToLower().Trim().Equals("número de reporte"))

            {
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }

        #endregion Validators

    }
}


