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
using BanCoreBot.Infrastructure.Luis;
using Luis;

namespace BanCoreBot.Dialogs.Reclamos
{
    public class ReclamoEmpleado : CancelDialog
    {
        private ILuisService _luisService;
        private BotState _userState;
        private readonly ISendGridEmailService _sendGridEmailService;
        private const string _CrearReclamo = "CrearReclamo";
        private const string _SetNombre = "SetNombre";
        private const string _SetCIPass = "SetCIPass";
        private const string _SetCI = "SetCI";
        private const string _SetTlf = "SetTlf";
        private const string _SetEmail = "SetEmail";
        private const string _SetDescripcion = "SetDescripcion";
        private const string _Confirmacion = "Confirmacion";
        private Utils utilitario = new Utils();
        private UserPersonalData ClientData;

        public ReclamoEmpleado(ISendGridEmailService sendGridEmailService, PrivateConversationState userState, ILuisService luisService)
            : base(nameof(ReclamoEmpleado))
        {
            _luisService = luisService;
            _userState = userState;
            _sendGridEmailService = sendGridEmailService;

            var waterfallStep = new WaterfallStep[]
            {
                CrearReclamo,
                SetNombre,
                SetCIPass,
                SetCI,
                SetTlf,
                SetEmail,
                SetDescripcion,
                Confirmacion,
                FinalProcess
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallStep));
            AddDialog(new TextPrompt(_CrearReclamo, CrearReclamoValidator));
            AddDialog(new TextPrompt(_SetNombre, NameValidator));
            AddDialog(new TextPrompt(_SetCIPass, CIPassValidator));
            AddDialog(new TextPrompt(_SetCI, CIValidator));
            AddDialog(new TextPrompt(_SetTlf, TlfValidator));
            AddDialog(new TextPrompt(_SetEmail, EmailValidator));
            AddDialog(new TextPrompt(_SetDescripcion));
            AddDialog(new TextPrompt(_Confirmacion, ConfirmValidator));
        }

        #region conversationClaim
        private async Task<DialogTurnResult> CrearReclamo(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
           var option = await stepContext.PromptAsync(
                _CrearReclamo,
                new PromptOptions
                {
                    Prompt = CreateButtonsClaims(""),
                    RetryPrompt = CreateButtonsClaims("Reintento")
                }, cancellationToken
            );
            return option;
        }

        private async Task<DialogTurnResult> SetNombre(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());

            if (string.IsNullOrEmpty(ClientData.name))
            {
                if (stepContext.Context.Activity.Text.Trim().ToLower().Equals("si"))
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
                    await stepContext.Context.SendActivityAsync($"¿Hay algo en lo que te pueda ayudar?", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
            }
            else
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> SetCIPass(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());

            if (string.IsNullOrEmpty(ClientData.name))
            {
                ClientData.name = stepContext.Context.Activity.Text;
            }

            if (string.IsNullOrEmpty(ClientData.typeDoc))
            {
                var option = await stepContext.PromptAsync(
                _SetCIPass,
                new PromptOptions
                {
                    Prompt = CreateButtonsCIPass()
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
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
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
                if (luisResult.Entities.venezolano != null || luisResult.Entities.Extranjero != null)
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


        private async Task<DialogTurnResult> SetTlf(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            if (string.IsNullOrEmpty(ClientData.ci ))
            {
                ClientData.ci = stepContext.Context.Activity.Text;
            }

            if (string.IsNullOrEmpty(ClientData.phone ))
            {

                return await stepContext.PromptAsync(
                    _SetTlf,
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Ya casi terminamos, como parte de este reclamo te voy a solicitar un número de teléfono (donde nos podamos comunicar contigo) incluyendo el código de área u operadora ejemplo: 04240000000 o 02120000000"),
                        RetryPrompt = MessageFactory.Text("Por favor ingresa tu número de teléfono sin caracteres especiales:")
                    },
                cancellationToken);
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
            if (string.IsNullOrEmpty(ClientData.phone))
            {
                ClientData.phone = stepContext.Context.Activity.Text;
            }

            if (string.IsNullOrEmpty(ClientData.email ))
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
                new PromptOptions { Prompt = MessageFactory.Text("Por último, dame una breve descripción de lo que sucedió, aclarando si el percance fue vía llamada o una Agencia Bancaria") },
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
            //SEND EMAIL
            await sendEmail(stepContext);
            await stepContext.Context.SendActivityAsync($"Envié la información que me suministraste al departamento encargado para que tomen las acciones pertinentes, me disculpo por esa mala experiencia. Próximamente un especialista se comunicará contigo en el horario comprendido entre las 8:00 am  y las 2:00 pm de lunes a viernes." +
                $"{Environment.NewLine} ¿Existe algo más en lo que te pueda ayudar?", cancellationToken: cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
        #endregion conversationClaim

        private async Task sendEmail(WaterfallStepContext stepContext)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            string contentEmail = $"Un usuario ha solicitado la creación de un reclamo por el trato recibido por un empleado, a continuación la información recopilada en la conversación {Environment.NewLine}" +
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
            string to = "Ariaprueba@bancaribe.com.ve";
            //string to = "Aria@bancaribe.com.ve";
            string toName = "Aria";
            string tittle = $"{stepContext.Context.Activity.ChannelId} - Solicitud de creación de un reclamo por trato recibido de un empleado";
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
        private  Task<bool> NameValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
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
                result.Entities.venezolano != null
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
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
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

        #endregion Validators




        #region Buttons
        private Activity CreateButtonsClaims(string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                reply = MessageFactory.Text("¿Deseas crear una queja?, Tomaré la información que me proporciones para enviarla al área pertinente");
            }
            else
            {
                reply = MessageFactory.Text("¿Deseas continuar con el proceso de creación de la queja?");
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

        private Activity CreateButtonsCIPass()
        {
            var reply = MessageFactory.Text("Bien, ahora por favor dime si tu tipo de documento de identidad corresponde a Venezolano (V), Extranjero (E) o Pasaporte( P)");
            /*reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() {Title = "Cédula", Value = "Cédula" , Type = ActionTypes.ImBack},
                    new CardAction() {Title = "Pasaporte", Value = "Pasaporte" , Type = ActionTypes.ImBack}

                }
            };*/
            return reply as Activity;
        }

        #endregion Buttons


    }
}
