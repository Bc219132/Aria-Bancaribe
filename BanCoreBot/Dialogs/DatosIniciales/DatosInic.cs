using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using BanCoreBot.Infrastructure.Luis;
using System.Threading;
using System.Threading.Tasks;
using BanCoreBot.Common.Utils;
using BanCoreBot.Dialogs.Cancel;
using BanCoreBot.Common.Models.User;
using Luis;

namespace BanCoreBot.Dialogs.DatosIniciales
{
    public class DatosInic : CancelDialog
    {
        private ILuisService _luisService;
        private BotState _userState;
        private const string _SetNombre = "SetNombre";
        private const string _SetCIPass = "SetCIPass";
        private const string _SetCI = "SetCI";
        private const string _TieneTlf = "TieneTlf";
        private const string _SetTlf = "SetTlf";
        private Utils utilitario = new Utils();
        private IStatePropertyAccessor<UserPersonalData> userStateAccessors;
        private UserPersonalData ClientData;

        public DatosInic(PrivateConversationState userState, ILuisService luisService)
            : base(nameof(DatosInic))
        {
            _luisService = luisService;
            _userState = userState;

            var waterfallStep = new WaterfallStep[]
            {
                SetNombre,
                SetCIPass,
                SetCI,
                TieneTlf,
                SetTlf,
                FinalProcess
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallStep));
            AddDialog(new TextPrompt(_SetNombre, NameValidator));
            AddDialog(new TextPrompt(_SetCIPass, CIPassValidator));
            AddDialog(new TextPrompt(_SetCI, CIValidator));
            AddDialog(new TextPrompt(_TieneTlf, TieneTlfValidator));
            AddDialog(new TextPrompt(_SetTlf, TlfValidator));
        }

        #region conversationClaim
        private async Task<DialogTurnResult> SetNombre(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            if (string.IsNullOrEmpty(ClientData.name))
            {
                return await stepContext.PromptAsync(
                _SetNombre,
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Lo primero que necesito saber es ¿cuál es tu nombre y apellido?"),
                    RetryPrompt = MessageFactory.Text("Por favor indicame tu nombre y apellido, no incluyas números ni caracteres especiales.")
                },
                cancellationToken
                );
            }
            else
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> SetCIPass(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());

            if (string.IsNullOrEmpty(ClientData.name))
            {
                ClientData.name = stepContext.Context.Activity.Text;
                await _userState.SaveChangesAsync(stepContext.Context, false, cancellationToken);
            }

            if (string.IsNullOrEmpty(ClientData.typeDoc))
            {
                var option = await stepContext.PromptAsync(
                _SetCIPass,
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Bien, ahora por favor dime si tu tipo de documento de identidad corresponde a Venezolano, Extranjero o Pasaporte"),
                    RetryPrompt = MessageFactory.Text("Tu documento de identidad corresponde a Venezolano, Extranjero o Pasaporte")
                }, cancellationToken);
                return option;
            }
            else
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> SetCI(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(stepContext.Context, cancellationToken).Result;
            
            if (string.IsNullOrEmpty(ClientData.typeDoc))
            {
                if (luisResult.Entities.venezolano != null)
                {
                    ClientData.typeDoc = "Venezolano";
                }
                else if (luisResult.Entities.Extranjero != null)
                {
                    ClientData.typeDoc = "Extranjero";
                }
                else
                {
                    ClientData.typeDoc = "Pasaporte";
                }
            }

            if (string.IsNullOrEmpty(ClientData.ci))
            {
                var auxText = "";
                if (luisResult.Entities.venezolano!=null || luisResult.Entities.Extranjero != null)
                {
                    auxText = "¿Cuál es tu número de cédula?";
                }
                else
                {
                    auxText = "¿Cuál es tu número de pasaporte?";
                }

                return await stepContext.PromptAsync(
                    _SetCI,
                    new PromptOptions { Prompt = MessageFactory.Text(auxText) },
                    cancellationToken
                    );
            }
            else
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> TieneTlf(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            if (string.IsNullOrEmpty(ClientData.ci))
            {
                ClientData.ci = stepContext.Context.Activity.Text;
                await _userState.SaveChangesAsync(stepContext.Context, false, cancellationToken);
            }

            if (string.IsNullOrEmpty(ClientData.phone))
            {

                return await stepContext.PromptAsync(
                    _TieneTlf,
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("¿Tienes algún número de teléfono donde podamos comunicarnos contigo?"),
                        RetryPrompt = MessageFactory.Text("Necesito que me indiques si posees o no algún número de teléfono donde podamos contactarte")
                    },
                cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }

        }

        private async Task<DialogTurnResult> SetTlf(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            var result = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(stepContext.Context, cancellationToken).Result;
            if (result.TopIntent().intent.ToString().Equals("afirmacion"))
            {
                if (string.IsNullOrEmpty(ClientData.phone))
                {
                    var tlf = utilitario.Extractphone(stepContext.Context.Activity.Text);
                    if (!string.IsNullOrEmpty(tlf))
                    {
                        ClientData.phone = tlf;
                    }
                }

                if (string.IsNullOrEmpty(ClientData.phone))
                {

                    return await stepContext.PromptAsync(
                        _SetTlf,
                        new PromptOptions
                        {
                            Prompt = MessageFactory.Text("Como parte de este reclamo te voy a solicitar un número de teléfono (donde nos podamos comunicar contigo) incluyendo el código de área u operadora ejemplo: 04240000000 o 02120000000"),
                            RetryPrompt = MessageFactory.Text("Por favor ingresa tu número de teléfono sin caracteres especiales:")
                        },
                    cancellationToken);
                }
                else
                {
                    return await stepContext.NextAsync(cancellationToken: cancellationToken);
                }
            }
            else
            {
                await stepContext.Context.SendActivityAsync("Sin esta información no es posible generar el reclamo, cuando la poseas, escríbeme nuevamente", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }

        }

        private async Task<DialogTurnResult> FinalProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            if (string.IsNullOrEmpty(ClientData.phone))
            {
                ClientData.phone = stepContext.Context.Activity.Text;
            }
            await _userState.SaveChangesAsync(stepContext.Context, false, cancellationToken);
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }
        #endregion conversationClaim

        

        #region Validators  

        private Task<bool> NameValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            if (utilitario.ValidateName(promptContext.Context.Activity.Text))
            {
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }

        private Task<bool> CIPassValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var result = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(promptContext.Context, cancellationToken).Result;
            if (
                result.Entities.venezolano!= null
                || result.Entities.Extranjero != null
                || result.Entities.pasaporte != null
                )
            {
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }

        private async Task<bool> CIValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(promptContext.Context, () => new UserPersonalData());
            var Validation = utilitario.ValidateNumberCI(promptContext.Context.Activity.Text, ClientData.typeDoc);
            if (!Validation.Equals("OK"))
            {
                await promptContext.Context.SendActivityAsync(Validation, cancellationToken: cancellationToken);
                return false;
            }

            return true;
        }

        private Task<bool> TlfValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(utilitario.Extractphone(promptContext.Context.Activity.Text)))
            {
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }

        private Task<bool> TieneTlfValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
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

        #endregion Validators
    }
}
