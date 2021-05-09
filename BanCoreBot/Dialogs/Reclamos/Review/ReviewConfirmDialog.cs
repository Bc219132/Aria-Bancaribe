using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;
using BanCoreBot.Dialogs.Cancel;
using BanCoreBot.Common.Models.User;

namespace BanCoreBot.Dialogs.Reclamos.Review
{
    public class ReviewConfirmDialog : CancelDialog
    {
        private BotState _userState;
        private const string _Confirm = "Confirm";
        private const string _ProcessConfirm = "ProcessConfirm";
        private const string _Review = "Review";
        private const string _StartOverAgain = "StartOverAgain";
        private UserPersonalData ClientData;

        public ReviewConfirmDialog(PrivateConversationState userState)
            : base(nameof(ReviewConfirmDialog))
        {
            _userState = userState;

            var waterfallStep = new WaterfallStep[]
            {
                Confirm,
                ProcessConfirm,
                Review,
                StartOverAgain
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallStep));
            AddDialog(new TextPrompt(_Confirm, ConfirmValidator));
            AddDialog(new TextPrompt(_ProcessConfirm, ProcessConfirmValidator));
            AddDialog(new TextPrompt(_Review));
            AddDialog(new TextPrompt(_StartOverAgain));
        }

        #region conversationConfirmReview
        private async Task<DialogTurnResult> Confirm(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(
                _Confirm,
                new PromptOptions
                {
                    Prompt = await CreateButtonsConfirm(stepContext),
                    RetryPrompt = await CreateButtonsConfirm(stepContext)
                }, cancellationToken
            );
        }

