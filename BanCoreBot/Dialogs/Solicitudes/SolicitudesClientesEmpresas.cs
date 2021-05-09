using BanCoreBot.Infrastructure.SendGrid;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Threading;
using System.Threading.Tasks;
using BanCoreBot.Common.Utils;
using BanCoreBot.Dialogs.Cancel;
using BanCoreBot.Dialogs.Reclamos.Review;
using BanCoreBot.Common.Models.User;
using BanCoreBot.Dialogs.BloqueadoSuspendido;
using BanCoreBot.Infrastructure.Luis;
using Luis;
using BanCoreBot.Dialogs.Reclamos;

namespace BanCoreBot.Dialogs.Solicitudes
{
    public class SolicitudesClientesEmpresas : CancelDialog
    {
        private BotState _userState;
        private ILuisService _luisService;
        private readonly ISendGridEmailService _sendGridEmailService;
        private const string _SetNombre = "SetNombre";
        private const string _SetNombreOptional = "SetNombreOptional";
        private const string _SetNombreEmpresa = "SetNombreEmpresa";
        private const string _SetRIF = "SetRIF";
        private const string _SetTlf = "SetTlf";
        private const string _SetEmail = "SetEmail";
        private const string _SetDescripcion = "SetDescripcion";
        private const string _Confirmacion = "Confirmacion";
        private Utils utilitario = new Utils();
        private UserPersonalData ClientData;

        public SolicitudesClientesEmpresas(ILuisService luisService, ISendGridEmailService sendGridEmailService, PrivateConversationState userState)
            : base(nameof(SolicitudesClientesEmpresas))
        {
            _userState = userState;
            _luisService = luisService;
            _sendGridEmailService = sendGridEmailService;

            var waterfallStep = new WaterfallStep[]
            {
                SetNombre,
                SetNombreOptional,
                SetNombreEmpresa,
                SetRIF,
                SetTlf,
                SetEmail,
                SetDescripcion,
                Confirmacion,
                FinalProcess
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallStep));
            AddDialog(new TextPrompt(_SetNombre, NameValidator));
            AddDialog(new TextPrompt(_SetNombreOptional, NameValidator));
            AddDialog(new TextPrompt(_SetNombreEmpresa));
            AddDialog(new TextPrompt(_SetRIF));
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

            if (ClientData.request.Equals("tdcbloqueada"))
            {//TDC Bloqueada
                await stepContext.Context.SendActivityAsync($"Si tu tarjeta de crédito fue bloqueada te puedo ayudar enviando una solicitud al área encargada y nos estaremos comunicando contigo para ayudarte.", cancellationToken: cancellationToken);
            }

            else if (ClientData.request.Equals("consultareclamo"))
            {
                ClientData.request = "consultareclamoFromSolicitudesClientes";
            }

            else if (ClientData.request.Equals("olvidoclavetdc"))
            {
                await stepContext.Context.SendActivityAsync($"Voy a solicitarte algunos datos, que me permitan enviar tu solicitud al área encargada y nos estaremos comunicando contigo para ayudarte.", cancellationToken: cancellationToken);

            }
            else if (ClientData.request.Equals("renovacionTDC"))
            {
                await stepContext.Context.SendActivityAsync($"Para realizar la renovación o reposición de tu tarjeta de crédito, debes presentar el plástico inutilizable, vencido o bloqueado por robo o extravío.", cancellationToken: cancellationToken);
                await stepContext.Context.SendActivityAsync($"Te puedo ayudar enviando una solicitud al área encargada y nos estaremos comunicando contigo para ayudarte.", cancellationToken: cancellationToken);
            }
            else if (ClientData.request.Equals("usuariosuspendido"))
            {
                await stepContext.Context.SendActivityAsync($"Como para este caso no te puedes autogestionar, te voy a solicitar algunos datos para enviarlos al área encargada", cancellationToken: cancellationToken);
            }
            else if (ClientData.request.Equals("claveconexbancaribe") || ClientData.request.Equals("cambioclave") || ClientData.request.Equals("cambiocontraseñabancaribe"))
            {
                await stepContext.Context.SendActivityAsync($"Para solicitar tu clave temporal debo pedirte algunos datos que enviaré al área encargada y nos estaremos comunicando contigo para ayudarte.", cancellationToken: cancellationToken);
            }
            else if (ClientData.request.Equals("solicitudcartaserial"))
            {
                await stepContext.Context.SendActivityAsync($"Para habilitar o solicitar la Carta Serial, debo pedirte algunos datos que enviaré al área encargada y nos estaremos comunicando contigo para ayudarte.", cancellationToken: cancellationToken);
            
            }
            else if (ClientData.request.Equals("TjtaseguraBloq") || ClientData.request.Equals("TjtaseguraSusp"))
            {
                await stepContext.Context.SendActivityAsync($"Te informo que tu tarjeta de conexión segura jurídica no se bloquea ni se suspende, sin embargo, puede verificar si el inconveniente" +
                    $" lo presenta es el usuario, recuerde que si ingresas más de 3 veces algún dato errado al momento de realizar alguna transacción," +
                    $" el sistema por precaución no le permitirá realizar operaciones.", cancellationToken: cancellationToken);
                await stepContext.Context.SendActivityAsync($"Te puedo ayudar enviando una solicitud al área encargada y nos estaremos comunicando contigo para ayudarte.", cancellationToken: cancellationToken);
                
            }
            else if (ClientData.request.Equals("perfilseguridad") || ClientData.request.Equals("perfilseguridadsusp"))
            {
                await stepContext.Context.SendActivityAsync($"Te puedo ayudar enviando una solicitud al área encargada y nos estaremos comunicando contigo para ayudarte.", cancellationToken: cancellationToken);
                
            }
            else if (ClientData.request.Equals("problemaonline") || ClientData.request.Equals("problemaonlinejur"))
            {
                await stepContext.Context.SendActivityAsync($"Valida muy bien si estas colocando los datos de manera correcta, de ser así y el sistema no te  permite el ingreso, te recomiendo ingresar por la opción Cambiar/Recuperar Contraseña.", cancellationToken: cancellationToken);
                await stepContext.Context.SendActivityAsync($"Sí has realizado ésta acción y aún persiste el inconveniente te puedo ayudar enviando tu caso al área encargada.", cancellationToken: cancellationToken);
            }
            return await stepContext.BeginDialogAsync(nameof(SolicitarDatosDialog), cancellationToken: cancellationToken);
            /*
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
            }*/
        }




