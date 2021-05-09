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

namespace BanCoreBot.Dialogs.Tarjetas
{
    public class RazonBloqueoDialog : CancelDialog
    {
        private BotState _userState;
        private ILuisService _luisService;
        private const string _ShowOptions = "ShowOptions";
        private const string _ValidateShowOptions = "ValidateShowOptions";
        private UserPersonalData ClientData;
        public RazonBloqueoDialog(ILuisService luisService, PrivateConversationState userState)
            : base(nameof(RazonBloqueoDialog))
        {
            _userState = userState;
            _luisService = luisService;
            var waterfallSteps = new WaterfallStep[]
            {
                ShowOptions,
                ValidateShowOptions
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(_ShowOptions));
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

            var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(stepContext.Context, cancellationToken).Result;
            if (luisResult.Entities.Robo != null || luisResult.Entities.opcion1List != null)
            {
                ClientData.request = "robotdc";
                await stepContext.Context.SendActivityAsync($"Lamento leer eso, con  mucho gusto te ayudo, voy a solicitarte algunos datos", cancellationToken: cancellationToken);

            }
            else if (luisResult.Entities.Extravio != null || luisResult.Entities.opcion2List != null)
            {// Tarjeta de Débito
                ClientData.request = "perdidatdc";
                await stepContext.Context.SendActivityAsync($"Que mal, con  mucho gusto te ayudo, voy a solicitarte algunos datos", cancellationToken: cancellationToken);
            }
            else
            {
                ClientData.request = "bloqueotdc"; 
                await stepContext.Context.SendActivityAsync($"Con  mucho gusto te ayudo, voy a solicitarte algunos datos", cancellationToken: cancellationToken);

            }
            return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);

        }

        #endregion DialogCtas


        #region Buttons

        private Activity CreateButtons(string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                reply = MessageFactory.Text("¿Por qué motivo deseas bloquear tu tarjeta de crédito, por robo, extravió o deseas cancelar tu tarjeta de crédito por otro motivo?");
            }
            else
            {
                reply = MessageFactory.Text("¿Por qué motivo deseas bloquear tu tarjeta de crédito?");
            }

            return reply as Activity;
        }

        #endregion Buttons

    }
}
