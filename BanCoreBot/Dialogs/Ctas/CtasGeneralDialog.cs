using BanCoreBot.Dialogs.Cancel;
using BanCoreBot.Infrastructure.Luis;
using Luis;
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

namespace BanCoreBot.Dialogs.Ctas
{
    public class CtasGeneralDialog : CancelDialog
    {
        private BotState _userState;
        private ILuisService _luisService;
        private const string _ShowOptions = "ShowOptions";
        private const string _ValidateShowOptions = "ValidateShowOptions";
        private UserPersonalData ClientData;
        private readonly IQnAMakerAIService _qnAMakerAIService;

        public CtasGeneralDialog(ILuisService luisService, PrivateConversationState userState, IQnAMakerAIService qnAMakerAIService)
            : base(nameof(CtasGeneralDialog))
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
            if (stepContext.Context.Activity.Text.Equals("Natural"))
            {
                stepContext.Context.Activity.Text = "solicitud de una cuenta natural";
            }
            if (stepContext.Context.Activity.Text.Equals("Jurídica"))
            {
                stepContext.Context.Activity.Text = "solicitud de una cuenta jurídica";
            }
            if (stepContext.Context.Activity.Text.Equals("Moneda Extranjera"))
            {
                stepContext.Context.Activity.Text = "solicitud de una cuenta para chamos";
            }
            if (stepContext.Context.Activity.Text.Equals("Chamos"))
            {
                stepContext.Context.Activity.Text = "solicitud de una cuenta en moneda extranjera";
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
              promptContext.Context.Activity.Text.Equals("Natural")
              || promptContext.Context.Activity.Text.Equals("Jurídica")
              || promptContext.Context.Activity.Text.Equals("Moneda Extranjera")
              || promptContext.Context.Activity.Text.Equals("Chamos")
              || promptContext.Context.Activity.Text.Equals("solicitud de una cuenta natural")
              || promptContext.Context.Activity.Text.Equals("solicitud de una cuenta jurídica")
              || promptContext.Context.Activity.Text.Equals("solicitud de una cuenta para chamos")
              || promptContext.Context.Activity.Text.Equals("solicitud de una cuenta en moneda extranjera")
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
                reply = MessageFactory.Text("¿Qué tipo de cuenta te interesa?");
            }
            else
            {
                reply = MessageFactory.Text("Por favor, selecciona el tipo de cuenta que te interesa");
            }

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() {Title = "Natural", Value = "solicitud de una cuenta natural" , Type = ActionTypes.ImBack},
                    new CardAction() {Title = "Jurídica", Value = "solicitud de una cuenta jurídica" , Type = ActionTypes.ImBack},
                    new CardAction() {Title = "Moneda Extranjera", Value = "solicitud de una cuenta en moneda extranjera" , Type = ActionTypes.ImBack},
                    new CardAction() {Title = "Chamos", Value = "solicitud de una cuenta para chamos" , Type = ActionTypes.ImBack}
                }
            };
            return reply as Activity;
        }

        #endregion Buttons

    }
}
