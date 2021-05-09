using BanCoreBot.Infrastructure.SendGrid;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Threading;
using System.Threading.Tasks;
using BanCoreBot.Common.Utils;
using BanCoreBot.Dialogs.Cancel;
using BanCoreBot.Dialogs.Reclamos.Review;
using BanCoreBot.Dialogs.DatosIniciales;
using BanCoreBot.Common.Models.User;
using BanCoreBot.Infrastructure.Luis;
using Luis;

namespace BanCoreBot.Dialogs.Reclamos
{
    public class ReclamoNoAbonadoDialog : CancelDialog
    {
        private ILuisService _luisService;
        private BotState _userState;
        private readonly ISendGridEmailService _sendGridEmailService;
        private const string _SetDatosIniciales = "SetDatosIniciales";
        private const string _SetEmail = "SetEmail";
        private const string _SetDescripcion = "SetDescripcion";
        private const string _Confirmacion = "Confirmacion";
        private Utils utilitario = new Utils();
        private UserPersonalData ClientData;

        public ReclamoNoAbonadoDialog(ISendGridEmailService sendGridEmailService, PrivateConversationState userState, ILuisService luisService)
            : base(nameof(ReclamoNoAbonadoDialog))
        {
            _luisService = luisService;
            _userState = userState;
            _sendGridEmailService = sendGridEmailService;

            var waterfallStep = new WaterfallStep[]
            {
                SetDatosIniciales,
                SetEmail,
                SetDescripcion,
                Confirmacion,
                FinalProcess
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallStep));
            AddDialog(new TextPrompt(_SetEmail, EmailValidator));
            AddDialog(new TextPrompt(_SetDescripcion));
            AddDialog(new TextPrompt(_Confirmacion, ConfirmValidator));
        }

        #region conversationClaim

        private async Task<DialogTurnResult> SetDatosIniciales(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(DatosInic), cancellationToken: cancellationToken);
        }


        private async Task<DialogTurnResult> SetEmail(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            if (string.IsNullOrEmpty(ClientData.email))
            {
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
            else
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> SetDescripcion(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());

            if (string.IsNullOrEmpty(ClientData.email))
            {
                ClientData.email = utilitario.ExtractEmails(stepContext.Context.Activity.Text);
                if (String.IsNullOrEmpty(ClientData.email)) { ClientData.email = "No Posee"; }
            }
            return await stepContext.PromptAsync(
                _SetDescripcion,
                new PromptOptions { Prompt = MessageFactory.Text("Escríbeme tu requerimiento") },
                cancellationToken
                );
        }

        private async Task<DialogTurnResult> Confirmacion(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            ClientData.code = "natural";
            ClientData.Description = stepContext.Context.Activity.Text;
            return await stepContext.BeginDialogAsync(nameof(ReviewConfirmDialog), cancellationToken: cancellationToken);
        }


        private async Task<DialogTurnResult> FinalProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Context.Activity.Text.ToLower().Equals("si"))
            {
               //SEND EMAIL
                await sendEmail(stepContext);
                await stepContext.Context.SendActivityAsync($"Envié la información que me suministraste al departamento encargado próximamente un especialista se comunicará contigo en el horario comprendido entre las 8:00 am  y las 2:00 pm de lunes a viernes." +
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

        private async Task sendEmail(WaterfallStepContext stepContext)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            string contentEmail = $"Un usuario ha solicitado información referente a un reclamo que generó, a continuación la información recopilada en la conversación {Environment.NewLine}" +
            $"{Environment.NewLine} 📝 Nombre y Apellido: {ClientData.name} " +
            $"{Environment.NewLine} 🎫 Documento de identidad: {ClientData.typeDoc} {ClientData.ci} " +
            $"{Environment.NewLine} ☎ Teléfono: {ClientData.phone}" +
            $"{Environment.NewLine} 📧 Correo: {ClientData.email}" +
            $"{Environment.NewLine} 📄 Canal: {stepContext.Context.Activity.ChannelId} " +
            $"{Environment.NewLine} 📄 Descripción: {ClientData.Description}";

            if (stepContext.Context.Activity.ChannelId.Equals("twitter"))
            {
                contentEmail = contentEmail + $"{Environment.NewLine} 🗣 Usuario: @{stepContext.Context.Activity.From.Name} ";
            }

            string from = "Aria@consein.com";
            string fromName = "Aria";
            //string to = "Aria@bancaribe.com.ve";
            string to = "AriaPrueba@bancaribe.com.ve";
            string toName = "Aria";
            string tittle = $"Solicitud de seguimiento de un reclamo";
            await _sendGridEmailService.Execute(from, fromName, to, toName, tittle, contentEmail, "");

        }

        #region Validators
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

        #endregion Validators




    }
}
