using BanCoreBot.Infrastructure.SendGrid;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Threading;
using System.Threading.Tasks;
using BanCoreBot.Common.Utils;
using BanCoreBot.Dialogs.Cancel;
using BanCoreBot.Dialogs.Reclamos.Review;
using BanCoreBot.Common.Models.User;
using Luis;
using BanCoreBot.Infrastructure.Luis;
using BanCoreBot.Dialogs.BloqueadoSuspendido;
using System.Collections.Generic;
using BanCoreBot.Dialogs.Reclamos;

namespace BanCoreBot.Dialogs.Solicitudes
{
    public class SolicitudesClientes : CancelDialog
    {
        private ILuisService _luisService;
        private BotState _userState;
        private readonly ISendGridEmailService _sendGridEmailService;
        private const string _SetNombre = "SetNombre";
        private const string _SetNombreOptional = "SetNombreOptional";
        private const string _SetCIPass = "SetCIPass";
        private const string _SetCI = "SetCI";
        private const string _SetTlf = "SetTlf";
        private const string _SetEmail = "SetEmail";
        private const string _SetDescripcion = "SetDescripcion";
        private const string _Confirmacion = "Confirmacion";
        private Utils utilitario = new Utils();
        private UserPersonalData ClientData;

        public SolicitudesClientes(ISendGridEmailService sendGridEmailService, PrivateConversationState userState, ILuisService luisService, IBotTelemetryClient telemetryClient)
            : base(nameof(SolicitudesClientes))
        {
            _luisService = luisService;
            _userState = userState;
            _sendGridEmailService = sendGridEmailService;
            this.TelemetryClient = telemetryClient;

            var waterfallStep = new WaterfallStep[]
            {
                SetNombre,
                SetNombreOptional,
                SetCIPass,
                SetCI,
                SetTlf,
                SetEmail,
                SetDescripcion,
                Confirmacion,
                FinalProcess
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallStep));
            AddDialog(new TextPrompt(_SetNombre, NameValidator));
            AddDialog(new TextPrompt(_SetNombreOptional, NameValidator));
            AddDialog(new TextPrompt(_SetCIPass, CIPassValidator));
            AddDialog(new TextPrompt(_SetCI, CIValidator));
            AddDialog(new TextPrompt(_SetTlf, TlfValidator));
            AddDialog(new TextPrompt(_SetEmail, EmailValidator));  
            AddDialog(new TextPrompt(_SetDescripcion));
            AddDialog(new TextPrompt(_Confirmacion, ConfirmValidator));
        }

        #region conversationClaim


        
        private async Task<DialogTurnResult> SetNombre(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            if (String.IsNullOrEmpty(ClientData.request))
            {
                ClientData.request = "";
            }
            if (String.IsNullOrEmpty(ClientData.TextoDesborde))
            {
                ClientData.TextoDesborde = "";
            }
            
            if (ClientData.request.Equals("tdcbloqueada"))
            {//TDC Bloqueada
                await stepContext.Context.SendActivityAsync($"Si tu tarjeta de crédito fue bloqueada te puedo ayudar enviando una solicitud al área encargada y nos estaremos comunicando contigo para ayudarte.", cancellationToken: cancellationToken);
            }
            else if (ClientData.request.Equals("olvidoclavetdc"))
            {
                await stepContext.Context.SendActivityAsync($"Voy a solicitarte algunos datos, que me permitan enviar tu solicitud al área encargada.", cancellationToken: cancellationToken);

            }

            else if (ClientData.request.Equals("consultareclamo"))
            {
                ClientData.request = "consultareclamoFromSolicitudesClientes";
            }

            else if (ClientData.request.Equals("renovacionTDC"))
            {
                await stepContext.Context.SendActivityAsync($"Para realizar la renovación o reposición de tu tarjeta de crédito, debes presentar el plástico inutilizable, vencido o bloqueado por robo o extravío.", cancellationToken: cancellationToken);
                await stepContext.Context.SendActivityAsync($"Te puedo ayudar enviando una solicitud al área encargada y nos estaremos comunicando contigo para ayudarte.", cancellationToken: cancellationToken);
            }
            else if (ClientData.request.Equals("perfilseguridadsusp"))
            {
                await stepContext.Context.SendActivityAsync($"Debido a que ingresaste diversos datos errados, tu Perfil de Seguridad por motivos de precaución fue suspendido, te puedo ayudar enviando una solicitud al área encargada y nos estaremos comunicando contigo para ayudarte.", cancellationToken: cancellationToken);
            }
            else if (ClientData.request.Equals("reclamoDomiciliacion"))
            {
                await stepContext.Context.SendActivityAsync($"Con gusto te puedo ayudar enviando una solicitud al área encargada y nos estaremos comunicando contigo para ayudarte.", cancellationToken: cancellationToken);
            }
            return await stepContext.BeginDialogAsync(nameof(SolicitarDatosDialog), cancellationToken: cancellationToken);
            /*
            
            else if (ClientData.request.Equals("desborde") || ClientData.request.Equals("consultareclamo") || ClientData.request.Equals("dudasaldotdc") || ClientData.request.Equals("renovacionTDC"))
            {
               return await stepContext.BeginDialogAsync(nameof(SolicitarDatosDialog), cancellationToken: cancellationToken);
            } 
           
            if (string.IsNullOrEmpty(ClientData.name))
            {
                return await stepContext.PromptAsync(
                _SetNombre,
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Lo primero que necesito saber es ¿cual es tu nombre y apellido?"),
                    RetryPrompt = MessageFactory.Text("Por favor indicame tu nombre y apellido, no incluyas números ni caracteres especiales.")
                },
                cancellationToken
                );
            }
            else
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
            */

        }

