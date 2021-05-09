using BanCoreBot.Dialogs.Cancel;
using BanCoreBot.Infrastructure.Luis;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BanCoreBot.Dialogs.MontosMaximos
{
    public class MontoMaximoGeneral : CancelDialog
    {
        private ILuisService _luisService;
        private const string _ShowOptions = "ShowOptions";
        private const string _ValidateShowOptions = "ValidateShowOptions";
        private const string _ShowOptionsTjta = "ShowOptionsTjta";
        private const string _ValidateShowOptionsTjta = "ValidateShowOptionsTjta";
        private const string _ShowOptionsTransf = "ShowOptionsTransf";
        private const string _ValidateShowOptionsTransf = "ValidateShowOptionsTransf";
        private string optionSelected = "";
        public MontoMaximoGeneral(ILuisService luisService) : base(nameof(MontoMaximoGeneral))
        {
            _luisService = luisService;
            var waterfallSteps = new WaterfallStep[]
            {
                ShowOptions,
                ValidateShowOptions,
                ShowOptionsTjta,
                ValidateShowOptionsTjta,
                ShowOptionsTransf,
                ValidateShowOptionsTransf
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(_ShowOptions, ValidateInputValidator));
            AddDialog(new TextPrompt(_ValidateShowOptions));
            AddDialog(new TextPrompt(_ShowOptionsTjta, ValidateInputValidatorTjta));
            AddDialog(new TextPrompt(_ValidateShowOptionsTjta));
            AddDialog(new TextPrompt(_ShowOptionsTransf, ValidateInputValidatorTransf));
            AddDialog(new TextPrompt(_ValidateShowOptionsTransf));
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
            var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(stepContext.Context, cancellationToken).Result;
            if (luisResult.Entities.pagorapido != null || luisResult.Entities.mipagobancaribe != null || luisResult.Entities.opcionultimaList != null
                || (luisResult.Entities.transferir != null && luisResult.Entities.sms != null || luisResult.Entities.sms != null)
                )
            { //MontoMaximoPagoMovil
                await stepContext.Context.SendActivityAsync($"📌 El Máximo Diario para realizar Pago Bancaribe entre Cuentas Naturales es de: 700.000.000,00 BsS {Environment.NewLine}" +
                    $"📌 El Máximo Diario para realizar Pago Bancaribe de Cuenta Persona Natural a Cuenta Comercio es de: 800.000.000,00 BsS {Environment.NewLine}" +
                    $"📌 El Máximo Diario para realizar Pago Bancaribe de Cuenta Comercio a Persona Natural es de: 1.000.000.000,00 BsS", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.cajero != null || luisResult.Entities.opcion2List != null)
            {  //MontoMaximoRetiroEnCajero
                await stepContext.Context.SendActivityAsync($"Hasta la fecha el Máximo Diario para  Retirar en Cajero Bancaribe es de: 30.000,00BsS, si deseas retirar por nuestros cajeros Bancaribe con una Tarjeta de Debito de otra entidad Bancaria es monto Máximo Diario es de : 5.000,00BsS", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.tdd != null || luisResult.Entities.tdc != null || luisResult.Entities.tarjeta != null || luisResult.Entities.pos != null || luisResult.Entities.opcion3List != null)
            {  //Tarjetas
                AuxClass.SelectedOption = "Tarjeta";
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
            else
            {  //Transferencias
                AuxClass.SelectedOption = "Transferencia";
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }

        }


        private async Task<DialogTurnResult> ShowOptionsTjta(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            optionSelected = AuxClass.SelectedOption;
            if (optionSelected.Equals("Tarjeta"))
            {
                var option = await stepContext.PromptAsync(
                     _ShowOptionsTjta,
                     new PromptOptions
                     {
                         Prompt = CreateButtonsTjta(""),
                         RetryPrompt = CreateButtonsTjta("Reintento")
                     }, cancellationToken
                     );
                return option;
            }
            else
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
        }


        private async Task<DialogTurnResult> ValidateShowOptionsTjta(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            optionSelected = AuxClass.SelectedOption;
            if (optionSelected.Equals("Tarjeta"))
            {
                var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(stepContext.Context, cancellationToken).Result;
                if (luisResult.Entities.tdd != null || luisResult.Entities.opcion1List != null)
                {
                    await stepContext.Context.SendActivityAsync($"Hasta la fecha el Máximo Diario para hacer uso de su Tarjeta de Débito por Punto de Venta es: 1.000.000.000,00BsS", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync($"En la Tarjeta de Crédito varia el disponible que se puede usar por Punto de Venta, eso dependerá del Límite que posea tu Tarjeta. ", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
            }
            else
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ShowOptionsTransf(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var option = await stepContext.PromptAsync(
                _ShowOptionsTransf,
                new PromptOptions
                {
                    Prompt = CreateButtonsTransf(""),
                    RetryPrompt = CreateButtonsTransf("Reintento")
                }, cancellationToken
                );
            return option;
        }

        private async Task<DialogTurnResult> ValidateShowOptionsTransf(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(stepContext.Context, cancellationToken).Result;
            if (luisResult.Entities.juridico != null || luisResult.Entities.opcion2List != null || luisResult.Entities.opcionultimaList != null)
            {
                await stepContext.Context.SendActivityAsync($"Hasta la fecha el Máximo Diario para  transferencia es de:{ Environment.NewLine} " +
                    $"📌 Transferencia Propias a Bancaribe: no existe límite de monto {Environment.NewLine}" +
                    $"📌 Transferencia Terceros Bancaribe: no existe límite de monto { Environment.NewLine}" +
                    $"📌 Transferencia a otros Bancos: 7.000.000.000,00BsS", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync($"Hasta la fecha el Máximo Diario para  transferencia es de:{ Environment.NewLine} " +
                $"📌 Transferencia Propias a Bancaribe:  no existe límite de monto {Environment.NewLine}" +
                $"📌 Transferencia Terceros Bancaribe:  no existe límite de monto {Environment.NewLine}" +
                $"📌 Transferencia a otros Bancos: 1.000.000.000,00BsS, ten en cuenta que por motivos de seguridad al transferir a otros bancos el sistema no dejará transferir el máximo, te sugiero realizar tus transferencias de manera fraccionadas, c/u de  500.000.000,00 Bs o montos menores", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
        }
        #endregion DialogCtas


        #region Validators


        private Task<bool> ValidateInputValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var luisresult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(promptContext.Context, cancellationToken).Result;
            if (
                luisresult.TopIntent().intent.ToString().Equals("TipoTarjeta")
                ||
               luisresult.TopIntent().intent.ToString().Equals("Opciones")
                ||
                luisresult.TopIntent().intent.ToString().Equals("ServiciosMontoLimite")
                ||
                luisresult.TopIntent().intent.ToString().Equals("transferencia")
                ||
                luisresult.Entities.pagorapido != null || luisresult.Entities.mipagobancaribe != null
                ||
                luisresult.Entities.sms != null || luisresult.Entities.transferir != null
                ||
                luisresult.Entities.tdd != null || luisresult.Entities.tdc != null
                ||
                luisresult.Entities.tarjeta != null || luisresult.Entities.pos != null)

            {
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }

        private Task<bool> ValidateInputValidatorTjta(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var luisresult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(promptContext.Context, cancellationToken).Result;
            if (
                luisresult.TopIntent().intent.ToString().Equals("TipoTarjeta")
                ||
                luisresult.TopIntent().intent.ToString().Equals("Opciones")
                || luisresult.Entities.tdc != null
                || luisresult.Entities.tdd != null
               )

            {
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }

        private Task<bool> ValidateInputValidatorTransf(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var luisresult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(promptContext.Context, cancellationToken).Result;

            if (
                luisresult.TopIntent().intent.ToString().Equals("TipoPersona") ||
                luisresult.TopIntent().intent.ToString().Equals("Opciones") ||
                luisresult.Entities.natural != null ||
                luisresult.Entities.juridico != null
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
                reply = MessageFactory.Text("¿De qué tipo de operación deseas saber el monto máximo? ¿Transferencias, Retiro en Cajero, Pago con Tarjetas o Mi Pago Bancaribe?");
            }
            else
            {
                reply = MessageFactory.Text("Por favor, me indicas el tipo de operación (Transferencias, Retiro en Cajero, Pago con Tarjetas o Mi Pago Bancaribe) para poder decirte el monto máximo que puedes procesar.");
            }

            return reply as Activity;
        }

        private Activity CreateButtonsTjta(string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                reply = MessageFactory.Text("¿Deseas saber esta información de una tarjeta de débito o una tarjeta de crédito?");
            }
            else
            {
                reply = MessageFactory.Text("Por favor, me indicas el tipo de tarjeta (débito o crédito) para poder darte el monto máximo a consumir");
            }
            return reply as Activity;
        }

        private Activity CreateButtonsTransf(string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                reply = MessageFactory.Text("¿Deseas saber esta información de una Cuenta Natural o una Cuenta Jurídica?");
            }
            else
            {
                reply = MessageFactory.Text("Por favor, me indicas el tipo de cuenta (natural o jurídica) para poder darte el monto máximo a transferir");
            }

            return reply as Activity;
        }

        #endregion Buttons

    }
}
