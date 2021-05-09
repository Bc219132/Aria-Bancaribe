
using BanCoreBot.Common.Models.Claim;
using BanCoreBot.Data;
using BanCoreBot.Infrastructure.SendGrid;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BanCoreBot.Common.Utils;
using BanCoreBot.Common.Models.User;
using BanCoreBot.Dialogs.Cancel;

namespace BanCoreBot.Dialogs.Reclamos
{
    public class ReclamoConexErrorContact : CancelDialog
    {
        private readonly IStatePropertyAccessor<ClaimAllModels> _claimstate;
        private readonly IStatePropertyAccessor<UserPersonalData> _userState;
        private readonly IDataBaseService _dataBaseService;
        public static ClaimAllModels ClaimModel = new ClaimAllModels();
        private readonly ISendGridEmailService _sendGridEmailService;
        private const string _SetSecondAlt = "SetSecondAlt";
        private const string _SetSecondPhone = "SetSecondPhone";
        private const string _SetEmail = "SetEmail";
        private const string _SetDescripcion = "SetDescripcion";
        private const string _Confirmacion = "Confirmacion";
        private Utils utilitario = new Utils();

        public ReclamoConexErrorContact(IDataBaseService dataBaseService, ISendGridEmailService sendGridEmailService, UserState userState)
            : base(nameof(ReclamoConexErrorContact))
        {
            _dataBaseService = dataBaseService;
            _sendGridEmailService = sendGridEmailService;
            _claimstate = userState.CreateProperty<ClaimAllModels>(nameof(ClaimAllModels));
            _userState = userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));