        private async Task<DialogTurnResult> SetNombreOptional(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Dictionary<string, string> Telemetry = new Dictionary<string, string>();
            Telemetry.Add("From", stepContext.Context.Activity.From.Id);
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(stepContext.Context, cancellationToken).Result;
            if(ClientData.request.Equals("consultareclamoFromSolicitudesClientes"))
            {
                if (!luisResult.TopIntent().intent.ToString().Equals("afirmacion"))
                {
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
                else
                {
                    ClientData.request = "consultareclamo";
                }
            }

            if (ClientData.request.Equals("desborde"))
            {
                if (!luisResult.TopIntent().intent.ToString().Equals("afirmacion"))
                {
                    string contentEmail = "";
                    string tittle = "";
                    string to = "";
                    if (ClientData.TextoDesborde.Equals("Aria recibió una imagen por mensaje directo"))
                    {
                        contentEmail = $"Un usuario ha realizado una solicitud que Aria no pudo manejar, sin embargo, dicho usuario no quiso proporcionar sus datos para que sea contactado, " +
                               $"a continuación el detalle que activó el mecanismo del desborde de Aria {Environment.NewLine}" +
                               $"{Environment.NewLine} 📄 Canal: {stepContext.Context.Activity.ChannelId} " +
                               $"{Environment.NewLine} 📄 Evento que Aria no pudo manejar: {ClientData.TextoDesborde}";
                        tittle = $"Desborde {stepContext.Context.Activity.ChannelId} - {ClientData.TextoDesborde}";
                        //to = "ariaredessociales@bancaribe.com.ve";
                        to = "AriaPrueba@bancaribe.com.ve";
                        TelemetryClient.TrackEvent("ImagenTwitter", Telemetry);
                        
                    }
                    else
                    {
                        contentEmail = $"Un usuario ha realizado una solicitud que Aria no pudo manejar, sin embargo, dicho usuario no quiso proporcionar sus datos para que sea contactado, " +
                            $"a continuación la frase que activó el mecanismo del desborde de Aria {Environment.NewLine}" +
                            $"{Environment.NewLine} 📄 Canal: {stepContext.Context.Activity.ChannelId} " +
                            $"{Environment.NewLine} 📄 Texto ingresado por el usuario que Aria no pudo manejar: {ClientData.TextoDesborde}";
                        tittle = $"Entrenamiento {stepContext.Context.Activity.ChannelId} - {ClientData.TextoDesborde}";
                        //to = "EntrenamientoBot@bancaribe.com.ve";
                        to = "AriaPrueba@bancaribe.com.ve";
                        TelemetryClient.TrackEvent("Entrenamiento", Telemetry);
                    }
                    if (stepContext.Context.Activity.ChannelId.Equals("twitter"))
                    {
                        contentEmail = contentEmail + $"{Environment.NewLine} 🗣 Usuario: @{stepContext.Context.Activity.From.Name} "; 
                    }
                    string from = "Aria@consein.com";
                    string fromName = "Aria";
                    string toName = "Entrenamiento Bot";
                    await _sendGridEmailService.Execute(from, fromName, to, toName, tittle, contentEmail, "");

                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
                if (string.IsNullOrEmpty(ClientData.name))
                {
                    return await stepContext.PromptAsync(
                    _SetNombre,
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Lo primero que necesito saber es ¿cuál es tu nombre completo?"),
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

            /*if (ClientData.request.Equals("consultareclamo") || ClientData.request.Equals("dudasaldotdc") || ClientData.request.Equals("renovacionTDC"))
            {*/

            if (!luisResult.TopIntent().intent.ToString().Equals("afirmacion"))
            {
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
            if (string.IsNullOrEmpty(ClientData.name))
            {
                return await stepContext.PromptAsync(
                _SetNombre,
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Lo primero que necesito saber es ¿cuál es tu nombre completo?"),
                    RetryPrompt = MessageFactory.Text("Por favor indicame tu nombre y apellido, no incluyas números ni caracteres especiales.")
                },
                cancellationToken
                );
            }
            else
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }

            /*}
            else
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }*/
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
            if (string.IsNullOrEmpty(ClientData.ci))
            {
                ClientData.ci = stepContext.Context.Activity.Text;
            }

            if (string.IsNullOrEmpty(ClientData.phone))
            {
                return await stepContext.PromptAsync(
                    _SetTlf,
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Ya casi terminamos, te voy a solicitar un número de teléfono (donde nos podamos comunicar contigo) incluyendo el código de área u operadora ejemplo: 04240000000 o 02120000000"),
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

            if (string.IsNullOrEmpty(ClientData.email))
            {
                return await stepContext.PromptAsync(
                    _SetEmail,
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Por favor ingresa tu dirección de correo electrónico"),
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

            if (ClientData.request.Equals("tdcbloqueada") || ClientData.request.Equals("olvidoclavetdc"))
            {
               return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }

            else
            {
                var message = "Escríbeme tu requerimiento";
                return await stepContext.PromptAsync(
                    _SetDescripcion,
                    new PromptOptions { Prompt = MessageFactory.Text(message) },
                    cancellationToken
                    );
            }
        }

        private async Task<DialogTurnResult> Confirmacion(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());

            if (ClientData.request.Equals("tdcbloqueada"))
            {
                ClientData.Description = "Tarjeta de Crédito Bloqueada";
            }
            else if (ClientData.request.Equals("olvidoclavetdc"))
            {
                ClientData.Description = "Asignar/Cambiar clave tarjeta de crédito";
            }
            else if (ClientData.request.Equals("consultareclamo"))
            {
                ClientData.desbordeQueryClaim = true;
                ClientData.Description = stepContext.Context.Activity.Text;
            }
            else if (ClientData.request.Equals("dudasaldotdc"))
            {
                ClientData.Description = "Consulta referente al saldo de la tarjeta de crédito";
            }
            else 
            { 
                ClientData.Description = stepContext.Context.Activity.Text;
            }

            ClientData.code = "natural";
            return await stepContext.BeginDialogAsync(nameof(ReviewConfirmDialog), cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> FinalProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(stepContext.Context, cancellationToken).Result;
            if (luisResult.TopIntent().intent.ToString().Equals("afirmacion") || stepContext.Context.Activity.Text.ToLower().Equals("si"))
            {
                //SEND EMAIL
                await sendEmail(stepContext);
                string message = "Envié la información que me suministraste al departamento encargado próximamente un especialista se comunicará contigo en nuestro horario comprendido entre las 8:00 a.m. y las 5:00 p.m de lunes a viernes.";
                /*if (utilitario.ValidateOfficeHours())
                {
                    message = "He enviado la información que me has suministrado al departamento encargado y un especialista se estará comunicando contigo.";
                }
                else
                {
                    message = "He enviado la información que me has suministrado al departamento encargado y un especialista se estará comunicando contigo";
                }*/

                await stepContext.Context.SendActivityAsync(message, cancellationToken: cancellationToken);

                if (ClientData.request.Equals("tdcbloqueada") || ClientData.request.Equals("olvidoclavetdc")
                    || ClientData.request.Equals("finiquito") || ClientData.request.Equals("saldocero") ||
                    ClientData.request.Equals("renovacionTDC"))
                {
                    await stepContext.Context.SendActivityAsync("Te recomiendo poseer tu tarjeta de crédito a la mano cuando nos comuniquemos contigo. ¿Existe algo más en lo que te pueda ayudar?", cancellationToken: cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync("¿Existe algo más en lo que te pueda ayudar?", cancellationToken: cancellationToken);
                }

                ClientData.request = "";

            }
            else
            {
                await stepContext.Context.SendActivityAsync("Entiendo, ¿Existe algo más en lo que te pueda ayudar?", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }
        #endregion conversationClaim

        private async Task sendEmail(WaterfallStepContext stepContext)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            string contentEmail = $"Un usuario ha realizado una solicitud a continuación la información recopilada en la conversación {Environment.NewLine}" +
                $"{Environment.NewLine} 📝 Nombre y Apellido: {ClientData.name} " +
                $"{Environment.NewLine} 🎫 Documento de identidad: {ClientData.typeDoc} {ClientData.ci} " +
                $"{Environment.NewLine} ☎ Teléfono: {ClientData.phone}" +
                $"{Environment.NewLine} 📧 Correo: {ClientData.email}" +
                $"{Environment.NewLine} 📄 Canal: {stepContext.Context.Activity.ChannelId} " +
                $"{Environment.NewLine} 📄 Descripción: {ClientData.Description}";

            if (ClientData.listaReclamos != null)
            {
                foreach (var elem in ClientData.listaReclamos)
                {
                    if (String.IsNullOrEmpty(getStatusFromEnum(elem.codEstado.ToString() + elem.codEstatus.ToString())))
                    {
                        contentEmail = contentEmail + $"{Environment.NewLine} Reclamo: {elem.nroReclamo},"+ 
                            $" Estatus: {elem.codEstado}  -  {elem.codEstatus}";
                    }
                }
                ClientData.listaReclamos = null;
            }

            if (ClientData.objectReclamo != null)
            {
                if (String.IsNullOrEmpty(getStatusFromEnum(ClientData.objectReclamo.codEstado.ToString() + ClientData.objectReclamo.codEstatus.ToString())))
                {
                    contentEmail = contentEmail + $"{Environment.NewLine} Reclamo: {ClientData.objectReclamo.nroReclamo}," + 
                        $" Estatus: {ClientData.objectReclamo.codEstado}  -  {ClientData.objectReclamo.codEstatus}";
                }
                ClientData.objectReclamo = null;
            }
            
            if (stepContext.Context.Activity.ChannelId.Equals("twitter"))
            {
                contentEmail = contentEmail + $"{Environment.NewLine} 🗣 Usuario: @{stepContext.Context.Activity.From.Name} ";
            }
            
            if (!String.IsNullOrEmpty(ClientData.TextoDesborde))
            {
                if (ClientData.TextoDesborde.Equals("Aria recibió una imagen por mensaje directo"))
                {
                    contentEmail = contentEmail + $"{Environment.NewLine} 📄 Evento que Aria no pudo manejar: {ClientData.TextoDesborde}";
                }
                else
                {
                    contentEmail = contentEmail + $"{Environment.NewLine} 📄 Texto ingresado por el usuario que Aria no pudo manejar: {ClientData.TextoDesborde}";
                }
            }

            string from = "Aria@consein.com";
            string fromName = "Aria";
            //string to = "Aria@bancaribe.com.ve";
            string to = "Ariaprueba@bancaribe.com.ve";
            if (!String.IsNullOrEmpty(ClientData.TextoDesborde))
            {
                if (ClientData.TextoDesborde.Equals("Aria recibió una imagen por mensaje directo"))
                {
                    //to = "ariaredessociales@bancaribe.com.ve";
                    to = "Ariaprueba@bancaribe.com.ve";
                }
            } 
                
            
            string toName = "Aria";
            string tittle = $"{stepContext.Context.Activity.ChannelId} - Solicitud de un Usuario";
            if (ClientData.request.Equals("tdcbloqueada"))
            {
                tittle = $"{stepContext.Context.Activity.ChannelId} - Solicitud de desbloqueo de una TDC";
            }
            else if (ClientData.request.Equals("robotdc"))
            {
                tittle = $"{stepContext.Context.Activity.ChannelId} - Solicitud de bloqueo de TDC por robo";
            }
            else if (ClientData.request.Equals("perdidatdc"))
            {
                tittle = $"{stepContext.Context.Activity.ChannelId} - Solicitud de bloqueo de TDC por perdida";
            }
            else if (ClientData.request.Equals("bloqueotdc"))
            {
                tittle = $"{stepContext.Context.Activity.ChannelId} - Solicitud de bloqueo de TDC";
            }
            else if (ClientData.request.Equals("olvidoclavetdc"))
            {
                tittle = $"{stepContext.Context.Activity.ChannelId} - Asignar/Cambiar clave de TDC";
            }
            else if (ClientData.request.Equals("finiquito"))
            {
                tittle = $"{stepContext.Context.Activity.ChannelId} - Solicitud de Carta Finiquito";
            }
            else if (ClientData.request.Equals("renovacionTDC"))
            {
                tittle = $"{stepContext.Context.Activity.ChannelId} - Renovación de Tarjeta de Crédito";
            }
            else if (ClientData.request.Equals("saldocero"))
            {
                tittle = $"{stepContext.Context.Activity.ChannelId} - Solicitud de Constancia de Saldo Cero";
            }
            else if (ClientData.request.Equals("novetdcnatural") || ClientData.request.Equals("novetdc"))
            {
                tittle = $"{stepContext.Context.Activity.ChannelId} - Un usuario no observa su TDC en Mi Conexión Bancaribe";
            }

            else if (ClientData.request.Equals("perfilseguridadsusp"))
            {
                tittle = $"{stepContext.Context.Activity.ChannelId} - Un usuario tiene su perfil de seguridad suspendido";
            }
            else if (ClientData.request.Equals("desborde"))
            {
                tittle = $"Desborde {stepContext.Context.Activity.ChannelId} - {ClientData.TextoDesborde}";
            }
            else if (ClientData.request.Equals("consultareclamo"))
            {
                if (ClientData.APIError )
                {
                    tittle = $"{stepContext.Context.Activity.ChannelId} - Ocurrió un error al Aria intentar consumir la API - Un usuario quiere consultar un reclamo ";
                    ClientData.APIError = false;
                }
                else
                {
                    tittle = $"{stepContext.Context.Activity.ChannelId} - Un usuario quiere consultar un reclamo que generó";
                }
            }
            else if (ClientData.request.Equals("dudasaldotdc"))
            {
                tittle = $"{stepContext.Context.Activity.ChannelId} - Un usuario tiene inconvenientes para consultar su TDC";
            }
            else if (ClientData.request.Equals("reclamoDomiciliacion"))
            {
                tittle = $"{stepContext.Context.Activity.ChannelId} - Un usuario tiene un requerimiento por las domiciliaciones en su cuenta";
            }
            await _sendGridEmailService.Execute(from, fromName, to, toName, tittle, contentEmail, "");
            
            Dictionary<string, string> Telemetry = new Dictionary<string, string>();
            Telemetry.Add("From", stepContext.Context.Activity.From.Id);
            if (ClientData.request.Equals("desborde"))
            {
                TelemetryClient.TrackEvent("Desborde", Telemetry);
            }
            else 
            {
                TelemetryClient.TrackEvent("Requerimiento", Telemetry);
            }

            ClientData.TextoDesborde = "";

        }

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
            var result = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(promptContext.Context, cancellationToken).Result;

            if (result.TopIntent().intent.ToString().Equals("afirmacion")
                || promptContext.Context.Activity.Text.ToLower().Equals("si")
                || promptContext.Context.Activity.Text.ToLower().Equals("no")
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




        #region Buttons

        private Activity CreateButtonsCIPass()
        {
            var reply = MessageFactory.Text("Bien, ahora por favor dime si tu tipo de documento de identidad corresponde a Venezolano (V), Extranjero (E) o Pasaporte( P)");
            
            return reply as Activity;
        }

        #endregion Buttons

        public string getStatusFromEnum(string codString)
        {
            if (codString.StartsWith("A"))
            {
                return EnumStatusClaim.A;
            }
            else if (codString.StartsWith("D"))
            {
                return EnumStatusClaim.D;
            }
            else if (codString.StartsWith("V"))
            {
                return EnumStatusClaim.V;
            }
            else if (codString.StartsWith("PD"))
            {
                return EnumStatusClaim.PD;
            }
            else if (codString.StartsWith("PV"))
            {
                return EnumStatusClaim.PV;
            }

            switch (codString)
            {
                case "C20":
                    return EnumStatusClaim.C20;
                case "C21":
                    return EnumStatusClaim.C21;
                case "C50":
                    return EnumStatusClaim.C50;
                case "CA20":
                    return EnumStatusClaim.CA20;
                case "CA22":
                    return EnumStatusClaim.CA22;
                case "CN21":
                    return EnumStatusClaim.CN21;
                case "CNA20":
                    return EnumStatusClaim.CNA20;
                case "CPN21":
                    return EnumStatusClaim.CPN21;
                default:
                    return "";
            }
        }

    }
}