        private async Task<DialogTurnResult> SetNombreOptional(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(stepContext.Context, cancellationToken).Result;

            if (ClientData.request.Equals("consultareclamoFromSolicitudesClientes"))
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

        }


        private async Task<DialogTurnResult> SetNombreEmpresa(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());

            if (string.IsNullOrEmpty(ClientData.name))
            {
                ClientData.name = stepContext.Context.Activity.Text;
            }

            if (string.IsNullOrEmpty(ClientData.companyname))
            {
                return await stepContext.PromptAsync(
                    _SetNombreEmpresa,
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Bien, ahora dime ¿cuál es el nombre de la empresa?")
                    },
                    cancellationToken
                    );
            }
            else
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> SetRIF(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());

            if (string.IsNullOrEmpty(ClientData.companyname))
            {
                ClientData.companyname = stepContext.Context.Activity.Text;
            }

            if (string.IsNullOrEmpty(ClientData.rif))
            {
                return await stepContext.PromptAsync(
                _SetRIF,
                new PromptOptions { Prompt = MessageFactory.Text("¿cuál es el número de RIF de la empresa?") },
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

            if (string.IsNullOrEmpty(ClientData.rif))
            {
                ClientData.rif = stepContext.Context.Activity.Text;
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

            if (ClientData.request.Equals("tdcbloqueada") || ClientData.request.Equals("olvidoclavetdc"))
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
            else
            {
                return await stepContext.PromptAsync(
                    _SetDescripcion,
                    new PromptOptions { Prompt = MessageFactory.Text("Escríbeme tu requerimiento") },
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
            }
            else
            {
                ClientData.Description = stepContext.Context.Activity.Text;
            }

            ClientData.code = "juridico";
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
                string message = "Envié la información que me suministraste al departamento encargado próximamente un especialista se comunicará contigo en el horario comprendido entre las 8:00 am  y las 2:00 pm de lunes a viernes.";
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
                $"{Environment.NewLine} 📝 Nombre de la Empresa: {ClientData.companyname} " +
                $"{Environment.NewLine} 🎫 RIF: {ClientData.rif} " +
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
                        contentEmail = contentEmail + $"{Environment.NewLine} Reclamo: {elem.nroReclamo}," +
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

            if (ClientData.TextoDesborde != null)
            {
                contentEmail = contentEmail + $"{Environment.NewLine} 📄 Texto ingresado por el usuario: {ClientData.TextoDesborde}";
            }

            string from = "Aria@consein.com";
            string fromName = "Aria";
            //string to = "Aria@bancaribe.com.ve";
            string to = "Ariaprueba@bancaribe.com.ve";
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
                tittle = $"{stepContext.Context.Activity.ChannelId} - Renovación o Reposición de Tarjeta de Crédito";
            }
            else if (ClientData.request.Equals("saldocero"))
            {
                tittle = $"{stepContext.Context.Activity.ChannelId} - Solicitud de Constancia de Saldo Cero";
            }
            else if (ClientData.request.Equals("novetdcjuridico") || ClientData.request.Equals("novetdc"))
            {
                tittle = $"{stepContext.Context.Activity.ChannelId} - Un usuario no observa su TDC en Mi Conexión Bancaribe";
            }
            else if(ClientData.request.Equals("usuariosuspendido"))
            {
                tittle = $"{stepContext.Context.Activity.ChannelId} - Solicitud Cliente Jurídico - Usuario Suspendido";
            }
            else if (ClientData.request.Equals("claveconexbancaribe") || ClientData.request.Equals("cambioclave") || ClientData.request.Equals("cambiocontraseñabancaribe"))
            {
                tittle = $"{stepContext.Context.Activity.ChannelId} - Solicitud Cliente Jurídico - Clave Temporal";
            }
            else if (ClientData.request.Equals("solicitudcartaserial"))
            {
                tittle = $"{stepContext.Context.Activity.ChannelId} - Solicitud Cliente Jurídico - Carta Serial";
            }
            else if (ClientData.request.Equals("TjtaseguraBloq") || ClientData.request.Equals("TjtaseguraSusp") ||
                     ClientData.request.Equals("problemaonline") || ClientData.request.Equals("problemaonlinejur"))
            {
                tittle = $"{stepContext.Context.Activity.ChannelId} - Solicitud Cliente Jurídico - Problemas en Mi Conexión Bancaribe";
            }
            else if (ClientData.request.Equals("perfilseguridad") || ClientData.request.Equals("perfilseguridadsusp"))
            {
                tittle = $"{stepContext.Context.Activity.ChannelId} - Solicitud Cliente Jurídico - Perfil de Seguridad Suspendido";
            }

            await _sendGridEmailService.Execute(from, fromName, to, toName, tittle, contentEmail, "");

            ClientData.TextoDesborde = null;
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