        private async Task<DialogTurnResult> ProcessConfirm(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Context.Activity.Text.ToLower().Equals("si"))
            {
                return await stepContext.EndDialogAsync(stepContext.Values, cancellationToken);
            }
            else
            {
                return await stepContext.PromptAsync(
                    _ProcessConfirm,
                    new PromptOptions
                    {
                        Prompt = await CreateButtonsReview(stepContext, ""),
                        RetryPrompt = await CreateButtonsReview(stepContext, "Reintento")
                    }, cancellationToken
                    );
            }
        }

        private async Task<DialogTurnResult> Review(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
             if (stepContext.Context.Activity.Text.ToLower().Equals("nombre"))
            {
                return await stepContext.BeginDialogAsync(nameof(ReviewNameDialog), null, cancellationToken);
            }
            else if (stepContext.Context.Activity.Text.ToLower().Equals("nombre de la empresa"))
            {
                return await stepContext.BeginDialogAsync(nameof(ReviewCompanyNameDialog), null, cancellationToken);
            }
            else if (stepContext.Context.Activity.Text.ToLower().Equals("rif"))
            {
                return await stepContext.BeginDialogAsync(nameof(ReviewRIFDialog), null, cancellationToken);
            }
            else if (stepContext.Context.Activity.Text.ToLower().Equals("documento de identidad"))
            {
                return await stepContext.BeginDialogAsync(nameof(ReviewCIOrPassDialog), null, cancellationToken);
            }
            else if (stepContext.Context.Activity.Text.ToLower().Equals("teléfono"))
            {
                return await stepContext.BeginDialogAsync(nameof(ReviewPhoneDialog), null, cancellationToken);
            }
            else if (stepContext.Context.Activity.Text.ToLower().Equals("correo"))
            {
                return await stepContext.BeginDialogAsync(nameof(ReviewMailDialog), null, cancellationToken);
            }
            else if (stepContext.Context.Activity.Text.ToLower().Equals("fecha"))
            {
                return await stepContext.BeginDialogAsync(nameof(ReviewDateDialog), null, cancellationToken);
            }
            else if (stepContext.Context.Activity.Text.ToLower().Equals("monto"))
            {
                return await stepContext.BeginDialogAsync(nameof(ReviewAmountDialog), null, cancellationToken);
            }
            else if (stepContext.Context.Activity.Text.ToLower().Equals("ultimos 4 dígitos de la cuenta"))
            {
                return await stepContext.BeginDialogAsync(nameof(Review4UltCtaDialog), null, cancellationToken);
            }
            else if (stepContext.Context.Activity.Text.ToLower().Equals("ultimos 4 dígitos de la tarjeta"))
            {
                return await stepContext.BeginDialogAsync(nameof(Review4UltTjtaDialog), null, cancellationToken);
            }
            else if (stepContext.Context.Activity.Text.ToLower().Equals("teléfono alternativo"))
            {
                return await stepContext.BeginDialogAsync(nameof(ReviewTlfAltDialog), null, cancellationToken);
            }
            else if (stepContext.Context.Activity.Text.ToLower().Equals("nombre del destinatario"))
            {
                return await stepContext.BeginDialogAsync(nameof(ReviewNameDestDialog), null, cancellationToken);
            }
            else if (stepContext.Context.Activity.Text.ToLower().Equals("nombre del otro banco") || 
                stepContext.Context.Activity.Text.ToLower().Equals("nombre del banco")) 
            {
                return await stepContext.BeginDialogAsync(nameof(ReviewNameOtherBankDialog), null, cancellationToken);
            }
            else if (stepContext.Context.Activity.Text.ToLower().Equals("celular que envía")) 
            {
                return await stepContext.BeginDialogAsync(nameof(ReviewSenderPhoneDialog), null, cancellationToken);
            }
            else if (stepContext.Context.Activity.Text.ToLower().Equals("celular que recibe"))
            {
                return await stepContext.BeginDialogAsync(nameof(ReviewReceiverPhoneDialog), null, cancellationToken);
            }
            else if (stepContext.Context.Activity.Text.ToLower().Equals("celular a recargar"))
            {
                return await stepContext.BeginDialogAsync(nameof(ReviewPhoneRefillDialog), null, cancellationToken);
            }
            else if (stepContext.Context.Activity.Text.ToLower().Equals("número de cuenta del beneficiario"))
            {
                return await stepContext.BeginDialogAsync(nameof(ReviewNumCtaBenefDialog), null, cancellationToken);
            }
            else if (stepContext.Context.Activity.Text.ToLower().Equals("descripción")) 
            {
                return await stepContext.BeginDialogAsync(nameof(ReviewDescriptionDialog), null, cancellationToken);
            }
            else 
            {
                return await stepContext.EndDialogAsync(stepContext, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> StartOverAgain(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.ReplaceDialogAsync(nameof(ReviewConfirmDialog), stepContext.Values,  cancellationToken);
        }
        #endregion conversationConfirmReview



        #region Buttons
        private async  Task<Activity> CreateButtonsConfirm(WaterfallStepContext stepContext)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            var text = "";
            if(ClientData.code.Equals("natural"))
            {
                text = $"¿Confirmas que los datos suministrados son correctos?" +
                $"{Environment.NewLine} 📝 Nombre y Apellido: {ClientData.name} " +
                $"{Environment.NewLine} 🎫 Documento de identidad: {ClientData.typeDoc} {ClientData.ci} " +
                $"{Environment.NewLine} ☎ Teléfono: {ClientData.phone}" +
                $"{Environment.NewLine} 📧 Correo: {ClientData.email}";
            }
            else if (ClientData.code.Equals("juridico"))
            {
                text = $"¿Confirmas que los datos suministrados son correctos?" +
                $"{Environment.NewLine} 📝 Nombre y Apellido: {ClientData.name} " +
                $"{Environment.NewLine} 📝 Nombre de la Empresa: {ClientData.companyname} " +
                $"{Environment.NewLine} 🎫 RIF: {ClientData.rif} " +
                $"{Environment.NewLine} ☎ Teléfono: {ClientData.phone}" +
                $"{Environment.NewLine} 📧 Correo: {ClientData.email}" ;
            }
            else
            {
                text = $"¿Confirmas que los datos suministrados son correctos?" +
                $"{Environment.NewLine} 📝 Nombre y Apellido: {ClientData.name} " +
                $"{Environment.NewLine} 🎫 Documento de identidad: {ClientData.typeDoc} {ClientData.ci} " +
                $"{Environment.NewLine} ☎ Teléfono: {ClientData.phone}" +
                $"{Environment.NewLine} 📧 Correo: {ClientData.email}" +
                $"{Environment.NewLine} 📆 Fecha: {ClientData.date}" +
                $"{Environment.NewLine} 💰 Monto del reclamo: {ClientData.amount}" +
                $"{Environment.NewLine} 🔢 Últimos 4 dígitos de la cuenta: {ClientData.ult4DigCta}";
            }
            

            if (ClientData.code.Equals("2.18") || ClientData.code.Equals("2.20") || ClientData.code.Equals("2.21"))
            {
                text = $"{text} {Environment.NewLine}  🔢 Últimos 4 dígitos de la tarjeta de crédito: { ClientData.ult4DigTjta}";
            }
            else if (!ClientData.code.Equals("juridico") && !ClientData.code.Equals("natural"))
            {
                text = $"{text} {Environment.NewLine} 🔢 Últimos 4 dígitos de la tarjeta de débito: {ClientData.ult4DigTjta}";
            }

            if (ClientData.hasOtherPhone)
            {
                text = $"{text} {Environment.NewLine} ☎ Teléfono alternativo: { ClientData.phoneAlt}";
            }


            if (ClientData.code.Equals("6.02") || ClientData.code.Equals("6.05") || ClientData.code.Equals("2.20") || ClientData.code.Equals("2.21"))
            {
                text = $"{text} {Environment.NewLine} 📝 Nombre y Apellido del beneficiario: {ClientData.fullNameBenef}";

                if (!ClientData.code.Equals("2.21"))
                {
                    text = $"{text} {Environment.NewLine} 🏦 Nombre del banco afiliado: {ClientData.nameOtherBank}";
                    if (!ClientData.code.Equals("2.20"))
                    {
                        text = $"{text} {Environment.NewLine} 🔢 Número de cuenta del beneficiario: {ClientData.numCtaBenef}";
                    }
                }
            }

            if (ClientData.code.Equals("1.03"))
            {
                text = $"{text} {Environment.NewLine}  🏦 Nombre del banco: {ClientData.nameOtherBank}";
            }

            if (ClientData.code.Equals("6.08"))
            {
                text = $"{text} {Environment.NewLine} ☎ Teléfono de quien envía los fondos: {ClientData.numCelEnvia}" +
                $"{Environment.NewLine} ☎ Teléfono de quien recibe los fondos: {ClientData.numCelRecibe}" +
                $"{Environment.NewLine} 🏦 Nombre del banco afiliado: {ClientData.nameOtherBank}";
            }


            if (ClientData.code.Equals("4.02") || ClientData.code.Equals("4.11") || ClientData.code.Equals("4.04"))
            {
                text = $"{text} {Environment.NewLine} 📱 Celular a recargar: {ClientData.phoneRefill}";
            }

            text = $"{text} {Environment.NewLine} 📄 Descripción: {ClientData.Description}";


            var reply = MessageFactory.Text(text);
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

        private async  Task<Activity> CreateButtonsReview(WaterfallStepContext stepContext, string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                reply = MessageFactory.Text($"Selecciona el campo que desea corregir");
            }
            else
            {
                reply = MessageFactory.Text($"Por favor indique una respuesta seleccionando alguna de las opciones que se presentan a continuación, ¿Que campo desea corregir?");
            }

            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            if (ClientData.code.Equals("natural"))
            {
                reply.SuggestedActions = new SuggestedActions()
                {

                    Actions = new List<CardAction>()
                    {
                        new CardAction() { Title = "Nombre", Value = "Nombre", Type = ActionTypes.ImBack },
                        new CardAction() { Title = "Documento de Identidad", Value = "Documento de Identidad", Type = ActionTypes.ImBack },
                        new CardAction() { Title = "Teléfono", Value = "Teléfono", Type = ActionTypes.ImBack },
                        new CardAction() { Title = "Correo", Value = "Correo", Type = ActionTypes.ImBack },
                    }
                };
            }
            else if (ClientData.code.Equals("juridico"))
            {
                reply.SuggestedActions = new SuggestedActions()
                {

                    Actions = new List<CardAction>()
                    {
                        new CardAction() { Title = "Nombre", Value = "Nombre", Type = ActionTypes.ImBack },
                        new CardAction() { Title = "Nombre de la Empresa", Value = "Nombre de la Empresa", Type = ActionTypes.ImBack },
                        new CardAction() { Title = "RIF", Value = "RIF", Type = ActionTypes.ImBack },
                        new CardAction() { Title = "Teléfono", Value = "Teléfono", Type = ActionTypes.ImBack },
                        new CardAction() { Title = "Correo", Value = "Correo", Type = ActionTypes.ImBack },
                        
                    }
                };
            }
            else
            {
                reply.SuggestedActions = new SuggestedActions()
                {

                    Actions = new List<CardAction>()
                    {
                        new CardAction() { Title = "Nombre", Value = "Nombre", Type = ActionTypes.ImBack },
                        new CardAction() { Title = "Documento de Identidad", Value = "Documento de Identidad", Type = ActionTypes.ImBack },
                        new CardAction() { Title = "Teléfono", Value = "Teléfono", Type = ActionTypes.ImBack },
                        new CardAction() { Title = "Correo", Value = "Correo", Type = ActionTypes.ImBack },
                        new CardAction() { Title = "Fecha", Value = "Fecha", Type = ActionTypes.ImBack },
                        new CardAction() { Title = "Monto", Value = "Monto", Type = ActionTypes.ImBack },
                        new CardAction() { Title = "Ultimos 4 Dígitos de la Cuenta", Value = "Ultimos 4 Dígitos de la Cuenta", Type = ActionTypes.ImBack },
                        new CardAction() { Title = "Ultimos 4 Dígitos de la Tarjeta", Value = "Ultimos 4 Dígitos de la Tarjeta", Type = ActionTypes.ImBack },

                    }
                };
            }
            

            if (ClientData.hasOtherPhone) 
            {
                reply.SuggestedActions.Actions.Add(new CardAction() { Title = "Teléfono Alternativo", Value = "Teléfono Alternativo", Type = ActionTypes.ImBack }); 
            }
            if (ClientData.code.Equals("6.02") || ClientData.code.Equals("6.05") || ClientData.code.Equals("2.20") || ClientData.code.Equals("2.21"))
            {
                reply.SuggestedActions.Actions.Add(new CardAction() { Title = "Nombre del Destinatario", Value = "Nombre del Destinatario", Type = ActionTypes.ImBack });
                if (!ClientData.code.Equals("2.21"))
                {
                    reply.SuggestedActions.Actions.Add(new CardAction() { Title = "Nombre del Otro Banco", Value = "Nombre del Otro Banco", Type = ActionTypes.ImBack });
                    if (!ClientData.code.Equals("2.20"))
                    {
                        reply.SuggestedActions.Actions.Add(new CardAction() { Title = "Número de Cuenta del Beneficiario", Value = "Número de Cuenta del Beneficiario", Type = ActionTypes.ImBack });
                    }
                }
            }
            if (ClientData.code.Equals("1.03"))
            {
                reply.SuggestedActions.Actions.Add(new CardAction() { Title = "Nombre del Otro Banco", Value = "Nombre del Otro Banco", Type = ActionTypes.ImBack });
            }

            if (ClientData.code.Equals("6.08"))
            {
                reply.SuggestedActions.Actions.Add(new CardAction() { Title = "Celular que Envía", Value = "Celular que Envía", Type = ActionTypes.ImBack });
                reply.SuggestedActions.Actions.Add(new CardAction() { Title = "Celular que Recibe", Value = "Celular que Recibe", Type = ActionTypes.ImBack });
                reply.SuggestedActions.Actions.Add(new CardAction() { Title = "Nombre del Banco", Value = "Nombre del Banco", Type = ActionTypes.ImBack });

            }

            if (ClientData.code.Equals("4.02") || ClientData.code.Equals("4.11") || ClientData.code.Equals("4.04"))
            {
                reply.SuggestedActions.Actions.Add(new CardAction() { Title = "Celular a Recargar", Value = "Celular a Recargar", Type = ActionTypes.ImBack });
            }

            reply.SuggestedActions.Actions.Add(new CardAction() { Title = "Descripción", Value = "Descripción", Type = ActionTypes.ImBack });
            return reply as Activity;
        }
        #endregion Buttons






        #region Validators
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
        private Task<bool> ProcessConfirmValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Context.Activity.Text.ToLower().Trim().Equals("nombre") ||
                promptContext.Context.Activity.Text.ToLower().Trim().Equals("nombre de la empresa") ||
                promptContext.Context.Activity.Text.ToLower().Trim().Equals("rif") || 
                promptContext.Context.Activity.Text.ToLower().Trim().Equals("documento de identidad") ||
                promptContext.Context.Activity.Text.ToLower().Trim().Equals("teléfono") || 
                promptContext.Context.Activity.Text.ToLower().Trim().Equals("correo") ||
                promptContext.Context.Activity.Text.ToLower().Trim().Equals("fecha") ||
                promptContext.Context.Activity.Text.ToLower().Trim().Equals("monto") ||
                promptContext.Context.Activity.Text.ToLower().Trim().Equals("ultimos 4 dígitos de la cuenta") ||
                promptContext.Context.Activity.Text.ToLower().Trim().Equals("ultimos 4 dígitos de la tarjeta") ||
                promptContext.Context.Activity.Text.ToLower().Trim().Equals("teléfono alternativo") ||
                promptContext.Context.Activity.Text.ToLower().Trim().Equals("nombre del destinatario") ||
                promptContext.Context.Activity.Text.ToLower().Trim().Equals("nombre del otro banco") ||
                promptContext.Context.Activity.Text.ToLower().Trim().Equals("número de cuenta del beneficiario") ||
                promptContext.Context.Activity.Text.ToLower().Trim().Equals("celular que envía") ||
                promptContext.Context.Activity.Text.ToLower().Trim().Equals("celular que recibe") ||
                promptContext.Context.Activity.Text.ToLower().Trim().Equals("nombre del banco") ||
                promptContext.Context.Activity.Text.ToLower().Trim().Equals("celular a recargar") ||
                promptContext.Context.Activity.Text.ToLower().Trim().Equals("descripción"))
                
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