            var waterfallStep = new WaterfallStep[]
            {
                SetSecondAlt,
                SetSecondPhone,
                SetEmail,
                SetDescripcion,
                Confirmacion,
                FinalProcess
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallStep));
            AddDialog(new TextPrompt(_SetSecondAlt, OtherPhoneValidator));
            AddDialog(new TextPrompt(_SetSecondPhone, SetSecondPhoneValidator));
            AddDialog(new TextPrompt(_SetEmail, EmailValidator));
            AddDialog(new TextPrompt(_SetDescripcion));
            AddDialog(new TextPrompt(_Confirmacion, ConfirmValidator));
        }


        #region conversationClaim

        private async Task<DialogTurnResult> SetSecondAlt(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateModel = await _userState.GetAsync(stepContext.Context, () => new UserPersonalData());
            await stepContext.Context.SendActivityAsync($"Estimado(a) {userStateModel.name}, posee algún otro número de contacto adicional al {userStateModel.phone}");
            return await stepContext.PromptAsync(
                _SetSecondAlt,
                new PromptOptions
                {
                    Prompt = CreateButtonsPhone(""),
                    RetryPrompt = CreateButtonsPhone("Retry")
                }, cancellationToken
            );
        }

        private async Task<DialogTurnResult> SetSecondPhone(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateModel = await _claimstate.GetAsync(stepContext.Context, () => new ClaimAllModels());
            if (stepContext.Context.Activity.Text.ToString().ToLower().Equals("si"))
            {
                userStateModel.hasOtherPhone = true;
                return await stepContext.PromptAsync(
                    _SetSecondPhone,
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Por favor ingresa tu número de teléfono incluyendo el código de área u operadora:"),
                        RetryPrompt = MessageFactory.Text("Por favor ingresa tu número de teléfono sin caracteres especiales:")
                    },
                    cancellationToken
                    );
            }
            else
            {
                userStateModel.hasOtherPhone = false;
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> SetEmail(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateModel = await _userState.GetAsync(stepContext.Context, () => new UserPersonalData());
            var userClaimModel = await _claimstate.GetAsync(stepContext.Context, () => new ClaimAllModels());

            userClaimModel.claimName = ClaimModel.claimName = "Formulario Error de Conexión  Bot";
            userClaimModel.code = ClaimModel.code = "0.00";
            ClaimModel.fullName = userStateModel.name;
            ClaimModel.typeDoc = userStateModel.typeDoc;
            ClaimModel.ci = userStateModel.ci;
            ClaimModel.phone = userStateModel.phone;
            ClaimModel.hasOtherPhone = userClaimModel.hasOtherPhone;

            if (userClaimModel.hasOtherPhone)
            {
                userClaimModel.phoneAlt = stepContext.Context.Activity.Text;
                ClaimModel.phoneAlt = userClaimModel.phoneAlt;
            }

            return await stepContext.PromptAsync(
                _SetEmail,
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Por favor ingresa tu dirección de correo electrónico:"),
                    RetryPrompt = MessageFactory.Text("El formato del correo ingresado no es correcto por favor verifica e ingresa nuevamente tu dirección de correo electrónico:")
                },
                cancellationToken
                );
        }

        private async Task<DialogTurnResult> SetDescripcion(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateModel = await _claimstate.GetAsync(stepContext.Context, () => new ClaimAllModels());
            userStateModel.ult4DigTjta = stepContext.Context.Activity.Text;
            ClaimModel.ult4DigTjta = userStateModel.ult4DigTjta;
            return await stepContext.PromptAsync(
                _SetDescripcion,
                new PromptOptions { Prompt = MessageFactory.Text("Por favor ingresa una breve descripción de lo que aconteció:") },
                cancellationToken
                );
        }


        private async Task<DialogTurnResult> Confirmacion(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateModel = await _claimstate.GetAsync(stepContext.Context, () => new ClaimAllModels());
            userStateModel.Description = stepContext.Context.Activity.Text;
            ClaimModel.Description = userStateModel.Description;
            var option = await stepContext.PromptAsync(
                _Confirmacion,
                new PromptOptions
                {
                    Prompt = CreateButtonsConfirm(""),
                    RetryPrompt = CreateButtonsConfirm("")
                }, cancellationToken
            );
            return option;

        }
        private async Task<DialogTurnResult> FinalProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Context.Activity.Text.ToLower().Equals("si"))
            {
                //SEND EMAIL
                await sendEmail(ClaimModel);
                await stepContext.Context.SendActivityAsync($"Tu solicitud de reclamo ha sido enviada al departamento encargado." +
                    $"{Environment.NewLine} ¿Existe algo más en lo que te pueda ayudar?", cancellationToken: cancellationToken);

            }
            else
            {
                await stepContext.Context.SendActivityAsync("Ok, cuando necesites realizar seguimiento a un reclamo generado puedes contar conmigo", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }
        #endregion conversationClaim

        private async Task sendEmail(ClaimAllModels ClaimModel)
        {
            string contentEmail = "";
            if (ClaimModel.hasOtherPhone)
            {
                contentEmail = $"Un usuario ha solicitado la creación de un reclamo, a continuación la información recopilada en la conversación {Environment.NewLine}" +
                  $"{Environment.NewLine} {Environment.NewLine} 🖊 Nombre del reclamo: {ClaimModel.claimName} " +
                  $"{Environment.NewLine} 📌 Código del reclamo: {ClaimModel.code} " +
                  $"{Environment.NewLine} 📝 Nombre y Apellido: {ClaimModel.fullName} " +
                  $"{Environment.NewLine} 🎫 Documento de identidad: {ClaimModel.typeDoc} {ClaimModel.ci} {Environment.NewLine}" +
                  $"☎ Teléfono: {ClaimModel.phone}" +
                  $"{Environment.NewLine} ☎ Teléfono alternativo: {ClaimModel.phoneAlt}" +
                  $"{Environment.NewLine} 📧 Correo: {ClaimModel.email}" +
                  $"{Environment.NewLine} 📄 Descripción: {ClaimModel.Description}";
            }
            else
            {
                contentEmail = $"Un usuario ha solicitado la creación de un reclamo, a continuación la información recopilada en la conversación {Environment.NewLine}" +
                  $"{Environment.NewLine} {Environment.NewLine} 🖊 Nombre del reclamo: {ClaimModel.claimName} " +
                  $"{Environment.NewLine} 📌 Código del reclamo: {ClaimModel.code} " +
                  $"{Environment.NewLine} 📝 Nombre y Apellido: {ClaimModel.fullName} " +
                  $"{Environment.NewLine} 🎫 Documento de identidad: {ClaimModel.typeDoc} {ClaimModel.ci} {Environment.NewLine}" +
                  $"☎ Teléfono: {ClaimModel.phone}" +
                  $"{Environment.NewLine} 📧 Correo: {ClaimModel.email}" +
                  $"{Environment.NewLine} 📄 Descripción: {ClaimModel.Description}";
            }
            string from = "Aria@consein.com";
            string fromName = "Aria";
            string to = "Aria@bancaribe.com.ve";
            string toName = "Aria";
            string tittle = $"Solicitud de creación de un reclamo - codigo {ClientData.code}";
            await _sendGridEmailService.Execute(from, fromName, to, toName, tittle, contentEmail, "");

        }

        #region Validators    

        private Task<bool> SetSecondPhoneValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            if (utilitario.ValidateNumberPhone(promptContext.Context.Activity.Text))
            {
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }


        private Task<bool> EmailValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var email = utilitario.ExtractEmails(promptContext.Context.Activity.Text);
            if (!String.IsNullOrEmpty(email))
            {
                return Task.FromResult(true);
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

        private Task<bool> OtherPhoneValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
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
        private Activity CreateButtonsClaims(string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                reply = MessageFactory.Text("¿Desea crear un reclamo relacionado con operaciones en puntos de venta?");
            }
            else
            {
                reply = MessageFactory.Text("Por favor indique una respuesta, ¿Desea crear un reclamo relacionado con operaciones en puntos de venta?");
            }

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

        private Activity CreateButtonsPhone(string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                //reply = MessageFactory.Text("Por favor indique una respuesta");
            }
            else
            {
                reply = MessageFactory.Text("Por favor indique una respuesta, ¿Posee otro número de contacto adicional al registrado anteriormente" +
                    "?");
            }

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


        private Activity CreateButtonsConfirm(string retry)
        {
            var reply = MessageFactory.Text("");
            
            if (ClaimModel.hasOtherPhone)
            {
                reply = MessageFactory.Text($"¿Confirmas que los datos suministrados son correctos?" +
                    $"{Environment.NewLine} 📝 Nombre y Apellido: {ClaimModel.fullName} " +
                    $"{Environment.NewLine} 🎫 Documento de identidad: {ClaimModel.typeDoc} {ClaimModel.ci} " +
                    $"{Environment.NewLine} ☎ Teléfono: {ClaimModel.phone}" +
                    $"{Environment.NewLine} ☎ Teléfono alternativo: {ClaimModel.phoneAlt}" +
                    $"{Environment.NewLine} 📧 Correo: {ClaimModel.email}" +
                    $"{Environment.NewLine} 📄 Descripción: {ClaimModel.Description}");
            }
            else
            {
                reply = MessageFactory.Text($"¿Confirmas que los datos suministrados son correctos?" +
                    $"{Environment.NewLine} 📝 Nombre y Apellido: {ClaimModel.fullName} " +
                    $"{Environment.NewLine} 🎫 Documento de identidad: {ClaimModel.typeDoc} {ClaimModel.ci} " +
                    $"{Environment.NewLine} ☎ Teléfono: {ClaimModel.phone}" +
                    $"{Environment.NewLine} 📧 Correo: {ClaimModel.email}" +
                    $"{Environment.NewLine} 📄 Descripción: {ClaimModel.Description}");
            }
            
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
