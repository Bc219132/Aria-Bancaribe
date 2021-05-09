using BanCoreBot.Dialogs.Cancel;
using BanCoreBot.Infrastructure.Luis;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using System.Threading.Tasks;
using BanCoreBot.Common.Models.User;

namespace BanCoreBot.Dialogs.Agencias
{
    public class AgenciaDialog : CancelDialog
    {
        private readonly ILuisService _luisService;
        private BotState _userState;
        private const string _ValidateInput = "ValidateInput";
        private const string _ValidateOption = "ValidateOption";
        private IStatePropertyAccessor<UserPersonalData> userStateAccessors;
        private UserPersonalData ClientData;

        public AgenciaDialog(PrivateConversationState userState, ILuisService luisService) : base(nameof(AgenciaDialog))
        {
            _luisService = luisService;
            _userState = userState;
            var waterfallSteps = new WaterfallStep[]
            {
                ValidateInput,
                ValidateOption
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(_ValidateInput));
            AddDialog(new TextPrompt(_ValidateOption));
        }


        #region DialogCtas
        private async Task<DialogTurnResult> ValidateInput(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            

            if (ClientData.request.Equals("saber"))
            {
                await stepContext.Context.SendActivityAsync($"¿Qué tipo de información quieres saber?", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else if (ClientData.request.Equals("cercana"))
            {
                await stepContext.Context.SendActivityAsync($"¿En donde te ubicas?, para ayudarte", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else if (ClientData.request.Equals("numero"))
            {
                await stepContext.Context.SendActivityAsync($"!Claro¡ ¿De que agencia?", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync($"¿Qué tipo de información quieres saber?", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
        }



        private async Task<DialogTurnResult> ValidateOption(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(stepContext.Context, cancellationToken).Result;
            if (luisResult.Entities.Agencias != null)
            {
                await stepContext.Context.SendActivityAsync($"La dirección es: Av.Casanova entre Av.Venezuela con Calle El Recreo, Sabana Grande, " +
                       $"Parroquia el Recreo, Municipio Libertador. Tambien puedes " +
                       $"comunicarte a los numeros :  02127632050 / 02127633986 ", cancellationToken: cancellationToken);

                await stepContext.Context.SendActivityAsync($"En este momento está oficina se encuentra temporalmente cerrada. Le sugiero dirigirse a otra sede cercana", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync("Entiendo, si hay algo en lo que te pueda ayudar solo me lo debes indicar y con gusto te ayudaré", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
            
        }

        #endregion DialogCtas

    }
}
