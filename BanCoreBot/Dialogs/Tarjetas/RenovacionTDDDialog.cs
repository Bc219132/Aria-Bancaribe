using BanCoreBot.Dialogs.Cancel;
using BanCoreBot.Infrastructure.Luis;
using BanCoreBot.Infrastructure.QnAMakerAI;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Threading;
using System.Threading.Tasks;
using BanCoreBot.Common.Models.User;
using System.Collections.Generic;
using System.Linq;

namespace BanCoreBot.Dialogs.Tarjetas
{
    public class RenovacionTDDDialog : CancelDialog
    {
        private BotState _userState;
        private ILuisService _luisService;
        private const string _ShowOptions = "ShowOptions";
        private const string _ValidateShowOptions = "ValidateShowOptions";
        private UserPersonalData ClientData;
        private readonly IQnAMakerAIService _qnAMakerAIService;

        public RenovacionTDDDialog(ILuisService luisService, PrivateConversationState userState, IQnAMakerAIService qnAMakerAIService)
            : base(nameof(RenovacionTDDDialog))
        {
            _userState = userState;
            _luisService = luisService;
            _qnAMakerAIService = qnAMakerAIService;
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
                    Prompt = CreateButtons(""),
                    RetryPrompt = CreateButtons("Reintento")
                }, cancellationToken
            );
            return option;
        }

        private async Task<DialogTurnResult> ValidateShowOptions(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            if (stepContext.Context.Activity.Text.ToLower().Equals("deterioro")
                          || stepContext.Context.Activity.Text.ToLower().Equals("vencimiento")
                          || stepContext.Context.Activity.Text.ToLower().Equals("robo")
                          || stepContext.Context.Activity.Text.ToLower().Equals("extravío"))
            {
                stepContext.Context.Activity.Text = stepContext.Context.Activity.Text + " de tarjeta de débito";
            }

            var resultQnA = await _qnAMakerAIService._qnAMakerResult.GetAnswersAsync(stepContext.Context);
            var score = resultQnA.FirstOrDefault()?.Score;
            string response = resultQnA.FirstOrDefault()?.Answer;
            await stepContext.Context.SendActivityAsync(response, cancellationToken: cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        #endregion DialogCtas


        #region Validators


        private Task<bool> ValidateInputValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            if (
              promptContext.Context.Activity.Text.ToLower().Equals("deterioro de tarjeta de débito")
              || promptContext.Context.Activity.Text.ToLower().Equals("vencimiento de tarjeta de débito")
              || promptContext.Context.Activity.Text.ToLower().Equals("robo de tarjeta de débito")
              || promptContext.Context.Activity.Text.ToLower().Equals("extravío de tarjeta de débito")
              || promptContext.Context.Activity.Text.ToLower().Equals("deterioro")
              || promptContext.Context.Activity.Text.ToLower().Equals("vencimiento")
              || promptContext.Context.Activity.Text.ToLower().Equals("robo")
              || promptContext.Context.Activity.Text.ToLower().Equals("extravío")
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
                reply = MessageFactory.Text("¿Selecciona la razón por la cual estás solicitando la renovación de la tarjeta?");
            }
            else
            {
                reply = MessageFactory.Text("Por favor, selecciona la razón por la cual estás solicitando la renovación de la tarjeta");
            }

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() {Title = "Deterioro", Value = "Deterioro de tarjeta de débito" , Type = ActionTypes.ImBack},
                    new CardAction() {Title = "Vencimiento", Value = "Vencimiento de tarjeta de débito" , Type = ActionTypes.ImBack},
                    new CardAction() {Title = "Robo", Value = "Robo de tarjeta de débito" , Type = ActionTypes.ImBack},
                    new CardAction() {Title = "Extravío", Value = "Extravío de tarjeta de débito" , Type = ActionTypes.ImBack}
                }
            };
            return reply as Activity;
        }

        #endregion Buttons

    }
}
