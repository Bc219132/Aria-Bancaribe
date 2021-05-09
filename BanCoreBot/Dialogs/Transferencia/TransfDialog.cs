using BanCoreBot.Dialogs.Cancel;
using BanCoreBot.Infrastructure.Luis;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BanCoreBot.Dialogs.Transferencia
{
    public class TransfDialog : CancelDialog
    {
        private ILuisService _luisService;
        private const string _ShowOptions = "ShowOptions";
        private const string _ValidateShowOptions = "ValidateShowOptions";
        public TransfDialog(ILuisService luisService) : base(nameof(TransfDialog))
        {
            _luisService = luisService;
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
                    Prompt = CreateButtonsTlf(""),
                    RetryPrompt = CreateButtonsTlf("Reintento")
                }, cancellationToken
            );
            return option;
        }

        private async Task<DialogTurnResult> ValidateShowOptions(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var option = stepContext.Context.Activity.Text;
            if (option.ToLower().Equals("si"))
            {
                await stepContext.Context.SendActivityAsync($"Para realizar una Transferencia, debes poseer tu Tarjeta Conexión Segura Activa al igual que tu Perfil de Seguridad y ya debes poseer la cuenta afiliada. Si posees esta serie de requisitos tienes que ingresar simplemente a  Mi conexión Bancaribe, persona natural, ingresa tus datos de acceso. Una vez dentro de tu cuenta, dirigirte a la sección de \"Transferencias\", selecciona si es \"Cuenta Propia\", a \"Terceros Bancaribe\" o \"Otro Banco\" y elige a que cuentas deseas realizar la transferencia. ", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
            else
            {
                var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(stepContext.Context, cancellationToken).Result;
                if (luisResult.TopIntent().intent.ToString().Equals("afirmacion"))
                {
                    await stepContext.Context.SendActivityAsync($"Para realizar una Transferencia, debes poseer tu Tarjeta Conexión Segura Activa al igual que tu Perfil de Seguridad y ya debes poseer la cuenta afiliada. Si posees esta serie de requisitos tienes que ingresar simplemente a  Mi conexión Bancaribe, persona natural, ingresa tus datos de acceso. Una vez dentro de tu cuenta, dirigirte a la sección de \"Transferencias\", selecciona si es \"Cuenta Propia\", a \"Terceros Bancaribe\" o \"Otro Banco\" y elige a que cuentas deseas realizar la transferencia. ", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync("¿Puedes ser un poco más específico? No logro entender que me estas solicitando", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
            }
        }

        #endregion DialogCtas


        #region Validators


        private Task<bool> ValidateInputValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            if (
                _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(promptContext.Context, cancellationToken).Result.TopIntent().intent.ToString().Equals("afirmacion")
               || _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(promptContext.Context, cancellationToken).Result.TopIntent().intent.ToString().Equals("negación")

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

        private Activity CreateButtonsTlf(string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                reply = MessageFactory.Text(null);
            }
            else
            {
                reply = MessageFactory.Text("¿Deseas conocer como realizar una transferencia?");
            }

            return reply as Activity;
        }

        #endregion Buttons

    }
}
