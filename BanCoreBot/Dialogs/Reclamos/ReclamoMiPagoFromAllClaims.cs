using BanCoreBot.Infrastructure.SendGrid;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BanCoreBot.Common.Utils;
using BanCoreBot.Dialogs.Cancel;
using BanCoreBot.Dialogs.Reclamos.Review;
using BanCoreBot.Common.Models.User;

namespace BanCoreBot.Dialogs.Reclamos
{
    public class ReclamoMiPagoFromAllClaims : CancelDialog
    {
        private BotState _userState;
        private readonly ISendGridEmailService _sendGridEmailService;
        private const string _SetTipoPersona = "SetTipoPersona";
        private const string _SetTipoReclamo = "SetTipoReclamo";
        private const string _SetSecondAlt = "SetSecondAlt";
        private const string _SetSecondPhone = "SetSecondPhone";
        private const string _SetEmail = "SetEmail";
        private const string _SetFecha = "SetFecha";
        private const string _SetMonto = "SetMonto";
        private const string _Set4DigCta = "Set4DigCta";
        private const string _Set4DigTjta = "Set4DigTjta";
        private const string _SetNumEnvia = "SetNumEnvia";
        private const string _SetNumRecibe = "SetNumRecibe";
        private const string _SetOtroBanco = "SetOtroBanco";
        private const string _SetDescripcion = "SetDescripcion";
        private const string _Confirmacion = "Confirmacion";
        private const string _FinalProcess = "FinalProcess";
        private Utils utilitario = new Utils();
        private UserPersonalData ClientData;

        public ReclamoMiPagoFromAllClaims(ISendGridEmailService sendGridEmailService, PrivateConversationState userState)
            : base(nameof(ReclamoMiPagoFromAllClaims))
        {
            _userState = userState;
            _sendGridEmailService = sendGridEmailService;

            var waterfallStep = new WaterfallStep[]
            {
                SetTipoPersona,
                SetTipoReclamo,
                SetSecondAlt,
                SetSecondPhone,
                SetEmail,
                SetFecha,
                SetMonto,
                Set4DigCta,
                Set4DigTjta,
                SetNumEnvia,
                SetNumRecibe,
                SetOtroBanco,
                SetDescripcion,
                Confirmacion,
                FinalProcess
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallStep));
            AddDialog(new TextPrompt(_SetTipoPersona, SetTipoPersonaValidator));
            AddDialog(new TextPrompt(_SetTipoReclamo, SetTipoReclamoValidator));
            AddDialog(new TextPrompt(_SetSecondAlt, OtherPhoneValidator));
            AddDialog(new TextPrompt(_SetSecondPhone, SetSecondPhoneValidator));
            AddDialog(new TextPrompt(_SetEmail, EmailValidator));
            AddDialog(new TextPrompt(_SetFecha, SetFechaValidator));
            AddDialog(new TextPrompt(_SetMonto, SetMontoValidator));
            AddDialog(new TextPrompt(_Set4DigCta, Set4DigCtaValidator));
            AddDialog(new TextPrompt(_Set4DigTjta, Set4DigTjtaValidator));
            AddDialog(new TextPrompt(_SetNumEnvia, SetSecondPhoneValidator));
            AddDialog(new TextPrompt(_SetNumRecibe, SetSecondPhoneValidator));
            AddDialog(new TextPrompt(_SetOtroBanco));
            AddDialog(new TextPrompt(_SetDescripcion));
            AddDialog(new TextPrompt(_Confirmacion, ConfirmValidator));
            AddDialog(new TextPrompt(_FinalProcess));
        }


        #region conversationClaim

       
        private async Task<DialogTurnResult> SetTipoPersona(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var option = await stepContext.PromptAsync(
                _SetTipoPersona,
                new PromptOptions
                {
                    Prompt = CreateButtonsTypePerson(""),
                    RetryPrompt = CreateButtonsTypePerson("Reintento")
                }, cancellationToken
                );
            return option;
        }

        private async Task<DialogTurnResult> SetTipoReclamo(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            ClientData.typeClaim = stepContext.Context.Activity.Text;
            var option = await stepContext.PromptAsync(
                _SetTipoReclamo,
                new PromptOptions
                {
                    Prompt = CreateButtonsTypeClaim(""),
                    RetryPrompt = CreateButtonsTypeClaim("Reintento")
                }, cancellationToken
            );
            return option;
        }

