using BanCoreBot.Common.Models.User;
using BanCoreBot.Dialogs.Cancel;
using BanCoreBot.Dialogs.Solicitudes;
using BanCoreBot.Infrastructure.Luis;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BanCoreBot.Dialogs.Tarjetas
{
    public class ConsultaSaldoTDC : CancelDialog
    {
        private BotState _userState;
        private ILuisService _luisService;
        private const string _ShowOptions = "ShowOptions";
        private const string _ValidateShowOptions = "ValidateShowOptions";
        private const string _ShowOptionsOK = "ShowOptionsTlf";
        private const string _ValidateShowOptionsOK = "ValidateShowOptionsTlf";
        private UserPersonalData ClientData;

        public ConsultaSaldoTDC(PrivateConversationState userState, ILuisService luisService) : base(nameof(ConsultaSaldoTDC))
        {
            _userState = userState;
            _luisService = luisService;
            var waterfallSteps = new WaterfallStep[]
            {
                ShowOptions,
                ValidateShowOptions,
                ShowOptionsOK,
                ValidateShowOptionsOK
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(_ShowOptions, ValidateInputValidator));
            AddDialog(new TextPrompt(_ValidateShowOptions));
            AddDialog(new TextPrompt(_ShowOptionsOK, InputValidator));
            AddDialog(new TextPrompt(_ValidateShowOptionsOK));
        }


        #region DialogTDC

        private async Task<DialogTurnResult> ShowOptions(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(stepContext.Context, cancellationToken).Result;
            if (luisResult.Entities.disponible != null)
            {
                await stepContext.Context.SendActivityAsync($"Ingresa en Mi Conexión Bancaribe, haz clic en consultas y allí podrás validar El Disponible de tu Tarjeta de Crédito. Para ello, revisa el ítem que dice Crédito Disponible");
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.deuda != null)
            {
                await stepContext.Context.SendActivityAsync($"Ingresa en Mi Conexión Bancaribe, haz clic en consultas y allí podrás validar la deuda de tu Tarjeta de Crédito. Para ello, revisa el ítem que dice Saldo Actual");
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
            else
            {
                var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
                ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
                ClientData.controlDialogvar = true;
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
        }

        private async Task<DialogTurnResult> ValidateShowOptions(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());

            if (ClientData.controlDialogvar)
            {
                var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(stepContext.Context, cancellationToken).Result;
                if (luisResult.Entities.disponible != null)
                {
                    await stepContext.Context.SendActivityAsync($"Ingresa en Mi Conexión Bancaribe, haz clic en consultas y allí podrás validar El Disponible de tu Tarjeta de Crédito. Para ello, revisa el ítem que dice Crédito Disponible");
                    return await stepContext.NextAsync(cancellationToken: cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync($"Ingresa en Mi Conexión Bancaribe, haz clic en consultas y allí podrás validar la deuda de tu Tarjeta de Crédito. Para ello, revisa el ítem que dice Saldo Actual");
                    return await stepContext.NextAsync(cancellationToken: cancellationToken);
                }
            }
            else
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ShowOptionsOK(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            ClientData.controlDialogvar = false;
            var option = await stepContext.PromptAsync(
                _ShowOptionsOK,
                new PromptOptions
                {
                    Prompt = CreateButtonsOK(""),
                    RetryPrompt = CreateButtonsOK("Reintento")
                }, cancellationToken
            );
            return option;
        }

        private async Task<DialogTurnResult> ValidateShowOptionsOK(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(stepContext.Context, cancellationToken).Result;
            if (luisResult.TopIntent().intent.ToString().Equals("afirmacion"))
            {
                await stepContext.Context.SendActivityAsync("¿Existe algo más en lo que te pueda ayudar?", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
            else
            {
                var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
                ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
                ClientData.request = "dudasaldotdc";
                ClientData.TextoDesborde = stepContext.Context.Activity.Text;
                await stepContext.Context.SendActivityAsync($"En este caso puedo tomar tus datos y enviar tu consulta al área encargada", cancellationToken: cancellationToken);
                return await stepContext.BeginDialogAsync(nameof(SolicitudesClientes), cancellationToken: cancellationToken);

            }
        }

        #endregion DialogTDC


        #region Validators

        private Task<bool> InputValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var result = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(promptContext.Context, cancellationToken).Result;
            if (
                result.TopIntent().intent.ToString().Equals("afirmacion")
               || result.TopIntent().intent.ToString().Equals("negación")
                )

            {
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }

        private Task<bool> ValidateInputValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var result = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(promptContext.Context, cancellationToken).Result;
            if (
                result.Entities.deuda != null
               || result.Entities.disponible != null
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
                reply = MessageFactory.Text("¿Me puedes indicar si deseas consultar la deuda de tu tarjeta de crédito o el disponible? ");
            }
            else
            {
                reply = MessageFactory.Text("Disculpa, para responder tu pregunta anterior necesito me indiques si deseas consultar la deuda de tu tarjeta de crédito o el saldo disponible");
            }

            return reply as Activity;
        }


        private Activity CreateButtonsOK(string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                reply = MessageFactory.Text("¿pude resolver tu duda?");
            }
            else
            {
                reply = MessageFactory.Text("Por favor, ¿me confirmas si te sirvió la información que te brinde?");
            }

            return reply as Activity;
        }
        #endregion Buttons

    }
}