        private async Task<DialogTurnResult> SetSecondAlt(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            ClientData.SubTypeClaim = stepContext.Context.Activity.Text;

            if (string.IsNullOrEmpty(ClientData.phoneAlt))
            {
                await stepContext.Context.SendActivityAsync($"Estimado(a) {ClientData.name}, posee algún otro número de contacto adicional al {ClientData.phone}");
                return await stepContext.PromptAsync(
                    _SetSecondAlt,
                    new PromptOptions
                    {
                        Prompt = CreateButtonsPhone(""),
                        RetryPrompt = CreateButtonsPhone("Retry")
                    }, cancellationToken
                );
            }
            else
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }

        }

        private async Task<DialogTurnResult> SetSecondPhone(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            if (string.IsNullOrEmpty(ClientData.phoneAlt))
            {
                if (stepContext.Context.Activity.Text.ToString().ToLower().Equals("si"))
                {
                    ClientData.hasOtherPhone = true;
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
                    ClientData.hasOtherPhone = false;
                    return await stepContext.NextAsync(cancellationToken: cancellationToken);
                }
            }
            else
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> SetEmail(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            if (ClientData.typeClaim.ToLower().Equals("mi pago bancaribe personas"))
            {
                if (ClientData.SubTypeClaim.ToLower().Equals("duplicado"))
                {
                    ClientData.claimName = "Formulario Mi Pago Bancaribe Personas Duplicado";
                    ClientData.code = "6.08";
                }
                else
                {
                    ClientData.claimName = "Formulario Mi Pago Bancaribe Personas No Enviado";
                    ClientData.code = "6.08";
                }
            }
            else
            {
                if (ClientData.SubTypeClaim.ToLower().Equals("duplicado"))
                {
                    ClientData.claimName = "Formulario Mi Pago Bancaribe Comercios Duplicado";
                    ClientData.code = "6.08";
                }
                else
                {
                    ClientData.claimName = "Formulario Mi Pago Bancaribe Comercios No Enviado";
                    ClientData.code = "6.08";
                }
            }

            if (string.IsNullOrEmpty(ClientData.phoneAlt))
            {
                if (ClientData.hasOtherPhone)
                {
                    ClientData.phoneAlt = stepContext.Context.Activity.Text;
                }
            }


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

        private async Task<DialogTurnResult> SetFecha(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());

            if (string.IsNullOrEmpty(ClientData.email))
            {
                ClientData.email = utilitario.ExtractEmails(stepContext.Context.Activity.Text);
                if (String.IsNullOrEmpty(ClientData.email)) { ClientData.email = "No Posee"; }
            }

            return await stepContext.PromptAsync(
                 _SetFecha,
                 new PromptOptions
                 {
                     Prompt = MessageFactory.Text("Por favor ingrese la fecha de la incidencia en el formato \"dd/mm/aaaa \":"),
                     RetryPrompt = MessageFactory.Text("Ingrese una fecha válida, Por favor ingrese la fecha de la incidencia en el formato \"dd/mm/aaaa \":")
                 },
                 cancellationToken
                 );
        }

        private async Task<DialogTurnResult> SetMonto(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            ClientData.date = stepContext.Context.Activity.Text;
            return await stepContext.PromptAsync(
                _SetMonto,
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Por favor ingrese el monto del reclamo:"),
                    RetryPrompt = MessageFactory.Text("Por favor ingresa el monto del reclamo sin letras ni caracteres especiales:")
                },
                cancellationToken
                );
        }

        private async Task<DialogTurnResult> Set4DigCta(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            ClientData.amount = stepContext.Context.Activity.Text;
            return await stepContext.PromptAsync(
                _Set4DigCta,
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Por favor ingrese los últimos cuatro (4) dígitos de la cuenta:"),
                    RetryPrompt = MessageFactory.Text("El dato ingresado no es válido, por favor verifique e ingrese solo los cuatro (4) últimos números de tu cuenta:")
                },
                cancellationToken
                );
        }

        private async Task<DialogTurnResult> Set4DigTjta(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            ClientData.ult4DigCta = stepContext.Context.Activity.Text;
            return await stepContext.PromptAsync(
                _Set4DigTjta,
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Por favor ingrese los últimos cuatro (4) dígitos de la tarjeta de débito:"),
                    RetryPrompt = MessageFactory.Text("El dato ingresado no es válido, por favor ingrese solo los cuatro (4) últimos números de la tarjeta de débito:")
                },
                cancellationToken
                );
        }

        private async Task<DialogTurnResult> SetNumEnvia(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            ClientData.ult4DigTjta = stepContext.Context.Activity.Text;
            return await stepContext.PromptAsync(
                _SetNumEnvia,
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Por favor ingrese número celular del cliente que envía los fondos:"),
                    RetryPrompt = MessageFactory.Text("El dato ingresado no es válido, por favor ingrese número celular del cliente que envía los fondos:")
                },
                cancellationToken
                );
        }

        private async Task<DialogTurnResult> SetNumRecibe(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            ClientData.numCelEnvia = stepContext.Context.Activity.Text;
            return await stepContext.PromptAsync(
                _SetNumRecibe,
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Por favor ingrese número celular destinatario de los fondos:"),
                    RetryPrompt = MessageFactory.Text("El dato ingresado no es válido, por favor ingrese número celular de quien recibe los fondos:")
                },
                cancellationToken
                );
        }

        private async Task<DialogTurnResult> SetOtroBanco(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            ClientData.numCelRecibe = stepContext.Context.Activity.Text;
            return await stepContext.PromptAsync(
                _SetOtroBanco,
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Por favor ingrese el nombre del otro banco afiliado:"),
                },
                cancellationToken
                );
        }

        private async Task<DialogTurnResult> SetDescripcion(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            ClientData.nameOtherBank = stepContext.Context.Activity.Text;
            return await stepContext.PromptAsync(
                _SetDescripcion,
                new PromptOptions { Prompt = MessageFactory.Text("Por favor ingresa una breve descripción de lo que aconteció:") },
                cancellationToken
                );
        }

        private async Task<DialogTurnResult> Confirmacion(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            ClientData.Description = stepContext.Context.Activity.Text;
            return await stepContext.BeginDialogAsync(nameof(ReviewConfirmDialog), cancellationToken: cancellationToken);

        }
        private async Task<DialogTurnResult> FinalProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await sendEmail(stepContext);
            await stepContext.Context.SendActivityAsync($"Tu solicitud de reclamo ha sido enviada al departamento encargado." +
                $"{Environment.NewLine} ¿Existe algo más en lo que te pueda ayudar?", cancellationToken: cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
        #endregion conversationClaim


        private async Task sendEmail(WaterfallStepContext stepContext)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            string contentEmail = "";
            if (ClientData.hasOtherPhone)
            {
                contentEmail = $"Un usuario ha solicitado la creación de un reclamo, a continuación la información recopilada en la conversación {Environment.NewLine}" +
                  $"{Environment.NewLine} {Environment.NewLine} 🖊 Nombre del reclamo: {ClientData.claimName} " +
                  $"{Environment.NewLine} 📌 Código del reclamo: {ClientData.code} " +
                  $"{Environment.NewLine} 📝 Nombre y Apellido: {ClientData.name} " +
                  $"{Environment.NewLine} 🎫 Documento de identidad: {ClientData.typeDoc} {ClientData.ci} {Environment.NewLine}" +
                  $"☎ Teléfono: {ClientData.phone}" +
                  $"{Environment.NewLine} ☎ Teléfono alternativo: {ClientData.phoneAlt}" +
                  $"{Environment.NewLine} 📧 Correo: {ClientData.email}" +
                  $"{Environment.NewLine} 📆 Fecha: {ClientData.date}" +
                  $"{Environment.NewLine} 💰 Monto del reclamo: {ClientData.amount}" +
                  $"{Environment.NewLine} 🔢 Últimos 4 dígitos de la cuenta: {ClientData.ult4DigCta}" +
                  $"{Environment.NewLine} 🔢 Últimos 4 dígitos de la tarjeta: {ClientData.ult4DigTjta}" +
                  $"{Environment.NewLine} ☎ Teléfono de quien envía los fondos: {ClientData.numCelEnvia}" +
                  $"{Environment.NewLine} ☎ Teléfono de quien recibe los fondos: {ClientData.numCelRecibe}" +
                  $"{Environment.NewLine} 🏦 Nombre del banco afiliado: {ClientData.nameOtherBank}" +
                  $"{Environment.NewLine} 📄 Descripción: {ClientData.Description}";
            }
            else
            {
                contentEmail = $"Un usuario ha solicitado la creación de un reclamo, a continuación la información recopilada en la conversación {Environment.NewLine}" +
                  $"{Environment.NewLine} {Environment.NewLine} 🖊 Nombre del reclamo: {ClientData.claimName} " +
                  $"{Environment.NewLine} 📌 Código del reclamo: {ClientData.code} " +
                  $"{Environment.NewLine} 📝 Nombre y Apellido: {ClientData.name} " +
                  $"{Environment.NewLine} 🎫 Documento de identidad: {ClientData.typeDoc} {ClientData.ci} {Environment.NewLine}" +
                  $"☎ Teléfono: {ClientData.phone}" +
                  $"{Environment.NewLine} 📧 Correo: {ClientData.email}" +
                  $"{Environment.NewLine} 📆 Fecha: {ClientData.date}" +
                  $"{Environment.NewLine} 💰 Monto del reclamo: {ClientData.amount}" +
                  $"{Environment.NewLine} 🔢 Últimos 4 dígitos de la cuenta: {ClientData.ult4DigCta}" +
                  $"{Environment.NewLine} 🔢 Últimos 4 dígitos de la tarjeta: {ClientData.ult4DigTjta}" +
                  $"{Environment.NewLine} ☎ Teléfono de quien envía los fondos: {ClientData.numCelEnvia}" +
                  $"{Environment.NewLine} ☎ Teléfono de quien recibe los fondos: {ClientData.numCelRecibe}" +
                  $"{Environment.NewLine} 🏦 Nombre del banco afiliado: {ClientData.nameOtherBank}" +
                  $"{Environment.NewLine} 📄 Descripción: {ClientData.Description}";
            }

            string from = "Aria@consein.com";
            string fromName = "Aria";
            string to = "Aria@bancaribe.com.ve";
            string toName = "Aria";
            string tittle = $"Solicitud de creación de un reclamo - codigo {ClientData.code}";
            await _sendGridEmailService.Execute(from, fromName, to, toName, tittle, contentEmail, "");


        }

        #region Validators     
        private Task<bool> CrearReclamoValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
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


        private async Task<bool> SetTipoPersonaValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Context.Activity.Text.ToLower().Equals("mi pago bancaribe personas") ||
                promptContext.Context.Activity.Text.ToLower().Equals("mi pago bancaribe comercios"))
            {
                return await Task.FromResult(true);
            }
            else
            {
                return await Task.FromResult(false);
            }
        }

        private async Task<bool> SetTipoReclamoValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Context.Activity.Text.ToLower().Equals("no enviado") ||
                promptContext.Context.Activity.Text.ToLower().Equals("duplicado"))
            {
                return await Task.FromResult(true);
            }
            else
            {
                return await Task.FromResult(false);
            }
        }

        private async Task<bool> SetSecondPhoneValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            if (utilitario.ValidateNumberPhone(promptContext.Context.Activity.Text))
            {
                return await Task.FromResult(true);
            }
            else
            {
                return await Task.FromResult(false);
            }
        }

        private Task<bool> SetMontoValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(utilitario.ValidateNumber(promptContext.Context.Activity.Text));
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



        private Task<bool> SetFechaValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var validate = utilitario.ValidateDate72a120(promptContext.Context.Activity.Text);
            if (validate == 0)
            {
                return Task.FromResult(true);
            }
            else if (validate == 1)
            {
                promptContext.Context.SendActivityAsync($"Deben de pasar al menos 3 días a partir de la fecha de operación para realizar el reclamo {Environment.NewLine}" +
                    $"La fecha del reclamo no puede superar los 120 días de acuerdo al tiempo máximo establecido en la ley.");
                return Task.FromResult(false);
            }
            else
            {
                return Task.FromResult(false);
            }
        }


        private Task<bool> Set4DigTjtaValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(utilitario.ValidateNumber4Dig(promptContext.Context.Activity.Text));
        }

        private Task<bool> Set4DigCtaValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(utilitario.ValidateNumber4Dig(promptContext.Context.Activity.Text));
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
                reply = MessageFactory.Text("¿Desea crear un reclamo referente al servicio Mi Pago Bancaribe?");
            }
            else
            {
                reply = MessageFactory.Text("Por favor indique una respuesta, ¿Desea crear un reclamo referente al servicio Mi Pago Bancaribe?");
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

        private Activity CreateButtonsTypePerson(string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                reply = MessageFactory.Text("Seleccione el tipo de reclamo:");
            }
            else
            {
                reply = MessageFactory.Text("Por favor indique una respuesta seleccionando la opción que corresponda a tu caso:");
            }

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() {Title = "Mi Pago Bancaribe Personas", Value = "Mi Pago Bancaribe Personas" , Type = ActionTypes.ImBack},
                    new CardAction() {Title = "Mi Pago Bancaribe Comercios", Value = "Mi Pago Bancaribe Comercios" , Type = ActionTypes.ImBack}

                }
            };
            return reply as Activity;
        }

        private Activity CreateButtonsTypeClaim(string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                reply = MessageFactory.Text("Seleccione alguna opción según sea tu caso:");
            }
            else
            {
                reply = MessageFactory.Text("Por favor indique una respuesta seleccionando la opción que corresponda a tu caso:");
            }

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() {Title = "No enviado", Value = "No enviado" , Type = ActionTypes.ImBack},
                    new CardAction() {Title = "Duplicado", Value = "Duplicado" , Type = ActionTypes.ImBack}

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

        #endregion Buttons


    }
}
