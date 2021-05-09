using BanCoreBot.Common.Models.User;
using BanCoreBot.Dialogs.ClienteNuevo;
using BanCoreBot.Dialogs.UsuarioClave;
using BanCoreBot.Dialogs.Ctas;
using BanCoreBot.Dialogs.DatosIniciales;
using BanCoreBot.Dialogs.MiPago;
using BanCoreBot.Dialogs.MontosMaximos;
using BanCoreBot.Dialogs.Pos;
using BanCoreBot.Dialogs.Reclamos;
using BanCoreBot.Dialogs.Reclamos.Review;
using BanCoreBot.Dialogs.Solicitudes;
using BanCoreBot.Dialogs.Tarjetas;
using BanCoreBot.Dialogs.Transferencia;
using BanCoreBot.Infrastructure.Luis;
using BanCoreBot.Infrastructure.QnAMakerAI;
using BanCoreBot.Infrastructure.SendGrid;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanCoreBot.Dialogs.BloqueadoSuspendido;
using BanCoreBot.Dialogs.CartaSerial;
using BanCoreBot.Dialogs.PerfilSeguridad;
using BanCoreBot.Common.Utils;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace BanCoreBot.Dialogs
{
    public class RootDialog : ComponentDialog
    {
        private BotState _userState;
        private readonly ILuisService _luisService;
        private readonly ISendGridEmailService _sendGridEmailService;
        private readonly IQnAMakerAIService _qnAMakerAIService;
        private UserPersonalData ClientData;
        protected readonly ILogger Logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private Utils utilitario = new Utils();

        public RootDialog( PrivateConversationState userState, ILuisService luisService, ILogger<RootDialog> logger, ISendGridEmailService sendGridEmailService, IQnAMakerAIService qnAMakerAIService, IBotTelemetryClient telemetryClient, IHttpClientFactory httpClientFactory)
        {
            _userState = userState;
            Logger = logger;
            _luisService = luisService;
            _sendGridEmailService = sendGridEmailService;
            _qnAMakerAIService = qnAMakerAIService;
            this.TelemetryClient = telemetryClient;
            _httpClientFactory = httpClientFactory;

            var waterfallSteps = new WaterfallStep[]
            {
                InitialProcess,
                FinalProcess
            };
            
            AddDialog(new DatosInic(userState, luisService));
            AddDialog(new CtasDialog());
            AddDialog(new AllClaims(luisService, userState));
            AddDialog(new ReviewConfirmDialog(userState)); 
            AddDialog(new ReviewNameDialog(userState));
            AddDialog(new ReviewCIOrPassDialog(userState, luisService));
            AddDialog(new ReviewPhoneDialog(userState));
            AddDialog(new ReviewMailDialog(userState));
            AddDialog(new ReviewDateDialog(userState));
            AddDialog(new ReviewAmountDialog(userState));
            AddDialog(new Review4UltCtaDialog(userState));
            AddDialog(new Review4UltTjtaDialog(userState));
            AddDialog(new ReviewTlfAltDialog(userState));
            AddDialog(new ReviewNameDestDialog(userState));
            AddDialog(new ReviewCompanyNameDialog(userState));
            AddDialog(new ReviewNameOtherBankDialog(userState)); 
            AddDialog(new ReviewNumCtaBenefDialog(userState)); 
            AddDialog(new ReviewSenderPhoneDialog(userState)); 
            AddDialog(new ReviewReceiverPhoneDialog(userState));
            AddDialog(new ReviewPhoneRefillDialog(userState));
            AddDialog(new ReviewDescriptionDialog(userState));
            AddDialog(new ReviewRIFDialog(userState));
            AddDialog(new ReclamoPOS(_sendGridEmailService, userState));
            AddDialog(new ReclamoPOSFromAllClaims(_sendGridEmailService, userState)); 
            AddDialog(new ReclamoMiPago(_sendGridEmailService, userState));
            AddDialog(new ReclamoMiPagoFromAllClaims( _sendGridEmailService, userState));
            AddDialog(new ReclamoRecarga(_sendGridEmailService, userState));
            AddDialog(new ReclamoRecargaFromAllClaims( _sendGridEmailService, userState));
            AddDialog(new ReclamoCajero(_sendGridEmailService, userState));
            AddDialog(new ReclamoCajeroFromAllClaims( _sendGridEmailService, userState));
            AddDialog(new ReclamoTDC(_sendGridEmailService, userState));
            AddDialog(new ReclamoTDCFromAllClaims( _sendGridEmailService, userState));
            AddDialog(new ReclamoTransferencia(luisService, _sendGridEmailService, userState));
            AddDialog(new ReclamoTransferenciaFromAllClaims( _sendGridEmailService, userState)); 
            AddDialog(new ReclamoEmpleado(_sendGridEmailService,userState, luisService)); 
            AddDialog(new InitReclamoPOS(luisService));
            AddDialog(new CtasCteDialog());
            AddDialog(new AfiliarmeMiPago(luisService));
            AddDialog(new AfiliacionMiPagoBancaribe(luisService)); 
            AddDialog(new PagoRapidoDialog(luisService));
            AddDialog(new PagoSMSDialog(luisService));
            AddDialog(new TlfSimpleDialog(luisService));
            AddDialog(new MontoMaximoTransferencia(luisService));
            AddDialog(new MontoMaximoTarjeta(luisService));
            AddDialog(new MontoMaximoGeneral(luisService)); 
            AddDialog(new TransferenciaDialog(luisService));
            AddDialog(new NoVeUsuarioDialog(luisService));
            AddDialog(new TransfDialog(luisService));
            AddDialog(new SolicitudesClientes(_sendGridEmailService, userState, luisService, telemetryClient)); 
            AddDialog(new ProblemasEnvioMiPagoDialog(luisService));
            AddDialog(new TarjetaBloqueadaDialog(luisService, userState)); 
            AddDialog(new NaturalJuridicoDialog(luisService, userState)); 
            AddDialog(new TipoTarjetaDialog(luisService, userState));
            AddDialog(new ClaveDialog(luisService, userState)); 
            AddDialog(new ConsultarReclamoDialog(_sendGridEmailService,luisService, userState, httpClientFactory));
            AddDialog(new BloqueadoSuspendidoDialog(luisService, userState));
            AddDialog(new RazonBloqueoDialog(luisService, userState));
            AddDialog(new OlvidoClaveDialog(luisService, userState)); 
            AddDialog(new OlvidoClaveTDCDialog(luisService, userState));
            AddDialog(new CartaSerialDialog(luisService, userState)); 
            AddDialog(new SolicitarDatosDialog(luisService, userState));
            AddDialog(new BloquearDesbloquearTjtaDialog(luisService, userState));
            AddDialog(new ConsultaSaldoTDC(userState, luisService)); 
            AddDialog(new ClienteNuevoDialog(luisService)); 
            AddDialog(new UsoPagoTDCDialog(luisService, userState));
            AddDialog(new IsTDCDialog(luisService, userState));
            AddDialog(new PerfilSeguridadDialog(luisService, userState));
            AddDialog(new RenovacionTDDDialog(luisService, userState,qnAMakerAIService));
            AddDialog(new CtasGeneralDialog(luisService, userState, qnAMakerAIService));
            AddDialog(new FechaTransferenciaReclamo(userState, luisService)); 
            AddDialog(new SolicitudesClientesEmpresas(luisService, _sendGridEmailService, userState));
            AddDialog(new ErrorConexDialog()); 
            AddDialog(new ReclamoNoAbonadoDialog(_sendGridEmailService, userState, luisService));
            AddDialog(new NoVeTDCDialog(luisService, userState));
            AddDialog(new ReviewConsultaReclamoDialog(userState));
            AddDialog(new ReviewDocumentIdDialog(userState, luisService));
            AddDialog(new ReviewNroReclamoDialog(userState, luisService));
            AddDialog(new TextPrompt(nameof(TextPrompt))); 
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> InitialProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            if (stepContext.Context.Activity.Attachments == null)
            {
                if (stepContext.Context.Activity.Text.Length > 500)
                {
                    var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
                    ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
                    ClientData.texto500 = stepContext.Context.Activity.Text;
                    stepContext.Context.Activity.Text = stepContext.Context.Activity.Text.Substring(0,499);
                }

                var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(stepContext.Context, cancellationToken).Result;
                //luisResult.Sentiment.Last().Value

                return await ManageIntentions(stepContext, luisResult, cancellationToken);
            }
            else
            {
                var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
                ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
                ClientData.request = "desborde";
                ClientData.TextoDesborde = $"Aria recibió una imagen por mensaje directo"; 
                await stepContext.Context.SendActivityAsync($"Disculpa no puedo identificar el detalle de la imagen, pero puedo tomar tu solicitud y enviarla al área encargada", cancellationToken: cancellationToken);
                return await stepContext.BeginDialogAsync(nameof(SolicitudesClientes), cancellationToken: cancellationToken);
            }

        }

        private async Task<DialogTurnResult> ManageIntentions(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());

            var topIntent = luisResult.TopIntent();
            if (topIntent.score > 0.50)
            {
                if (!topIntent.intent.ToString().Equals("Saludar"))
                {
                    ClientData.ClienteAtendidoAux = true;
                }
                switch (topIntent.intent.ToString())
                {
                    case "Saludar":
                        return await IntentSaludar(stepContext, luisResult, cancellationToken);
                    case "AfiliacionPagoMovil":
                        return await IntentAfiliacionPagoMovil(stepContext, luisResult, cancellationToken);
                    case "ActivacionTDC":
                        return await IntentActivacionTDC(stepContext, luisResult, cancellationToken);
                    case "afirmacion":
                        return await IntentAfirmacion(stepContext, luisResult, cancellationToken);
                    case "InfoPtoVenta":
                        return await IntentInfoPtoVenta(stepContext, luisResult, cancellationToken);
                    case "AgenciasMetroOeste":
                        return await IntentAgencias(stepContext, luisResult, cancellationToken);
                    case "AumentoLimiteTDC":
                        return await IntentAumentoLimiteTDC(stepContext, luisResult, cancellationToken);
                    case "AppConexionDigital":
                        return await IntentAppConexionDigital(stepContext, luisResult, cancellationToken);
                    case "ClienteNuevo":
                        return await IntentClienteNuevo(stepContext, luisResult, cancellationToken);
                    case "CartaSerial":
                        return await IntentCartaSerial(stepContext, luisResult, cancellationToken);
                    case "TiempoPagoTDC":
                        return await IntentTiempoPagoTDC(stepContext, luisResult, cancellationToken);
                    case "conexionBancaribe":
                        return await IntentConexionBancaribe(stepContext, luisResult, cancellationToken);
                    case "Opciones":
                        return await IntentConexionBancaribe(stepContext, luisResult, cancellationToken);
                    case "Contacto":
                        return await IntentContacto(stepContext, luisResult, cancellationToken); 
                    case "EmojisTwitter":
                        return await IntentEmojisTwitter(stepContext, luisResult, cancellationToken);
                    case "OlvidoClave":
                        return await IntentOlvidoClave(stepContext, luisResult, cancellationToken);
                    case "GestionEnLinea":
                        return await IntentGestionEnLinea(stepContext, luisResult, cancellationToken);
                    case "PreAperturaCta":
                        return await IntentPreAperturaCta(stepContext, luisResult, cancellationToken);
                    case "Pensionado":
                        return await IntentPensionado(stepContext, luisResult, cancellationToken);
                    case "MontosMaximos":
                        return await IntentMontosMaximos(stepContext, luisResult, cancellationToken);
                    case "PagoMovilSMS":
                        return await IntentPagoMovilSMS(stepContext, luisResult, cancellationToken);
                    case "RegistroConexBancaribe":
                        return await IntentRegistroConexBancaribe(stepContext, luisResult, cancellationToken);
                    case "transferencia":
                        return await IntentTransferencia(stepContext, luisResult, cancellationToken);
                    case "TiempoTransferencia":
                        return await IntentTiempoTransferencia(stepContext, luisResult, cancellationToken);
                    //  case "TarjetaBloqueada":
                    //      return await IntentTarjetaBloqueada(stepContext, luisResult, cancellationToken);
                    case "TDCSaldoCero":
                        return await IntentTDCSaldoCero(stepContext, luisResult, cancellationToken);
                    case "TipoTarjeta":
                        return await IntentTipoTarjeta(stepContext, luisResult, cancellationToken);
                    case "TipoPersona":
                        return await IntentTipoPersona(stepContext, luisResult, cancellationToken);
                    case "NoVeTDC":
                        return await IntentNoVeTDC(stepContext, luisResult, cancellationToken);
                    case "BloquearDesbloquearTarjeta":
                        return await IntentBloquearDesbloquearTarjeta(stepContext, luisResult, cancellationToken);
                    case "ServiciosMontoLimite":
                        if (luisResult.Entities.transferir != null)
                        {
                            return await IntentTransferencia(stepContext, luisResult, cancellationToken);
                        }
                        else
                        {
                            await stepContext.Context.SendActivityAsync("Requiero más información para ayudarte", cancellationToken: cancellationToken);
                            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
                        }
                    case "BloqueadoSuspendido":
                        return await IntentBloqueadoSuspendido(stepContext, luisResult, cancellationToken);
                    case "ComoPagoTDC":
                        return await IntentComoPagoTDC(stepContext, luisResult, cancellationToken);
                    case "RenovacionTarjeta":
                        return await IntentRenovacionTarjeta(stepContext, luisResult, cancellationToken);
                    //case "NoEsUnJuego":
                    //    return await IntentNoEsUnJuego(stepContext, luisResult, cancellationToken); 
                    case "ConsultaSaldo":
                        return await IntentConsultaSaldo(stepContext, luisResult, cancellationToken);
                    case "ConsultaSaldoTDC":
                        return await IntentConsultaSaldoTDC(stepContext, luisResult, cancellationToken);
                    case "Despedida":
                        return await IntentDespedida(stepContext, luisResult, cancellationToken);
                    case "Tarifa":
                        return await IntentTarifa(stepContext, luisResult, cancellationToken);
                    //case "TiempoReclamo":
                    //    return await IntentTiempoReclamo(stepContext, luisResult, cancellationToken);
                    case "Tasas":
                        return await IntentTasas(stepContext, luisResult, cancellationToken);
                    case "redes":
                        return await IntentRedes(stepContext, luisResult, cancellationToken);
                    case "PerfilSeguridad":
                        return await IntentPerfilSeguridad(stepContext, luisResult, cancellationToken);
                    case "TarjetaConexionSegura":
                        return await IntentTarjetaConexionSegura(stepContext, luisResult, cancellationToken);
                    case "Reclamo":
                        return await IntentReclamo(stepContext, luisResult, cancellationToken);
                    case "reclamoNoAbonado":
                        return await IntentReclamoNoAbonado(stepContext, luisResult, cancellationToken);
                    case "ReclamoPOS":
                        return await IntentReclamoPOS(stepContext, luisResult, cancellationToken);
                    case "ReclamoPagoMovil":
                        return await IntentReclamoPagoMovil(stepContext, luisResult, cancellationToken);
                    case "ReclamoRecarga":
                        return await IntentReclamoRecarga(stepContext, luisResult, cancellationToken);
                    case "ReclamoCajero":
                        return await IntentReclamoCajero(stepContext, luisResult, cancellationToken);
                    case "ReclamoTDC":
                        return await IntentReclamoTDC(stepContext, luisResult, cancellationToken);
                    case "ReclamoTransferencia":
                       return await IntentReclamoTransferencia(stepContext, luisResult, cancellationToken);
                    case "negación":
                        return await IntentNegacion(stepContext, luisResult, cancellationToken);
                    case "None":
                        return await IntentNone(stepContext, luisResult, cancellationToken);
                    default:
                        break;

                }
            }
            else
            {
                return await IntentNone(stepContext, luisResult, cancellationToken);
            }
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }



        #region LuisIntent

        private async Task<DialogTurnResult> IntentAppConexionDigital(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            stepContext.Context.Activity.Text = stepContext.Context.Activity.Text.ToLower();
            var resultQnA = await _qnAMakerAIService._qnAMakerResult.GetAnswersAsync(stepContext.Context);
            var score = resultQnA.FirstOrDefault()?.Score;
            string response = resultQnA.FirstOrDefault()?.Answer;

            if (score >= 0.80)
            {
                await stepContext.Context.SendActivityAsync(response, cancellationToken: cancellationToken);
            }
            else
            {
                var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
                ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
                ClientData.request = "desborde"; 
                if (string.IsNullOrEmpty(ClientData.texto500))
                {
                    ClientData.TextoDesborde = stepContext.Context.Activity.Text;
                }
                else
                {
                    ClientData.TextoDesborde = ClientData.texto500;
                    ClientData.texto500 = null;
                }
                await stepContext.Context.SendActivityAsync($"Disculpa la información que me solicitas no la manejo aún, pero puedo tomar tu solicitud y enviarla al área encargada", cancellationToken: cancellationToken);
                return await stepContext.BeginDialogAsync(nameof(SolicitudesClientes), cancellationToken: cancellationToken);
            }
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> IntentNone(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            stepContext.Context.Activity.Text = stepContext.Context.Activity.Text.ToLower();
            var resultQnA = await _qnAMakerAIService._qnAMakerResult.GetAnswersAsync(stepContext.Context);
            var score = resultQnA.FirstOrDefault()?.Score;
            string response = resultQnA.FirstOrDefault()?.Answer;

            if (score >= 0.80)
            {
                await stepContext.Context.SendActivityAsync(response, cancellationToken: cancellationToken);
            }
            else
            {
                var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
                ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
                ClientData.request = "desborde";
                if(string.IsNullOrEmpty(ClientData.texto500))
                {
                    ClientData.TextoDesborde = stepContext.Context.Activity.Text;
                }
                else
                {
                    ClientData.TextoDesborde = ClientData.texto500;
                    ClientData.texto500 = null;
                }
                await stepContext.Context.SendActivityAsync($"Disculpa la información que me solicitas no la manejo aún, pero puedo tomar tu solicitud y enviarla al área encargada", cancellationToken: cancellationToken);
                return await stepContext.BeginDialogAsync(nameof(SolicitudesClientes), cancellationToken: cancellationToken);
            }
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);

        }


        private async Task<DialogTurnResult> IntentNegacion(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync($"Ha sido todo un placer atenderte.");
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }

        /*private async Task<DialogTurnResult> IntentNoEsUnJuego(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync($"Para brindarte un mejor servicio requiero que por favor ingreses la misma información con otras palabras y así entender en que deseas que te ayude");
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }*/

        private async Task<DialogTurnResult> IntentConsultaSaldo(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync($"Por este medio no te puedo brindar esta información, ingresa por favor a \"Mi Conexión Bancaribe\" para que veas todos los movimientos de tus cuentas, también te puedes comunicar al número 05002262274 o si te encuentras en el exterior marca +582129545777 por las opciones 2-1-1");
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> IntentConsultaSaldoTDC(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(ConsultaSaldoTDC), cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> IntentComoPagoTDC(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync($"Para realizar un pago o abono a tu tarjeta de crédito, debes poseer tu tarjeta conexión segura activa al igual que " +
                $"tu perfil de seguridad y ya debes poseer la tarjeta asociada al sistema. Si posees esta serie de requisitos tienes que ingresar simplemente a  " +
                $"Mi Conexión Bancaribe, persona natural, ingresa tus datos de acceso. Una vez dentro de tu cuenta, dirigirte a la sección de \"Pagos\", seleccionar " +
                $"si es \"Tarjeta de Crédito Propias\", \"de Terceros Bancaribe\" o \"de Otro Banco\" y elige a cual tarjeta deseas realizar el pago o abono.", cancellationToken: cancellationToken);
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> IntentRenovacionTarjeta(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            if (luisResult.Entities.tdd != null && luisResult.Entities.tdc == null) 
            {
                return await stepContext.BeginDialogAsync(nameof(RenovacionTDDDialog), cancellationToken: cancellationToken);
            }
            if (luisResult.Entities.tdc != null && luisResult.Entities.tdd == null)
            {
                ClientData.request = "renovacionTDC";
                return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
            }
            else
            {
                ClientData.request = "renovaciontjta";
                return await stepContext.BeginDialogAsync(nameof(TipoTarjetaDialog), cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> IntentReclamo(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            ClientData.request = "reclamoDomiciliacion";
            if (luisResult.Entities.domiciliacion != null )
            {
                return await stepContext.BeginDialogAsync(nameof(SolicitudesClientes), cancellationToken: cancellationToken);
            }
            if (luisResult.Entities.empleado != null || luisResult.Entities.maltrato != null)
            {
                return await stepContext.BeginDialogAsync(nameof(ReclamoEmpleado), cancellationToken: cancellationToken);
            }
            else
            {
                return await stepContext.BeginDialogAsync(nameof(AllClaims), cancellationToken: cancellationToken); 
            }
        }

        private async Task<DialogTurnResult> IntentReclamoNoAbonado(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            ClientData.request = "consultareclamo";

            if (luisResult.Entities.NroReclamo != null) 
            {
                ClientData.nroReclamo = luisResult.Entities.NroReclamo.ToString();

                //await stepContext.Context.SendActivityAsync("Un reclamo ya generado, tiene hasta un máximo de 20 días continuos para su resolución, tiempo establecido por el ente regulador SUDEBAN.", cancellationToken: cancellationToken);
                //await stepContext.Context.SendActivityAsync("Si ya transcurrieron los días correspondientes y aún no has recibido respuesta puedo tomar esta solicitud y enviarla al área encargada. ", cancellationToken: cancellationToken);

                return await stepContext.BeginDialogAsync(nameof(SolicitarDatosDialog), cancellationToken: cancellationToken);
            }
            else
            {
                return await stepContext.BeginDialogAsync(nameof(SolicitarDatosDialog), cancellationToken: cancellationToken);

            }
        }

        private async Task<DialogTurnResult> IntentTiempoPagoTDC(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync($"El plazo para que aparezca abonado el pago es de 24 horas.");
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }


        private async Task<DialogTurnResult> IntentRedes(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync($"Mi twitter y mi instagram es @bancaribe y en facebook me puedes ubicar como Bancaribe");
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> IntentDespedida(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());

            /*
            if (luisResult.Entities.navidad != null && luisResult.Entities.año != null) 
            { 
                await stepContext.Context.SendActivityAsync($"Igualmente, Feliz navidad y próspero año nuevo", cancellationToken: cancellationToken); 
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            if (luisResult.Entities.navidad != null)
            {
                await stepContext.Context.SendActivityAsync($"Igualmente, Feliz navidad", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            if ( luisResult.Entities.año != null)
            {
                await stepContext.Context.SendActivityAsync($"Igualmente, Feliz año nuevo", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            if (luisResult.Entities.fiestas != null)
            {
                await stepContext.Context.SendActivityAsync($"Igualmente, Felices Fiestas", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }*/

            if (luisResult.Entities.navidad != null && luisResult.Entities.año != null)
            {
                await stepContext.Context.SendActivityAsync($"Me alegra, ¿En que te puedo ayudar?", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }

            if (luisResult.Entities.gracias != null)
            {
                if (!ClientData.ClienteAtendido)
                {
                    await stepContext.Context.SendActivityAsync($"Me alegra, ¿En que te puedo ayudar?", cancellationToken: cancellationToken) ;
                }
                else
                {
                    await stepContext.Context.SendActivityAsync($"Gracias a ti, fue un placer ayudarte, que tengas" + utilitario.ValidateTime() , cancellationToken: cancellationToken);
                }
            }
            else if (luisResult.Entities.apurado != null)
            {
                await stepContext.Context.SendActivityAsync($"Fue un placer ayudarte, que tengas" + utilitario.ValidateTime() , cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.cuidate != null && luisResult.Entities.nosvemos != null)
            {
                await stepContext.Context.SendActivityAsync($"Gracias igualmente, fue un placer ayudarte, que tengas" + utilitario.ValidateTime() , cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.nosvemos != null)
            {
                await stepContext.Context.SendActivityAsync($"Gracias, fue un placer ayudarte, que tengas" + utilitario.ValidateTime() , cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.TeVeo != null)
            {
                await stepContext.Context.SendActivityAsync($"Igualmente, fue un placer ayudarte", cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.hablamos != null)
            {
                await stepContext.Context.SendActivityAsync($"Hasta pronto, fue un placer ayudarte,  que tengas" + utilitario.ValidateTime() , cancellationToken: cancellationToken);
            }
            else 
            {
                await stepContext.Context.SendActivityAsync($"Hasta pronto, fue un placer ayudarte, que tengas" + utilitario.ValidateTime() , cancellationToken: cancellationToken);
            }
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> IntentAfiliacionPagoMovil(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            if ((luisResult.Entities.pagorapido != null || luisResult.Entities.sms != null || luisResult.Entities.operar != null) && luisResult.Entities.Smartphone is null)
            {
                return await stepContext.BeginDialogAsync(nameof(AfiliarmeMiPago), cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.Smartphone != null)
            {
                await stepContext.Context.SendActivityAsync("Descarga la aplicación \"Mi Pago Bancaribe\", tilda el icono que te indica \"Afiliación\", completa los datos de registro y confirma los datos pulsando la opción \"Aceptar\"", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else
            {
                return await stepContext.BeginDialogAsync(nameof(AfiliacionMiPagoBancaribe), cancellationToken: cancellationToken);
            }
        }


        private async Task<DialogTurnResult> IntentPensionado(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            stepContext.Context.Activity.Text = stepContext.Context.Activity.Text.ToLower();
            var resultQnA = await _qnAMakerAIService._qnAMakerResult.GetAnswersAsync(stepContext.Context);
            string response = resultQnA.FirstOrDefault()?.Answer;
            if (String.IsNullOrEmpty(response))
            {
                await stepContext.Context.SendActivityAsync("¿En que te puedo ayudar?", cancellationToken: cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(response, cancellationToken: cancellationToken);
            }
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> IntentMontosMaximos(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            if (luisResult.Entities.transferir != null && luisResult.Entities.sms is null && luisResult.Entities.pagorapido is null && luisResult.Entities.mipagobancaribe is null)
            { //MontoMaximoTransferencia
                if (luisResult.Entities.juridico != null)//Persona Jurídica
                {
                    await stepContext.Context.SendActivityAsync($"Hasta la fecha el Máximo Diario para  transferencia es de:{ Environment.NewLine} " +
                    $"📌 Transferencia Propias a Bancaribe: no existe límite de monto {Environment.NewLine}" +
                    $"📌 Transferencia Terceros Bancaribe: no existe límite de monto { Environment.NewLine}" +
                    $"📌 Transferencia a otros Bancos: 7.000.000.000,00BsS", cancellationToken: cancellationToken);
                    return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
                }
                
                else if (luisResult.Entities.natural != null) //Persona Natural
                {
                    await stepContext.Context.SendActivityAsync($"Hasta la fecha el Máximo Diario para  transferencia es de:{ Environment.NewLine} " +
                    $"📌 Transferencia Propias a Bancaribe:  no existe límite de monto {Environment.NewLine}" +
                    $"📌 Transferencia Terceros Bancaribe:  no existe límite de monto {Environment.NewLine}" +
                    $"📌 Transferencia a otros Bancos: 1.000.000.000,00BsS, ten en cuenta que por motivos de seguridad al transferir a otros bancos el sistema no dejará transferir el máximo, te sugiero realizar tus transferencias de manera fraccionadas, c/u de  500.000.000,00 Bs o montos menores", cancellationToken: cancellationToken);
                    return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
                }
                else
                    return await stepContext.BeginDialogAsync(nameof(MontoMaximoTransferencia), cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.sms != null || (luisResult.Entities.pagorapido != null && luisResult.Entities.pos is null) || (luisResult.Entities.mipagobancaribe != null && luisResult.Entities.pos is null))
            { //MontoMaximoPagoMovil
                await stepContext.Context.SendActivityAsync($"Hasta la fecha el Máximo Diario para transferencia por Mi Pago Bancaribe es de: 700.000.000,00BsS", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.cajero != null)
            { //MontoMaximoRetiroEnCajero
                await stepContext.Context.SendActivityAsync($"Hasta la fecha el Máximo Diario para  Retirar en Cajero Bancaribe es de: 30.000,00BsS, si deseas retirar por nuestros cajeros Bancaribe con una Tarjeta de Debito de otra entidad Bancaria es monto Máximo Diario es de : 5.000,00BsS", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.tdc != null)
            { //MontoMaximoPagoConTDC
                await stepContext.Context.SendActivityAsync($"En la Tarjeta de Crédito varia el disponible que se puede usar por Punto de Venta, eso dependerá del Límite que posea tu Tarjeta. ", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.tdd != null)
            { //MontoMaximoPagoConTDD
                await stepContext.Context.SendActivityAsync($"Hasta la fecha el Máximo Diario para hacer uso de su Tarjeta de Débito por Punto de Venta es: 500.000.000,00BsS", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.tarjeta != null || luisResult.Entities.pos != null)
            { //MontoMaximoPagoConTarjetaNoEspecificada
                return await stepContext.BeginDialogAsync(nameof(MontoMaximoTarjeta), cancellationToken: cancellationToken);
            }
            else
            { //Cuál Límite desea Conocer?
                return await stepContext.BeginDialogAsync(nameof(MontoMaximoGeneral), cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> IntentPagoMovilSMS(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            if (luisResult.Entities.pagorapido != null && luisResult.Entities.sms is null  &&
                 luisResult.Entities.tlfbasico is null && luisResult.Entities.escribirsms is null
                 && luisResult.Entities.problema is null && luisResult.Entities.nopuedo is null)
            { //DineroRapido
                return await stepContext.BeginDialogAsync(nameof(PagoRapidoDialog), cancellationToken: cancellationToken);
            }
            else if ( luisResult.Entities.sms != null && luisResult.Entities.escribirsms is null &&
                luisResult.Entities.problema is null && luisResult.Entities.nopuedo is null)
            { //PagoPorSMS
                return await stepContext.BeginDialogAsync(nameof(PagoSMSDialog), cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.tlfbasico != null && luisResult.Entities.problema is null 
                && luisResult.Entities.nopuedo is null)
            { //TLFSimple
                return await stepContext.BeginDialogAsync(nameof(TlfSimpleDialog), cancellationToken: cancellationToken);
            }
            else if ((luisResult.Entities.escribirsms != null && luisResult.Entities.problema is null 
                && luisResult.Entities.nopuedo is null) || (luisResult.Entities.sms != null && luisResult.Entities.problema is null
                && luisResult.Entities.nopuedo is null)) 
            { //PagoPorSMS
                await stepContext.Context.SendActivityAsync($"En el cuerpo del mensaje escribirás en este orden: Mipago + Cédula del beneficiario(Incluyendo V, E o P) + Los 4 primeros dígitos de la cuenta del beneficiario + Monto del pago(con sus centimos) + Número de teléfono del beneficiario. Por último envía el SMS al número(22741)" +
                    $"{Environment.NewLine}Ejemplo: " +
                    $"{Environment.NewLine}Mipago V12345678 0114 5000,00 04141234567", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }

            else if (luisResult.Entities.problema != null || luisResult.Entities.nopuedo != null)
            { //Problemas con el Pago Movil 
                return await stepContext.BeginDialogAsync(nameof(ProblemasEnvioMiPagoDialog), cancellationToken: cancellationToken);
            }
            else 
            { //PagoMovil
                await stepContext.Context.SendActivityAsync($"Para realizar un Pago Bancaribe tienes que estar ya afiliado a este servicio, ya afiliado puedes hacer realizar el Pago por la Aplicación \"Mi pago Bancaribe\" o por SMS ", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
        }

        
        private async Task<DialogTurnResult> IntentGestionEnLinea(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            ClientData.request = "desborde";
            if (string.IsNullOrEmpty(ClientData.texto500))
            {
                ClientData.TextoDesborde = stepContext.Context.Activity.Text;
            }
            else
            {
                ClientData.TextoDesborde = ClientData.texto500;
                ClientData.texto500 = null;
            }
            await stepContext.Context.SendActivityAsync($"Disculpa la información que me solicitas no la manejo aún pero puedo tomar tu solicitud y enviarla al área encargada", cancellationToken: cancellationToken);
            return await stepContext.BeginDialogAsync(nameof(SolicitudesClientes), cancellationToken: cancellationToken);
            
        }
        

        private async Task<DialogTurnResult> IntentTiempoTransferencia(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            stepContext.Context.Activity.Text = stepContext.Context.Activity.Text.ToLower();
            var resultQnA = await _qnAMakerAIService._qnAMakerResult.GetAnswersAsync(stepContext.Context);
            string response = resultQnA.FirstOrDefault()?.Answer;
            if (String.IsNullOrEmpty(response))
            {
                return await stepContext.BeginDialogAsync(nameof(FechaTransferenciaReclamo), cancellationToken: cancellationToken);
            }
            await stepContext.Context.SendActivityAsync(response, cancellationToken: cancellationToken);
            return await stepContext.ContinueDialogAsync(cancellationToken);
        }


        private async Task<DialogTurnResult> IntentTransferencia(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            if (luisResult.Entities.transferir != null && luisResult.Entities.ahorroMalEscrito is null
                 && luisResult.Entities.corrienteMalEscrito is null && luisResult.Entities.TipoCtaBienEscrito is null)
            {//verificar Transferencia o Problemas para Transferir
                return await stepContext.BeginDialogAsync(nameof(TransferenciaDialog), cancellationToken: cancellationToken);
            }
            else if ((luisResult.Entities.transferir != null && luisResult.Entities.ahorroMalEscrito != null) ||
                (luisResult.Entities.transferir != null && luisResult.Entities.corrienteMalEscrito != null))
            {//verificar cuenta
                string textoaux = "";

                if (luisResult.Entities.ahorroMalEscrito !=null)
                {
                    textoaux = "Cuenta de Ahorro?";
                }
                else
                {
                    textoaux = "Cuenta de Corriente?";
                }

                await stepContext.Context.SendActivityAsync($"Me quieres decir ¿Qué deseas realizar una transferencia de tu " + textoaux, cancellationToken: cancellationToken);
                return await stepContext.BeginDialogAsync(nameof(TransfDialog), cancellationToken: cancellationToken);
            }
            else
            { 
                await stepContext.Context.SendActivityAsync($"Para realizar una Transferencia, debes poseer tu Tarjeta Conexión Segura Activa al igual que tu Perfil de Seguridad y ya debes poseer la cuenta afiliada. Si posees esta serie de requisitos tienes que ingresar simplemente a  Mi conexión Bancaribe, persona natural, ingresa tus datos de acceso. Una vez dentro de tu cuenta, dirigirte a la sección de \"Transferencias\", selecciona si es \"Cuenta Propia\", a \"Terceros Bancaribe\" o \"Otro Banco\" y elige a que cuentas deseas realizar la transferencia. ", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
        }

        /*
        private async Task<DialogTurnResult> IntentTarjetaBloqueada(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            if(luisResult.Entities.tdc != null && luisResult.Entities.Bloqueo != null && luisResult.Entities.Desbloqueo is null && luisResult.Entities.Bloqueado is null)
            {
                ClientData.TextoDesborde = stepContext.Context.Activity.Text;
                return await stepContext.BeginDialogAsync(nameof(BloquearDesbloquearTjtaDialog), cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.tdc != null && luisResult.Entities.tdd is null && luisResult.Entities.tarjetasegura is null)
            {//Tarjeta de Crédito
                ClientData.request = "tdcbloqueada";
                return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.tdd != null && luisResult.Entities.tdc is null && luisResult.Entities.tarjetasegura is null)
            {// Tarjeta de Débito
                await stepContext.Context.SendActivityAsync($"Si presentas un bloqueo, accede a este servicio, es muy sencillo. Sólo necesitas tu Tarjeta de Débito y llamar al 0500-Bancaribe (0500-2262274) o, si te encuentras en el exterior, " +
                    $"al 58-212-9545777. Vas a escuchar atentamente al sistema automatizado y vas a marcar la opción correspondiente si es Persona Natural(opción 2) o Jurídica(opción 3) y el sistema te indicara paso a paso como desbloquearla " +
                    $"para que puedas seguir disfrutando de nuestros servicios." , cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.tarjetasegura != null && luisResult.Entities.tdc is null && luisResult.Entities.tdd is null)
            {// Tarjeta Segura
                await stepContext.Context.SendActivityAsync($"La tarjeta de Conexión Segura o Coordenadas no se bloquea, ni se suspende. Si el sistema le indica algún problema al momento " +
                    $"de transferir y ésta se encuentra activa, lo más probable es que el inconveniente lo presenta el Perfil de Seguridad", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else
            {
                return await stepContext.BeginDialogAsync(nameof(TarjetaBloqueadaDialog), cancellationToken: cancellationToken);
            }
        }*/
        

        private async Task<DialogTurnResult> IntentClienteNuevo(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            if(luisResult.Entities.usuario != null && luisResult.Entities.clave != null && luisResult.Entities.Afiliacion != null)
            {
                return await stepContext.BeginDialogAsync(nameof(ClienteNuevoDialog), cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.usuario != null || (luisResult.Entities.opcion1List != null 
                && !luisResult.Entities.opcion1List.ToString().Equals("a") && luisResult.Entities.clave != null) || 
                (luisResult.Entities.opcion1List != null && !luisResult.Entities.opcion1List.ToString().Equals("a") 
                && luisResult.Entities.OnlineBanking != null) || luisResult.Entities.nuevo != null
                )
            {
                await stepContext.Context.SendActivityAsync($"Si no posees un usuario ni una contraseña para el ingreso a Mi conexión Bancaribe, te sugiero que ingreses " +
                    $"por las opciones Persona Natural y Cliente Nuevo. El sistema te solicitara los datos de tu tarjeta de débito (estado activa) " +
                    $"y algunos de tus datos personales, así podrás acceder sin problema", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else
            {
                return await stepContext.BeginDialogAsync(nameof(ClienteNuevoDialog), cancellationToken: cancellationToken);
            }
        }


        private async Task<DialogTurnResult> IntentCartaSerial(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());

            if (luisResult.Entities.solicitud!= null && luisResult.Entities.Carta != null)
            {
                ClientData.request = "solicitudcartaserial";
                return await stepContext.BeginDialogAsync(nameof(SolicitudesClientesEmpresas), cancellationToken: cancellationToken);
            }
            else if ((luisResult.Entities.Carta != null || luisResult.Entities.Afiliacion != null || luisResult.Entities.usuario != null)
                && luisResult.Entities.nopuedo is null )
            {
                return await stepContext.BeginDialogAsync(nameof(CartaSerialDialog), cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.nopuedo != null || (luisResult.Entities.problema != null))
            {
                await stepContext.Context.SendActivityAsync($"Si posees una Carta Serial, pero el sistema no te lo toma, te recomiendo validar si tu usuario para el ingreso no presenta algún tipo de bloqueo o asegúrate de que la Carta Serial que posees sea la actual y no una anterior.", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else
            {
                return await stepContext.BeginDialogAsync(nameof(CartaSerialDialog), cancellationToken: cancellationToken);
            }
        }


        private async Task<DialogTurnResult> IntentPerfilSeguridad(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            if(luisResult.Entities.bancaribedigital != null)
            {
                stepContext.Context.Activity.Text = stepContext.Context.Activity.Text.ToLower();
                var resultQnA = await _qnAMakerAIService._qnAMakerResult.GetAnswersAsync(stepContext.Context);
                var score = resultQnA.FirstOrDefault()?.Score;
                string response = resultQnA.FirstOrDefault()?.Answer;

                if (score >= 0.80)
                {
                    await stepContext.Context.SendActivityAsync(response, cancellationToken: cancellationToken);
                    return await stepContext.ContinueDialogAsync(cancellationToken);
                }
                else
                {
                    ClientData.request = "perfilseguridad";
                    return await stepContext.BeginDialogAsync(nameof(PerfilSeguridadDialog), cancellationToken: cancellationToken);
                }

            }
            if ((luisResult.Entities.Bloqueado != null || luisResult.Entities.Bloqueo != null) && luisResult.Entities.perfil != null && luisResult.Entities.natural != null
                && luisResult.Entities.porque is null)
            {
                await stepContext.Context.SendActivityAsync($"Para desbloquear tu perfil de seguridad, solo ingresa a Mi Conexión Bancaribe, persona natural, accede con tu login y contraseña. Una vez dentro de tu cuenta tilda las opciones Servicio al Cliente, Administración de Seguridad y ¿Se Bloqueó su Perfil de Seguridad? recuerda que para realizar esta transacción debe conocer las respuestas de seguridad y poseer la tarjeta conexión segura", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else if ((luisResult.Entities.Bloqueado != null || luisResult.Entities.Bloqueo != null) && luisResult.Entities.perfil != null && luisResult.Entities.juridico != null
                && luisResult.Entities.porque is null)
            {
                await stepContext.Context.SendActivityAsync($"Te recuerdo que el usuario jurídico no posee perfil de seguridad, sin embargo, " +
                    $"si ingresas más de 3 veces algún dato errado al momento de realizar alguna transacción, el sistema por precaución no te " +
                    $"permitirá realizar operaciones.", cancellationToken: cancellationToken);
                ClientData.request = "solicitudcartaserial";
                return await stepContext.BeginDialogAsync(nameof(SolicitudesClientesEmpresas), cancellationToken: cancellationToken);
            }
            else if ((luisResult.Entities.Bloqueado != null || luisResult.Entities.Bloqueo != null) && luisResult.Entities.perfil != null
                && luisResult.Entities.porque is null)
            {
                ClientData.request = "perfilseguridad";
                return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
            }
            else if ((luisResult.Entities.Bloqueado != null || luisResult.Entities.Bloqueo != null) && luisResult.Entities.tarjetasegura != null
                && luisResult.Entities.porque is null)
            {
                ClientData.request = "Segura";
                await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken); 
                return await stepContext.BeginDialogAsync(nameof(BloquearDesbloquearTjtaDialog), cancellationToken: cancellationToken);
            }
            else if ((luisResult.Entities.Bloqueado != null || luisResult.Entities.Bloqueo != null) && luisResult.Entities.perfil is null
                        && (luisResult.Entities.operar != null || luisResult.Entities.transferir != null)
                && luisResult.Entities.porque is null)
            {
                ClientData.request = "perfilseguridad";
                return await stepContext.BeginDialogAsync(nameof(PerfilSeguridadDialog), cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.suspendido!= null  && luisResult.Entities.perfil is null
                        &&  luisResult.Entities.tarjetasegura != null)
            {
                ClientData.request = "perfilseguridadsusp";
                return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
            }

            else if ((luisResult.Entities.porque is null && (luisResult.Entities.nopuedo != null || luisResult.Entities.disponible != null)
                && luisResult.Entities.Bloqueado is null && luisResult.Entities.Bloqueo is null
                && luisResult.Entities.tarjeta is null && luisResult.Entities.perfil is null) ||
                luisResult.Entities.disponible != null && luisResult.Entities.porque is null
                && luisResult.Entities.Bloqueado is null && luisResult.Entities.Bloqueo is null
                && luisResult.Entities.tarjeta is null && luisResult.Entities.perfil is null)
            {
                await stepContext.Context.SendActivityAsync($"Asegúrate de poseer una tarjeta de conexión segura activa. De poseerla y aun no poder hacer transacciones, te recomiendo validar si tu perfil de seguridad se encuentra Bloqueado o Suspendido.", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }

            else if ((luisResult.Entities.Bloqueado != null || luisResult.Entities.Bloqueo != null) && luisResult.Entities.perfil != null && luisResult.Entities.porque != null)
            {
                await stepContext.Context.SendActivityAsync($"Se bloquea debido a que ingresan una o varias coordenadas de manera errada o al colocar la respuesta de la pregunta de seguridad de forma incorrecta.", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.suspendido != null && luisResult.Entities.perfil != null && luisResult.Entities.natural != null
                && luisResult.Entities.porque is null)
            {
                ClientData.request = "perfilseguridadsusp";
                return await stepContext.BeginDialogAsync(nameof(SolicitudesClientes), cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.suspendido != null && luisResult.Entities.perfil != null && luisResult.Entities.juridico != null
                && luisResult.Entities.porque is null)
            {
                await stepContext.Context.SendActivityAsync($"Te recuerdo que el usuario jurídico no posee perfil de seguridad, sin embargo, " +
                    $"si ingresas más de 3 veces algún dato errado al momento de realizar alguna transacción, el sistema por precaución no te " +
                    $"permitirá realizar operaciones.", cancellationToken: cancellationToken);
                ClientData.request = "perfilseguridad";
                return await stepContext.BeginDialogAsync(nameof(SolicitudesClientesEmpresas), cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.suspendido != null && luisResult.Entities.transferir != null && luisResult.Entities.natural is null
                && luisResult.Entities.juridico is null)
            {
                ClientData.request = "perfilseguridadsusp";
                return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.suspendido != null && luisResult.Entities.transferir != null && luisResult.Entities.juridico is null
                && luisResult.Entities.natural != null)
            {
                ClientData.request = "perfilseguridadsusp";
                return await stepContext.BeginDialogAsync(nameof(SolicitudesClientes), cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.suspendido != null && luisResult.Entities.transferir != null && luisResult.Entities.juridico != null
                && luisResult.Entities.natural is null)
            {
                await stepContext.Context.SendActivityAsync($"Te recuerdo que el usuario jurídico no posee perfil de seguridad, sin embargo, " +
                       $"si ingresas más de 3 veces algún dato errado al momento de realizar alguna transacción, el sistema por precaución no te " +
                       $"permitirá realizar operaciones.", cancellationToken: cancellationToken);
                ClientData.request = "perfilseguridadsusp";
                return await stepContext.BeginDialogAsync(nameof(SolicitudesClientesEmpresas), cancellationToken: cancellationToken);
            }

            else if ((luisResult.Entities.suspendido != null && luisResult.Entities.perfil != null
                && luisResult.Entities.porque is null)
                || (luisResult.Entities.suspendido != null && luisResult.Entities.OnlineBanking != null
                && luisResult.Entities.porque is null))
            {
                ClientData.request = "perfilseguridadsusp";
                return await stepContext.BeginDialogAsync(nameof(PerfilSeguridadDialog), cancellationToken: cancellationToken);
            }

            else if (((luisResult.Entities.nopuedo != null && luisResult.Entities.transferir != null)
                || (luisResult.Entities.nopuedo != null && luisResult.Entities.disponible != null)
                || (luisResult.Entities.nopuedo != null && luisResult.Entities.OnlineBanking != null))
                && luisResult.Entities.porque is null)
            {
                await stepContext.Context.SendActivityAsync($"Asegúrate de poseer una tarjeta de conexión segura activa. De poseerla y aun no poder hacer transacciones, te recomiendo validar si tu perfil de seguridad se encuentra Bloqueado o Suspendido.", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }

            else if (luisResult.Entities.porque != null && luisResult.Entities.suspendido != null)
            {
                await stepContext.Context.SendActivityAsync($"Cuando ingresas algún dato errado, ya sean datos de la Tarjeta Conexión Segura \"Coordenadas\" o la respuesta de la pregunta de seguridad, el perfil queda bloqueado por precaución y si continúan ingresando los datos errados ya pasa a estar suspendido", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }

            else
            {
                ClientData.request = "perfilseguridad";
                return await stepContext.BeginDialogAsync(nameof(PerfilSeguridadDialog), cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> IntentTarjetaConexionSegura(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());

            if (((luisResult.Entities.Afiliacion != null  && luisResult.Entities.tarjetasegura != null) ||
                luisResult.Entities.solicitud != null && luisResult.Entities.tarjetasegura != null
                ) && luisResult.Entities.activar is null)
            {
                await stepContext.Context.SendActivityAsync($"Puedes emitirla,  ingresando a la opción, persona natural, ingresa tu login y contraseña. Una vez dentro de tu cuenta " +
                    $"tilda las opciones servicio al cliente, administración de seguridad, autogestión De tarjeta conexión segura " +
                    $"y por ultimo emisión o reposición de tarjeta conexión segura", cancellationToken: cancellationToken);
                await stepContext.Context.SendActivityAsync($"Recuerda que para realizar esta transacción debe conocer las Respuestas de Seguridad y una vez obtenida, tienes un plazo de " +
                    $"24 horas para ser activas comunicándose al 0500-Bancaribe (0500-2262274) por las opciones 2 / 6 / 3", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.activar != null && luisResult.Entities.tarjetasegura != null && luisResult.Entities.natural != null)
            {
                await stepContext.Context.SendActivityAsync($"Una vez que solicites la tarjeta conexión segura, tienes un plazo de 24 horas para realizar la activación, te comunicas al " +
                    $" 0500-Bancaribe (0500-2262274) por las opciones 2 / 6 / 3. Debes tener a la mano la información de tu tarjeta de débito y poseer la numeración Serial que se " +
                    $"encuentra en la tarjeta conexión segura ya emitida y así realizar la activación", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.activar != null && luisResult.Entities.tarjetasegura != null && luisResult.Entities.juridico != null)
            {
                await stepContext.Context.SendActivityAsync($"Recuerda que tu tarjeta de conexión segura jurídica no requiere que sea activada, desde que la emites a través de la página " +
                    $"puede realizar operaciones con ella. ", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.activar != null && luisResult.Entities.tarjetasegura != null )
            {
                ClientData.request = "TjtaSegura";
                return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.vencida != null && luisResult.Entities.tarjetasegura != null)
            {
                await stepContext.Context.SendActivityAsync($"Te indico que cuando vence tu Tarjeta de Conexión Segura, ¡Puedes auto gestionarte! Ingresa a Mi Conexión Bancaribe Jurídica " +
                    $"el sistema después de que te solicite las coordenadas para ingresar, te arrojara un mensaje que indica cómo puedes obtener la reposición o si no puedes realizarlo " +
                    $"de este modo, también puedes ingresar por la opción \"Recuperar tarjeta Conexión Segura\" ", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else
            {
                ClientData.request = "TjtaSegura";
                return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> IntentNoVeTDC(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            await stepContext.Context.SendActivityAsync($"Entiendo, estare realizandote algunas preguntas para obtener la información que debo enviar al área encargada", cancellationToken: cancellationToken);
            
            if (luisResult.Entities.juridico != null)
            {
                ClientData.request = "novetdcjuridico";
            }
            
            else if (luisResult.Entities.natural != null)
            {
                ClientData.request = "novetdcnatural";
            }
            
            else
            {
                ClientData.request = "novetdc";
            }
            return await stepContext.BeginDialogAsync(nameof(NoVeTDCDialog), cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> IntentTipoTarjeta(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            if (luisResult.Entities.tdc != null)
            {
                await stepContext.Context.SendActivityAsync($"Por favor dame más información ¿en que requieres que te ayude relacionado con tarjetas de crédito?", cancellationToken: cancellationToken);
            }
            else if(luisResult.Entities.tdd != null)
            {
                await stepContext.Context.SendActivityAsync($"Por favor dame más información ¿en que requieres que te ayude relacionado con tarjetas de débito?", cancellationToken: cancellationToken);

            }
            else
            {
                await stepContext.Context.SendActivityAsync($"Por favor dame más información ¿en que requieres que te ayude relacionado con tarjetas?", cancellationToken: cancellationToken);

            }
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);

        }
        private async Task<DialogTurnResult> IntentTipoPersona(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync($"Por favor dame más información para poder ayudarte", cancellationToken: cancellationToken);
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);

        }


        private async Task<DialogTurnResult> IntentTDCSaldoCero(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            if (luisResult.Entities.QueEs != null && luisResult.Entities.CartaFiniquito != null )
            {
                await stepContext.Context.SendActivityAsync($"Es un documento que solicitas para cancelar tu tarjeta de crédito y así hacer constancia de que alguna vez poseías una.", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.QueEs != null && luisResult.Entities.SaldoCero != null)
            {
                await stepContext.Context.SendActivityAsync($"Es un documento que solicitas al banco para comprobar que estas exento de cualquier deuda de tu tarjeta de crédito.", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.CartaFiniquito != null)
            {
                ClientData.request = "finiquito"; 
                await stepContext.Context.SendActivityAsync($"Entiendo, estare realizandote algunas preguntas para obtener la información que debo enviar al área encargada", cancellationToken: cancellationToken);
                return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
            }
            else
            {
                ClientData.request = "saldocero";
                await stepContext.Context.SendActivityAsync($"Entiendo, estare realizandote algunas preguntas para obtener la información que debo enviar al área encargada", cancellationToken: cancellationToken);
                return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
            }
        }


        private async Task<DialogTurnResult> IntentBloquearDesbloquearTarjeta(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());

            //Tipo de tarjeta
            if (luisResult.Entities.tdc != null && luisResult.Entities.natural != null)
            {
                ClientData.request = "NaturalTDC";
            }
            else if (luisResult.Entities.tdc != null && luisResult.Entities.juridico != null)
            {
                ClientData.request = "JuridicoTDC";
            }
            else if(luisResult.Entities.tdd != null && luisResult.Entities.natural != null)
            {
                ClientData.request = "NaturalTDD";
            }
            else if (luisResult.Entities.tdd != null && luisResult.Entities.juridico != null)
            {
                ClientData.request = "JuridicoTDD";
            }
            else if (luisResult.Entities.tarjetasegura != null && luisResult.Entities.natural != null)
            {
                ClientData.request = "NaturalSegura";
            }
            else if (luisResult.Entities.tarjetasegura != null && luisResult.Entities.juridico != null)
            {
                ClientData.request = "JuridicoSegura";
            }
            else if (luisResult.Entities.tdc != null)
            {
                ClientData.request = "TDC";
            }
            else if (luisResult.Entities.tdd != null )
            {
                ClientData.request = "TDD";
            } 
            else if ( luisResult.Entities.tarjetasegura != null)
            {
                ClientData.request = "Segura";
            }
            else if ( luisResult.Entities.natural != null)
            {
                ClientData.request = "Natural"; 
                  return await stepContext.BeginDialogAsync(nameof(TarjetaBloqueadaDialog), cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.juridico != null)
            {
                ClientData.request = "Juridico";
                return await stepContext.BeginDialogAsync(nameof(TarjetaBloqueadaDialog), cancellationToken: cancellationToken);
            }
            else
            {
                return await stepContext.BeginDialogAsync(nameof(TarjetaBloqueadaDialog), cancellationToken: cancellationToken);
            }


            //Tipo de Persona
            if (ClientData.request.Contains("Natural") || ClientData.request.Contains("Juridico"))
            {
                return await stepContext.BeginDialogAsync(nameof(BloquearDesbloquearTjtaDialog), cancellationToken: cancellationToken);
            }
            else
            {
                return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
            }


            /*

            if (luisResult.Entities.Desbloqueo != null && luisResult.Entities.tdc != null)
            {//Debloqueo Tarjeta de Crédito
                ClientData.TextoDesborde = stepContext.Context.Activity.Text;
                ClientData.request = "tdcbloqueada";
                return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.Desbloqueo != null && luisResult.Entities.tdd != null)
            {//Desbloqueo Tarjeta de Débito
                await stepContext.Context.SendActivityAsync($"Si presentas un bloqueo, accede a este servicio, es muy sencillo. Sólo necesitas tu Tarjeta de Débito y llamar al 0500-Bancaribe (0500-2262274) o, si te encuentras en el exterior, " +
                    $"al 58-212-9545777. Vas a escuchar atentamente al sistema automatizado y vas a marcar la opción correspondiente si es Persona Natural(opción 2) o Jurídica(opción 3) y el sistema te indicara paso a paso como desbloquearla " +
                    $"para que puedas seguir disfrutando de nuestros servicios.", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.Bloqueo != null && luisResult.Entities.tdd != null)
            {//Bloqueo Tarjeta de débito
                await stepContext.Context.SendActivityAsync($"Si presentas un bloqueo, accede a este servicio, es muy sencillo. Sólo necesitas tu Tarjeta de Débito y llamar al 0500-Bancaribe (0500-2262274) o, si te encuentras en el exterior, " +
                    $"al 58-212-9545777. Vas a escuchar atentamente al sistema automatizado y vas a marcar la opción correspondiente si es Persona Natural(opción 2) o Jurídica(opción 3) y el sistema te indicara paso a paso como desbloquearla " +
                    $"para que puedas seguir disfrutando de nuestros servicios.", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.Desbloqueo != null)
            {//Desbloqueo Tarjeta
                return await stepContext.BeginDialogAsync(nameof(TarjetaBloqueadaDialog), cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.Robo != null)
            {//Bloqueo Tarjeta
                ClientData.request = "robotdc";
                await stepContext.Context.SendActivityAsync($"Lamento leer eso, con  mucho gusto te ayudo, voy a solicitarte algunos datos", cancellationToken: cancellationToken);
                return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.Extravio != null)
            {//Bloqueo Tarjeta
                ClientData.request = "perdidatdc";
                await stepContext.Context.SendActivityAsync($"Que mal, con  mucho gusto te ayudo, voy a solicitarte algunos datos", cancellationToken: cancellationToken);
                return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.Bloqueo != null || luisResult.Entities.SuspendeElimina != null)
            {//Bloqueo Tarjeta
                ClientData.request = "bloqueotdc";
                await stepContext.Context.SendActivityAsync($"Con  mucho gusto te ayudo, voy a solicitarte algunos datos", cancellationToken: cancellationToken);
                return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
            }
            else 
            {
                await stepContext.Context.SendActivityAsync($"Con  mucho gusto te ayudo, voy a solicitarte algunos datos", cancellationToken: cancellationToken);
                return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
            }
            */
        }


        private async Task<DialogTurnResult> IntentActivacionTDC(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync($"Debes comunicarte por los números 0500-2262274, 0212-9545777 y marcar las opciones 2 persona natural o 3 persona jurídica según corresponda.", cancellationToken : cancellationToken);
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> IntentAfirmacion( WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync($"Si hay algo en lo que te pueda ayudar solo me lo debes indicar y con gusto te ayudaré");
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }
        private async Task<DialogTurnResult> IntentInfoPtoVenta(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync($"Para cualquier gestión con punto de venta te recomendamos leer la información de este [enlace](https://www.bancaribe.com.ve/zona-de-informacion-para-cada-mercado/empresas/punto-de-venta-empresa#tabs-3493-0-0), " +
                $"ahí podrás ver requisitos, características entre otros detalles.");
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }


        private async Task<DialogTurnResult> IntentAgencias(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            if (luisResult.Entities.saber != null && luisResult.Entities.cercana is null && luisResult.Entities.Agencias is null)
            {
                await stepContext.Context.SendActivityAsync($"¿Qué tipo de información quieres saber?", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.cercana != null && luisResult.Entities.Agencias is null)
            {
                await stepContext.Context.SendActivityAsync($"¿En donde te ubicas?, para ayudarte", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.numero != null && luisResult.Entities.Agencias is null)
            {
                await stepContext.Context.SendActivityAsync($"! Claro ¡ ¿De que agencia ?", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else if ( luisResult.Entities.Agencias != null)
            {
                stepContext.Context.Activity.Text = stepContext.Context.Activity.Text.ToLower();
                var resultQnA = await _qnAMakerAIService._qnAMakerResult.GetAnswersAsync(stepContext.Context);
                var score = resultQnA.FirstOrDefault()?.Score;
                string response = resultQnA.FirstOrDefault()?.Answer;
                if(String.IsNullOrEmpty(response))
                {
                    await stepContext.Context.SendActivityAsync($"Disculpa no he podido encontrar una agencia con la información que me has proporcionado, " +
                     $"de igual manera te informo que estos momentos no todas nuestras agencias se encuentran prestando servicio, siguiendo las instrucciones " +
                     $"de la SUDEBAN. Para saber cuales están operativas te invito a que busques esta información en nuestras " +
                     $"redes sociales Instagram y Twitter @Bancaribe.", cancellationToken: cancellationToken);
                    return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
                }
                else
                {

                    await stepContext.Context.SendActivityAsync(response, cancellationToken: cancellationToken);
                    await stepContext.Context.SendActivityAsync($"En estos momentos no todas nuestras agencias se encuentran prestando servicio, siguiendo las instrucciones " +
                     $"de la SUDEBAN. Para saber cuales están operativas te invito a que busques esta información en nuestras " +
                     $"redes sociales Instagram y Twitter @Bancaribe.", cancellationToken: cancellationToken);
                    return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
                }
            }
            else
            {
                await stepContext.Context.SendActivityAsync($"En estos momentos no todas nuestras agencias se encuentran prestando servicio, siguiendo las instrucciones " +
                     $"de la SUDEBAN. Para saber cuales están operativas te invito a que busques esta información en nuestras " +
                     $"redes sociales Instagram y Twitter @Bancaribe.", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            
        }
        private async Task<DialogTurnResult> IntentAumentoLimiteTDC(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync($"Debes ingresar al link Mi gestión en línea seguir la ruta: {Environment.NewLine}" +
                $"Personas/Aumento de límite de Tarjetas de créditos se le va a desplegar la información de Recaudos y Requisitos. {Environment.NewLine}" +
                $"Para proceder a la solicitud debe completar todos los datos, y seleccionar \"Nueva solicitud\".");
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> IntentSaludar(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            if (luisResult.Entities.pensionado != null)
            {
                await stepContext.Context.SendActivityAsync($"Si deseas una atención especial para tu gestión sobre tu Cuenta de Pensión comunícate al número 0212 - 5055100.", cancellationToken: cancellationToken);
            }
            if (luisResult.Entities.buendia != null || luisResult.Entities.buenatarde != null || luisResult.Entities.buenasnoches != null)
            {
                if (!ClientData.ClienteAtendido)
                {
                    if (luisResult.Entities.comoestas != null || luisResult.Entities.quetalestas != null || luisResult.Entities.quetalteva != null)
                    {
                        if (luisResult.Entities.buendia != null && luisResult.Entities.comoestas != null)
                        {
                            if (ClientData.Saludo1 != true)
                            {
                                await stepContext.Context.SendActivityAsync($"Buenos Días, Muy bien, ¿y tú?", cancellationToken: cancellationToken);
                                ClientData.Saludo1 = true;
                            }
                            else if (ClientData.Saludo2 != true)
                            {
                                await stepContext.Context.SendActivityAsync($"Muy bien, ¿En qué te puedo ayudar?", cancellationToken: cancellationToken);
                                ClientData.Saludo2 = true;
                            }
                            else
                            {
                                await stepContext.Context.SendActivityAsync($"Muy bien, ¿Hay algo en lo qué te pueda ayudar?", cancellationToken: cancellationToken);
                            }
                        }
                        else if (luisResult.Entities.buendia != null && luisResult.Entities.quetalestas != null)
                        {
                            if (ClientData.Saludo1 != true)
                            {
                                await stepContext.Context.SendActivityAsync($"Buenos Días, Estoy muy bien, ¿y tú?", cancellationToken: cancellationToken);
                                ClientData.Saludo1 = true;
                            }
                            else if (ClientData.Saludo2 != true)
                            {
                                await stepContext.Context.SendActivityAsync($"Estoy muy bien, ¿En qué te puedo ayudar?", cancellationToken: cancellationToken);
                                ClientData.Saludo2 = true;
                            }
                            else
                            {
                                await stepContext.Context.SendActivityAsync($"Estoy muy bien, ¿Hay algo en lo qué te pueda ayudar?", cancellationToken: cancellationToken);
                            }
                        }
                        else if (luisResult.Entities.buendia != null && luisResult.Entities.quetalteva != null)
                        {
                            if (ClientData.Saludo1 != true)
                            {
                                await stepContext.Context.SendActivityAsync($"Buenos Días, Me va muy bien, ¿y tú?", cancellationToken: cancellationToken);
                                ClientData.Saludo1 = true;
                            }
                            else if (ClientData.Saludo2 != true)
                            {
                                await stepContext.Context.SendActivityAsync($"Me va muy bien, ¿En qué te puedo ayudar?", cancellationToken: cancellationToken);
                                ClientData.Saludo2 = true;
                            }
                            else
                            {
                                await stepContext.Context.SendActivityAsync($"Me va muy bien, ¿Hay algo en lo qué te pueda ayudar?", cancellationToken: cancellationToken);
                            }
                        }
                        else if (luisResult.Entities.buenatarde != null && luisResult.Entities.comoestas != null)
                        {
                            if (ClientData.Saludo1 != true)
                            {
                                await stepContext.Context.SendActivityAsync($"Buenas Tardes, Muy bien, ¿y tú?", cancellationToken: cancellationToken);
                                ClientData.Saludo1 = true;
                            }
                            else if (ClientData.Saludo2 != true)
                            {
                                await stepContext.Context.SendActivityAsync($"Muy bien, ¿En qué te puedo ayudar?", cancellationToken: cancellationToken);
                                ClientData.Saludo2 = true;
                            }
                            else
                            {
                                await stepContext.Context.SendActivityAsync($"Muy bien, ¿Hay algo en lo qué te pueda ayudar?", cancellationToken: cancellationToken);
                            }
                        }
                        else if (luisResult.Entities.buenatarde != null && luisResult.Entities.quetalestas != null)
                        {
                            if (ClientData.Saludo1 != true)
                            {
                                await stepContext.Context.SendActivityAsync($"Buenas Tardes, Estoy muy bien, ¿y tú?", cancellationToken: cancellationToken);
                                ClientData.Saludo1 = true;
                            }
                            else if (ClientData.Saludo2 != true)
                            {
                                await stepContext.Context.SendActivityAsync($"Estoy muy bien, ¿En qué te puedo ayudar?", cancellationToken: cancellationToken);
                                ClientData.Saludo2 = true;
                            }
                            else
                            {
                                await stepContext.Context.SendActivityAsync($"Estoy muy bien, ¿Hay algo en lo qué te pueda ayudar?", cancellationToken: cancellationToken);
                            }
                        }
                        else if (luisResult.Entities.buenatarde != null && luisResult.Entities.quetalteva != null)
                        {
                            if (ClientData.Saludo1 != true)
                            {
                                await stepContext.Context.SendActivityAsync($"Buenas Tardes, Me va muy bien, ¿y tú?", cancellationToken: cancellationToken);
                                ClientData.Saludo1 = true;
                            }
                            else if (ClientData.Saludo2 != true)
                            {
                                await stepContext.Context.SendActivityAsync($"Me va muy bien, ¿En qué te puedo ayudar?", cancellationToken: cancellationToken);
                                ClientData.Saludo2 = true;
                            }
                            else
                            {
                                await stepContext.Context.SendActivityAsync($"Me va muy bien, ¿Hay algo en lo qué te pueda ayudar?", cancellationToken: cancellationToken);
                            }
                        }
                        else if (luisResult.Entities.buenasnoches != null && luisResult.Entities.comoestas != null)
                        {
                            if (ClientData.Saludo1 != true)
                            {
                                await stepContext.Context.SendActivityAsync($"Buenas Noches, Muy bien, ¿y tú?", cancellationToken: cancellationToken);
                                ClientData.Saludo1 = true;
                            }
                            else if (ClientData.Saludo2 != true)
                            {
                                await stepContext.Context.SendActivityAsync($"Muy bien, ¿En qué te puedo ayudar?", cancellationToken: cancellationToken);
                                ClientData.Saludo2 = true;
                            }
                            else
                            {
                                await stepContext.Context.SendActivityAsync($"Muy bien, ¿Hay algo en lo qué te pueda ayudar?", cancellationToken: cancellationToken);
                            }
                        }
                        else if (luisResult.Entities.buenasnoches != null && luisResult.Entities.quetalestas != null)
                        {
                            if (ClientData.Saludo1 != true)
                            {
                                await stepContext.Context.SendActivityAsync($"Buenas Noches, Estoy muy bien, ¿y tú?", cancellationToken: cancellationToken);
                                ClientData.Saludo1 = true;
                            }
                            else if (ClientData.Saludo2 != true)
                            {
                                await stepContext.Context.SendActivityAsync($"Estoy muy bien, ¿En qué te puedo ayudar?", cancellationToken: cancellationToken);
                                ClientData.Saludo2 = true;
                            }
                            else
                            {
                                await stepContext.Context.SendActivityAsync($"Estoy muy bien, ¿Hay algo en lo qué te pueda ayudar?", cancellationToken: cancellationToken);
                            }
                        }
                        else if (luisResult.Entities.buenasnoches != null && luisResult.Entities.quetalteva != null)
                        {
                            if (ClientData.Saludo1 != true)
                            {
                                await stepContext.Context.SendActivityAsync($"Buenas Noches, Me va muy bien, ¿y tú?", cancellationToken: cancellationToken);
                                ClientData.Saludo1 = true;
                            }
                            else if (ClientData.Saludo2 != true)
                            {
                                await stepContext.Context.SendActivityAsync($"Me va muy bien, ¿En qué te puedo ayudar?", cancellationToken: cancellationToken);
                                ClientData.Saludo2 = true;
                            }
                            else
                            {
                                await stepContext.Context.SendActivityAsync($"Me va muy bien, ¿Hay algo en lo qué te pueda ayudar?", cancellationToken: cancellationToken);
                            }
                        }
                    }
                    else if (luisResult.Entities.buendia != null)
                    {
                        if (ClientData.Saludo1 != true)
                        {
                            await stepContext.Context.SendActivityAsync($"Buenos Días, ¿En qué te puedo ayudar?", cancellationToken: cancellationToken);
                            ClientData.Saludo1 = true;
                        }
                        else if (ClientData.Saludo2 != true)
                        {
                            await stepContext.Context.SendActivityAsync($"¿Hay algo en lo qué te pueda ayudar?", cancellationToken: cancellationToken);
                            ClientData.Saludo2 = true;
                        }
                        else
                        {
                            await stepContext.Context.SendActivityAsync($"¿Hay algo en lo qué te pueda ayudar?", cancellationToken: cancellationToken);
                        }
                    }
                    else if (luisResult.Entities.buenatarde != null)
                    {
                        if (ClientData.Saludo1 != true)
                        {
                            await stepContext.Context.SendActivityAsync($"Buenas Tardes, ¿En qué te puedo ayudar?", cancellationToken: cancellationToken);
                            ClientData.Saludo1 = true;
                        }
                        else if (ClientData.Saludo2 != true)
                        {
                            await stepContext.Context.SendActivityAsync($"¿Hay algo en lo qué te pueda ayudar?", cancellationToken: cancellationToken);
                            ClientData.Saludo2 = true;
                        }
                        else
                        {
                            await stepContext.Context.SendActivityAsync($"¿Hay algo en lo qué te pueda ayudar?", cancellationToken: cancellationToken);
                        }
                    }

                    else if (luisResult.Entities.buenasnoches != null)
                    {
                        if (ClientData.Saludo1 != true)
                        {
                            await stepContext.Context.SendActivityAsync($"Buenas Noches, ¿En qué te puedo ayudar?", cancellationToken: cancellationToken);
                            ClientData.Saludo1 = true;
                        }
                        else if (ClientData.Saludo2 != true)
                        {
                            await stepContext.Context.SendActivityAsync($"¿Hay algo en lo qué te pueda ayudar?", cancellationToken: cancellationToken);
                            ClientData.Saludo2 = true;
                        }
                        else
                        {
                            await stepContext.Context.SendActivityAsync($"¿Hay algo en lo qué te pueda ayudar?", cancellationToken: cancellationToken);
                        }
                    }
                }
                else
                {
                    //Speech despedida
                    if (luisResult.Entities.cuidate != null)
                    {
                        await stepContext.Context.SendActivityAsync($"Igualmente, fue un placer ayudarte", cancellationToken: cancellationToken);
                    }
                    else
                    {
                        if (luisResult.Entities.buendia != null)
                        {
                            await stepContext.Context.SendActivityAsync($"Fue un placer ayudarte, que tengas buen día", cancellationToken: cancellationToken);
                        }
                        else if (luisResult.Entities.buenatarde != null)
                        {
                            await stepContext.Context.SendActivityAsync($"Fue un placer ayudarte, que tengas una bonita tarde", cancellationToken: cancellationToken);
                        }
                        else if (luisResult.Entities.buenasnoches != null)
                        {
                            await stepContext.Context.SendActivityAsync($"Fue un placer ayudarte, que tengas una bonita noche", cancellationToken: cancellationToken);
                        }
                        else
                        {
                            await stepContext.Context.SendActivityAsync($"Fue un placer ayudarte que tengas" + utilitario.ValidateTime() , cancellationToken: cancellationToken);
                        }
                    }
                }
            }
            else if (luisResult.Entities.saludoinformal != null)
            {

                if (luisResult.Entities.comoestas != null)
                {
                    if (ClientData.Saludo1 != true)
                    {
                        await stepContext.Context.SendActivityAsync($"Muy bien, ¿y tú?", cancellationToken: cancellationToken);
                        ClientData.Saludo1 = true;
                    }
                    else if (ClientData.Saludo2 != true)
                    {
                        await stepContext.Context.SendActivityAsync($"Muy bien, ¿En qué te puedo ayudar?", cancellationToken: cancellationToken);
                        ClientData.Saludo2 = true;
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync($"Muy bien, ¿Hay algo en lo qué te pueda ayudar?", cancellationToken: cancellationToken);
                    }
                }
                else if (luisResult.Entities.quetalestas != null)
                {
                    if (ClientData.Saludo1 != true)
                    {
                        await stepContext.Context.SendActivityAsync($"Estoy muy bien, ¿y tú?", cancellationToken: cancellationToken);
                        ClientData.Saludo1 = true;
                    }
                    else if (ClientData.Saludo2 != true)
                    {
                        await stepContext.Context.SendActivityAsync($"Estoy muy bien, ¿En qué te puedo ayudar?", cancellationToken: cancellationToken);
                        ClientData.Saludo2 = true;
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync($"Estoy muy bien, ¿Hay algo en lo qué te pueda ayudar?", cancellationToken: cancellationToken);
                    }
                }
                else if (luisResult.Entities.quetalteva != null)
                {
                    if (ClientData.Saludo1 != true)
                    {
                        await stepContext.Context.SendActivityAsync($"Me va muy bien, ¿y tú?", cancellationToken: cancellationToken);
                        ClientData.Saludo1 = true;
                    }
                    else if (ClientData.Saludo2 != true)
                    {
                        await stepContext.Context.SendActivityAsync($"Me va muy bien, ¿En qué te puedo ayudar?", cancellationToken: cancellationToken);
                        ClientData.Saludo2 = true;
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync($"Me va muy bien, ¿Hay algo en lo qué te pueda ayudar?", cancellationToken: cancellationToken);
                    }
                }
                else
                {
                    if (ClientData.Saludo1 != true)
                    {
                        await stepContext.Context.SendActivityAsync($"¿En qué te puedo ayudar?", cancellationToken: cancellationToken);
                        ClientData.Saludo1 = true;
                    }
                    else if (ClientData.Saludo2 != true)
                    {
                        await stepContext.Context.SendActivityAsync($"¿Hay algo en lo qué te pueda ayudar?", cancellationToken: cancellationToken);
                        ClientData.Saludo2 = true;
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync($"¿Hay algo en lo qué te pueda ayudar?", cancellationToken: cancellationToken);
                    }
                }
            }
            else if (luisResult.Entities.saludojuvenil != null)
            {
                if (ClientData.Saludo1 != true)
                {
                    await stepContext.Context.SendActivityAsync($"Gusto en saludarte, ¿En qué te puedo ayudar?", cancellationToken: cancellationToken);
                    ClientData.Saludo1 = true;
                }
                else if (ClientData.Saludo2 != true)
                {
                    await stepContext.Context.SendActivityAsync($"Gusto en saludarte, ¿Hay algo en lo qué te pueda ayudar?", cancellationToken: cancellationToken);
                    ClientData.Saludo2 = true;
                }
                else
                {
                    await stepContext.Context.SendActivityAsync($"Gusto en saludarte, ¿Hay algo en lo qué te pueda ayudar?", cancellationToken: cancellationToken);
                }
            }
            else if (luisResult.Entities.quepaso != null)
            {
                await stepContext.Context.SendActivityAsync($"Todo muy bien, gracias, ¿Hay algo en lo qué te pueda ayudar?", cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.comoestas != null || luisResult.Entities.quetalestas != null || luisResult.Entities.quetalteva != null)
            {
                if (luisResult.Entities.comoestas != null)
                {
                    if (ClientData.Saludo1 != true)
                    {
                        await stepContext.Context.SendActivityAsync($"Muy bien, ¿y tú?", cancellationToken: cancellationToken);
                        ClientData.Saludo1 = true;
                    }
                    else if (ClientData.Saludo2 != true)
                    {
                        await stepContext.Context.SendActivityAsync($"Muy bien, ¿En qué te puedo ayudar?", cancellationToken: cancellationToken);
                        ClientData.Saludo2 = true;
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync($"Muy bien, ¿Hay algo en lo qué te pueda ayudar?", cancellationToken: cancellationToken);
                    }
                }
                else if (luisResult.Entities.quetalestas != null)
                {
                    if (ClientData.Saludo1 != true)
                    {
                        await stepContext.Context.SendActivityAsync($"Estoy muy bien, ¿y tú?", cancellationToken: cancellationToken);
                        ClientData.Saludo1 = true;
                    }
                    else if (ClientData.Saludo2 != true)
                    {
                        await stepContext.Context.SendActivityAsync($"Estoy muy bien, ¿En qué te puedo ayudar?", cancellationToken: cancellationToken);
                        ClientData.Saludo2 = true;
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync($"Estoy muy bien, ¿Hay algo en lo qué te pueda ayudar?", cancellationToken: cancellationToken);
                    }
                }
                else if (luisResult.Entities.quetalteva != null)
                {
                    if (ClientData.Saludo1 != true)
                    {
                        await stepContext.Context.SendActivityAsync($"Me va muy bien, ¿y tú?", cancellationToken: cancellationToken);
                        ClientData.Saludo1 = true;
                    }
                    else if (ClientData.Saludo2 != true)
                    {
                        await stepContext.Context.SendActivityAsync($"Me va muy bien, ¿En qué te puedo ayudar?", cancellationToken: cancellationToken);
                        ClientData.Saludo2 = true;
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync($"Me va muy bien, ¿Hay algo en lo qué te pueda ayudar?", cancellationToken: cancellationToken);
                    }
                }
            }
            else
            {
                await stepContext.Context.SendActivityAsync($"¿En qué te puedo ayudar?", cancellationToken: cancellationToken);
            }
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> IntentTarifa(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync($"Si deseas conocer la Tarifa o el costo de la comisión actual que genera el producto indicado haz clic [aquí](https://bancaribe-prod.s3.amazonaws.com/wp-content/uploads/2017/09/Tarifario.pdf)");
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }

      /*  private async Task<DialogTurnResult> IntentTiempoReclamo(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync($"El tiempo estimado por la Superintendencia de Bancos (Sudeban) disponemos de 15 días hábiles para procesarlo por concepto de tarjeta de crédito y débito, y  20 días continuos para otro tipo de reclamos, sin embargo procuramos dar una respuesta en menor tiempo.");
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }*/

        
        private async Task<DialogTurnResult> IntentTasas(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync($"Para conocer la Tasa máxima de nuestros productos haz clic [aquí](https://bancaribe-prod-2020.s3.amazonaws.com/wp-content/uploads/2018/10/Tasas.pdf)");
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> IntentOlvidoClave( WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            if(luisResult.Entities.Clavedinamica != null)
            {
                stepContext.Context.Activity.Text = stepContext.Context.Activity.Text.ToLower();
                var resultQnA = await _qnAMakerAIService._qnAMakerResult.GetAnswersAsync(stepContext.Context);
                var score = resultQnA.FirstOrDefault()?.Score;
                string response = resultQnA.FirstOrDefault()?.Answer;

                if (score >= 0.80)
                {
                    await stepContext.Context.SendActivityAsync(response, cancellationToken: cancellationToken);
                    return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
                }
                else
                {
                    return await stepContext.BeginDialogAsync(nameof(OlvidoClaveDialog), cancellationToken: cancellationToken);
                }
            }

                //SECCIÓN TARJETA
            if ((luisResult.Entities.tdc != null && luisResult.Entities.operar != null) || (luisResult.Entities.tdc != null && luisResult.Entities.Olvido != null))
            {
                ClientData.request = "olvidoclavetdc";
                await stepContext.Context.SendActivityAsync($"Voy a solicitarte algunos datos, que me permitan enviar tu solicitud al área encargada.", cancellationToken: cancellationToken);
                return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);

            }
            else if (luisResult.Entities.tdc != null)
            {
                ClientData.request = "olvidoclavetdc";
                return await stepContext.BeginDialogAsync(nameof(OlvidoClaveTDCDialog), cancellationToken: cancellationToken);

            }
            else if (luisResult.Entities.tdd != null || (luisResult.Entities.tdd != null && luisResult.Entities.Olvido != null))
            {
                ClientData.request = "olvidoclave";
                return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.tarjeta != null)
            {
                ClientData.request = "olvidoclavetjta";
                return await stepContext.BeginDialogAsync(nameof(TipoTarjetaDialog), cancellationToken: cancellationToken);
            }

            //SECCIÓN USUARIO

            else if (luisResult.Entities.Olvido != null && luisResult.Entities.usuario != null)
            {
                await stepContext.Context.SendActivityAsync($"Te puedo recomendar en ese caso que ingreses a Mi Conexión Bancaribe, Persona Natural, ¿Olvide mi Login? Valida la información que te solicitan y se hará él envió del Login al correo anexado al Perfil de Seguridad", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.usuario != null && luisResult.Entities.OnlineBanking != null && luisResult.Entities.cambio != null
                && luisResult.Entities.natural != null)
            {
                await stepContext.Context.SendActivityAsync($"Valida muy bien el Login que estas ingresando, de no recordarte puedes ingresar por la opción ¿Olvide mi Login?, en tal caso si aún presentas el mismo error te recomiendo validar si tu Usuario no está Suspendido", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.usuario != null && luisResult.Entities.NoVeo != null)
            {
                return await stepContext.BeginDialogAsync(nameof(NoVeUsuarioDialog), cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.usuario != null && luisResult.Entities.cambio != null
                && luisResult.Entities.natural != null)
            {
                await stepContext.Context.SendActivityAsync($"Disculpa pero el Login no se puede modificar, pero si lo puede recuperar", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.usuario != null && luisResult.Entities.cambio != null
                && luisResult.Entities.juridico != null)
            {
                await stepContext.Context.SendActivityAsync($"¿Presentas algún problema con tu usuario? el usuario no se puede modificar, te recuerdo que este lo puedes ver en tu carta serial.", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            if (luisResult.Entities.usuario != null && luisResult.Entities.clave != null && luisResult.Entities.Afiliacion != null)
            {
                return await stepContext.BeginDialogAsync(nameof(ClienteNuevoDialog), cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.usuario != null && luisResult.Entities.cambio != null)
            {
                ClientData.request = "cambiousuario";
                return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
            }


            //SECCIÓN CONTRASEÑA
            else if ((luisResult.Entities.temporal != null && luisResult.Entities.NoRecibo is null) || 
                (luisResult.Entities.correo != null && luisResult.Entities.clave != null && luisResult.Entities.NoRecibo is null))
            {
                ClientData.request = "cambioclave";
                return await stepContext.BeginDialogAsync(nameof(SolicitudesClientesEmpresas), cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.NoRecibo != null)
            {
                await stepContext.Context.SendActivityAsync($"Una vez que solicitas la Clave temporal, esta tarda un plazo de 10 a 15 minutos para ser visualizada en el correo, también puedes ver si no se encuentra en la Bandeja de Spam o Correo No Deseados.", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }

            else if (luisResult.Entities.Olvido != null && luisResult.Entities.clave != null && luisResult.Entities.OnlineBanking is null && luisResult.Entities.cambio is null
                && luisResult.Entities.natural is null && luisResult.Entities.juridico is null)
            {
                return await stepContext.BeginDialogAsync(nameof(OlvidoClaveDialog), cancellationToken: cancellationToken);
            }

            else if (luisResult.Entities.Olvido != null && luisResult.Entities.clave != null && luisResult.Entities.OnlineBanking != null && luisResult.Entities.cambio is null
                && luisResult.Entities.natural!= null && luisResult.Entities.juridico is null)
            {
                await stepContext.Context.SendActivityAsync($"Te recomiendo en este caso ingresa a Mi Conexión Bancaribe, Persona Natural, ¿Olvido su Contraseña? Luego responde las preguntas que te solicita el sistema y podrás modificar la contraseña sin ningún problema.", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }

            else if (luisResult.Entities.Olvido != null && luisResult.Entities.clave != null && luisResult.Entities.OnlineBanking != null && luisResult.Entities.cambio is null
                && luisResult.Entities.juridico != null && luisResult.Entities.natural is null)
            {
                ClientData.request = "claveconexbancaribe";
                return await stepContext.BeginDialogAsync(nameof(SolicitudesClientesEmpresas), cancellationToken: cancellationToken); 
            }

            else if (luisResult.Entities.Olvido != null && luisResult.Entities.clave != null && luisResult.Entities.OnlineBanking != null && luisResult.Entities.cambio is null
               && luisResult.Entities.natural is null && luisResult.Entities.juridico is null)
            {
                ClientData.request = "claveconexbancaribe";
                return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
            }

            else if (luisResult.Entities.clave != null && luisResult.Entities.OnlineBanking != null && luisResult.Entities.cambio is null && luisResult.Entities.clave is null)
            {
                return await stepContext.BeginDialogAsync(nameof(ClaveDialog), cancellationToken: cancellationToken);
            }

            else if (luisResult.Entities.Olvido is null && luisResult.Entities.clave != null && luisResult.Entities.cambio != null
               && luisResult.Entities.natural != null && luisResult.Entities.juridico is null)
            {
                await stepContext.Context.SendActivityAsync($"Si deseas modificar la contraseña del ingreso a Mi Conexión Bancaribe, te puedo sugerir que  ingreses por la opción Persona Natural, Cambiar Contraseña. Pero para cambiar la contraseña, debes conocer la contraseña actual, con esa información podrás incluir una nueva.", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }

            else if (luisResult.Entities.Olvido is null && luisResult.Entities.clave != null && luisResult.Entities.cambio != null
               && luisResult.Entities.natural is null && luisResult.Entities.juridico != null)
            {
                ClientData.request = "cambioclave";
                return await stepContext.BeginDialogAsync(nameof(SolicitudesClientesEmpresas), cancellationToken: cancellationToken);
            }

            else if (luisResult.Entities.Olvido is null && luisResult.Entities.clave != null && luisResult.Entities.cambio != null)
            {
                ClientData.request = "cambiocontraseña";
                return await stepContext.BeginDialogAsync(nameof(OlvidoClaveDialog), cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.Olvido is null && luisResult.Entities.clave is null && luisResult.Entities.cambio is null
                && luisResult.Entities.usuario is null && luisResult.Entities.OnlineBanking != null)
            {
                await stepContext.Context.SendActivityAsync($"Valida muy bien si estas colocando los datos de manera correcta, de ser así y el sistema no te permite el ingreso, te recomiendo ingresar por las opciones  ¿Olvido su Login? y ¿Olvido su Contraseña?", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }

            else if ((luisResult.Entities.OnlineBanking != null && luisResult.Entities.usuario is null && luisResult.Entities.clave is null) ||
                (luisResult.Entities.Olvido != null && luisResult.Entities.datos != null))
            {
                await stepContext.Context.SendActivityAsync($"Puedes modificar tu clave por la opción Cambiar Contraseña u ¿Olvido su contraseña? o si lo que olvidaste fue tu Usuario puedes ingresar por la opción ¿Olvido su Login? ", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else
            {
                return await stepContext.BeginDialogAsync(nameof(OlvidoClaveDialog), cancellationToken: cancellationToken);
            }

        }

        private async Task<DialogTurnResult> IntentBloqueadoSuspendido(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());

            //SUSPENDIDO
            if (luisResult.Entities.usuario != null && luisResult.Entities.suspendido != null && luisResult.Entities.natural != null)
            {
                await stepContext.Context.SendActivityAsync($"De ser así, te recuerdo que puede ingresar a Mi conexión Bancaribe , Persona Natural, colocar tu usuario, la última clave que recuerdas y tilde la opción Ingresar. Automáticamente el sistema te enviara a una nueva ventana donde tendrás que presionar la opción \"Aquí\". Esto eliminara el usuario y tendrás que ingresar por la opción Cliente Nuevo ", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.usuario != null && luisResult.Entities.suspendido != null && luisResult.Entities.juridico != null
                 && luisResult.Entities.porque is null)
            {
                ClientData.request = "usuariosuspendido";
                return await stepContext.BeginDialogAsync(nameof(SolicitudesClientesEmpresas), cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.usuario != null && luisResult.Entities.suspendido != null && luisResult.Entities.natural is null
                && luisResult.Entities.juridico is null && luisResult.Entities.porque is null)
            {
                ClientData.request = "usuariosuspendido";
                return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.OnlineBanking != null && luisResult.Entities.suspendido != null && luisResult.Entities.natural != null
                 && luisResult.Entities.porque is null)
            {
                return await stepContext.BeginDialogAsync(nameof(BloqueadoSuspendidoDialog), cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.OnlineBanking != null && luisResult.Entities.suspendido != null && luisResult.Entities.juridico != null
                 && luisResult.Entities.porque is null)
            {
                ClientData.request = "usuariosuspendido";
                return await stepContext.BeginDialogAsync(nameof(SolicitudesClientesEmpresas), cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.OnlineBanking != null && luisResult.Entities.suspendido != null && luisResult.Entities.juridico is null
                && luisResult.Entities.natural is null && luisResult.Entities.porque is null)
            {
                ClientData.request = "usuariosusponline";
                return await stepContext.BeginDialogAsync(nameof(BloqueadoSuspendidoDialog), cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.suspendido != null && luisResult.Entities.porque != null && luisResult.Entities.usuario != null)
            {
                await stepContext.Context.SendActivityAsync($"Cuando ingresas varios datos errados, el sistema por seguridad  bloquea tu usuario, luego cuando intentas desbloquearlo y fallas al hacerlo este pasa a estar Suspendido.", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.tarjeta != null && luisResult.Entities.suspendido != null)
            {
                ClientData.request = "TjtaSusp";
                return await stepContext.BeginDialogAsync(nameof(TipoTarjetaDialog), cancellationToken: cancellationToken);
            }


            //BLOQUEADO
            else if (luisResult.Entities.usuario != null && (luisResult.Entities.Bloqueado != null || luisResult.Entities.Bloqueo != null || luisResult.Entities.Desbloqueo != null)
                && luisResult.Entities.natural != null && luisResult.Entities.porque is null)
            {
                await stepContext.Context.SendActivityAsync($"De ser así, te recuerdo que puedes ingresar a mi conexión bancaribe, persona natural y por ultimo usuario bloqueado. Responde las preguntas que te solicitan y solventaras sin problemas", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.usuario != null && (luisResult.Entities.Bloqueado != null || luisResult.Entities.Bloqueo != null || luisResult.Entities.Desbloqueo != null)
                && luisResult.Entities.juridico != null && luisResult.Entities.porque is null)
            {
                await stepContext.Context.SendActivityAsync($"Si tienes a la mano la tarjeta de conexión segura y tu carta serial, te sugiero que ingreses a Mi Conexión Bancaribe, Persona Jurídica, Cambiar/Recuperar contraseña. Responde las pregunta que te solicita el sistema y solventaras el bloqueo", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.usuario != null && (luisResult.Entities.Bloqueado != null || luisResult.Entities.Bloqueo != null || luisResult.Entities.Desbloqueo != null)
                && luisResult.Entities.natural is null && luisResult.Entities.juridico is null && luisResult.Entities.porque is null)
            {
                ClientData.request = "usuariobloqueado";
                return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.usuario is null && luisResult.Entities.OnlineBanking != null && (luisResult.Entities.Bloqueado != null || luisResult.Entities.Bloqueo != null || luisResult.Entities.Desbloqueo != null)
                && luisResult.Entities.natural != null && luisResult.Entities.porque is null)
            {
                ClientData.request = "usuariobloqueadonat";
                return await stepContext.BeginDialogAsync(nameof(BloqueadoSuspendidoDialog), cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.usuario is null && luisResult.Entities.OnlineBanking != null && (luisResult.Entities.Bloqueado != null || luisResult.Entities.Bloqueo != null || luisResult.Entities.Desbloqueo != null)
                && luisResult.Entities.juridico != null && luisResult.Entities.porque is null)
            {
                ClientData.request = "usuariobloqueadojur";
                return await stepContext.BeginDialogAsync(nameof(BloqueadoSuspendidoDialog), cancellationToken: cancellationToken);
            }
            else if ((luisResult.Entities.usuario is null && luisResult.Entities.OnlineBanking != null && (luisResult.Entities.Bloqueado != null || luisResult.Entities.Bloqueo != null || luisResult.Entities.Desbloqueo != null)
                && luisResult.Entities.juridico is null && luisResult.Entities.natural is null && luisResult.Entities.porque is null) ||
                (luisResult.Entities.clave != null && (luisResult.Entities.Bloqueado != null || luisResult.Entities.Desbloqueo != null || luisResult.Entities.Bloqueo != null && luisResult.Entities.porque is null)))
            {
                ClientData.request = "usuariobloqueado";
                return await stepContext.BeginDialogAsync(nameof(BloqueadoSuspendidoDialog), cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.tarjeta != null && (luisResult.Entities.Bloqueado != null || luisResult.Entities.Bloqueo != null)
                 && luisResult.Entities.porque is null)
            {
                ClientData.request = "TjtaBloq";
                return await stepContext.BeginDialogAsync(nameof(TipoTarjetaDialog), cancellationToken: cancellationToken);
            }
            else if ((luisResult.Entities.Bloqueado != null || luisResult.Entities.Bloqueo != null) && luisResult.Entities.porque != null && luisResult.Entities.usuario != null)
            {
                await stepContext.Context.SendActivityAsync($"Tu usuario se bloquea motivado a que ingresaste 3 veces seguidas la contraseña de manera errada.", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }


            //GENERAL
            else if ((luisResult.Entities.nopuedo != null && luisResult.Entities.OnlineBanking != null && luisResult.Entities.usuario != null) ||
               (luisResult.Entities.nopuedo != null && luisResult.Entities.OnlineBanking != null && luisResult.Entities.datos != null))
            {
                await stepContext.Context.SendActivityAsync($"Valida muy bien el login que estas ingresando, de no recordarte puedes ingresar por la opción ¿Olvide mi Login?, en tal caso si aún presentas el mismo error te recomiendo validar si tu usuario no está suspendido", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else if ((luisResult.Entities.nopuedo != null && luisResult.Entities.OnlineBanking != null && luisResult.Entities.porque is null)
                || (luisResult.Entities.nopuedo != null && luisResult.Entities.usuario != null && luisResult.Entities.porque is null))
            {
                ClientData.request = "problemaonline";
                return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
            }
            else if ((luisResult.Entities.porque != null && luisResult.Entities.usuario != null) ||
                (luisResult.Entities.porque != null && luisResult.Entities.OnlineBanking != null) ||
                (luisResult.Entities.porque != null && luisResult.Entities.clave != null) ||
                (luisResult.Entities.disponible != null && luisResult.Entities.usuario != null) ||
                (luisResult.Entities.problema != null && luisResult.Entities.OnlineBanking is null) )
            {
                await stepContext.Context.SendActivityAsync($"Te recuerdo que hay diversos medios de seguridad que te limitan el ingreso a tu cuenta por la página Mi Conexión Bancaribe si coloca algún dato errado. Por favor indícame ¿Que te arroja el sistema?", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else if ((luisResult.Entities.nuevo != null && luisResult.Entities.problema != null && luisResult.Entities.natural != null) ||
                    (luisResult.Entities.disponible != null && luisResult.Entities.problema != null && luisResult.Entities.natural != null))
            {
                await stepContext.Context.SendActivityAsync($"¿Qué error te arroja la página?", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else if ((luisResult.Entities.nuevo != null && luisResult.Entities.problema != null && luisResult.Entities.juridico != null) ||
                    (luisResult.Entities.disponible != null && luisResult.Entities.problema != null && luisResult.Entities.juridico != null))
            {
               ClientData.request = "problemaonlinejur";
                return await stepContext.BeginDialogAsync(nameof(SolicitudesClientesEmpresas), cancellationToken: cancellationToken);
            }
            else if ((luisResult.Entities.nuevo != null && luisResult.Entities.problema != null) ||
                    (luisResult.Entities.disponible != null && luisResult.Entities.problema != null ))
            {
                ClientData.request = "problemaonline";
                return await stepContext.BeginDialogAsync(nameof(NaturalJuridicoDialog), cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.OnlineBanking != null && luisResult.Entities.problema != null)
            {
                await stepContext.Context.SendActivityAsync($"En este momento no presentamos ningún error en la página Mi conexión Bancaribe, te recomiendo que si no puedes acceder a tu cuenta, valida si tu Usuario o Contraseña son correctos, o si la página no te indica ningún Bloqueo o Suspensión.", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.restaurar != null)
            {
                await stepContext.Context.SendActivityAsync($"Puede modificar tu clave por la opción Cambiar Contraseña u ¿Olvido su contraseña? o si olvidaste tu usuario puede tildar la opción ¿Olvido su Login?", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.Bloqueado != null || luisResult.Entities.Bloqueado != null)
            {
                return await stepContext.BeginDialogAsync(nameof(TipoTarjetaDialog), cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.nopuedo != null )
            {
                await stepContext.Context.SendActivityAsync($"¿Qué error te arroja la página?", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync($"Puede modificar tu clave por la opción Cambiar Contraseña u ¿Olvido su contraseña? o si olvidaste tu usuario puede tildar la opción ¿Olvido su Login?", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> IntentContacto(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync($"Tienes varias formas de contactarme " +
                $"{Environment.NewLine}☎ 0500-BANCARIBE 0500-2262274" +
                $"{Environment.NewLine}☎ 0212-9545777" +
                $"{Environment.NewLine}Whatsapp: 04146426682" +
                $"Sí te encuentras en el exterior comunícate al ☎ +58 (212) 954.57.77", cancellationToken: cancellationToken);
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }
        private async Task<DialogTurnResult> IntentEmojisTwitter(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            
            if (stepContext.Context.Activity.Text.Length >3)
            { await stepContext.Context.SendActivityAsync($"Para brindarte un mejor servicio requiero que por favor ingreses la misma información con otras palabras y así entender en que deseas que te ayude");
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync($"Para evitar confusión en lo que yo interprete del emoji, te pido que por favor escribas lo que deseas decirme", cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> IntentRegistroConexBancaribe(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync($"Debes ingresar al siguiente enlace www3.bancaribe.com.ve/bcn/ y luego hacer clic {Environment.NewLine}" +
                $"en la opción \"Cliente Nuevo\" para que siga los pasos del registro.", cancellationToken: cancellationToken);
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> IntentPreAperturaCta(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {

            if (luisResult.Entities.juridico != null)
            {
                stepContext.Context.Activity.Text = "solicitud de una cuenta jurídica";
                var resultQnA = await _qnAMakerAIService._qnAMakerResult.GetAnswersAsync(stepContext.Context);
                string response = resultQnA.FirstOrDefault()?.Answer;
                await stepContext.Context.SendActivityAsync(response, cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.natural != null)
            {
                stepContext.Context.Activity.Text = "solicitud de una cuenta natural";
                var resultQnA = await _qnAMakerAIService._qnAMakerResult.GetAnswersAsync(stepContext.Context);
                string response = resultQnA.FirstOrDefault()?.Answer;
                await stepContext.Context.SendActivityAsync(response, cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.Joven != null)
            {
                stepContext.Context.Activity.Text = "solicitud de una cuenta para chamos";
                var resultQnA = await _qnAMakerAIService._qnAMakerResult.GetAnswersAsync(stepContext.Context);
                string response = resultQnA.FirstOrDefault()?.Answer;
                await stepContext.Context.SendActivityAsync(response, cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else if (luisResult.Entities.divisa != null && luisResult.Entities.Bs is null)
            {
                stepContext.Context.Activity.Text = "solicitud de una cuenta en moneda extranjera";
                var resultQnA = await _qnAMakerAIService._qnAMakerResult.GetAnswersAsync(stepContext.Context);
                string response = resultQnA.FirstOrDefault()?.Answer;
                await stepContext.Context.SendActivityAsync(response, cancellationToken: cancellationToken);
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            else
            {
                return await stepContext.BeginDialogAsync(nameof(CtasGeneralDialog), cancellationToken: cancellationToken);
            }

            /* if (luisResult.Entities.TipoCta is null)
             {
                 return await stepContext.BeginDialogAsync(nameof(CtasDialog), cancellationToken: cancellationToken);
             }
             else if (luisResult.Entities.TipoCta.First().Equals("corriente"))
             {
                 return await stepContext.BeginDialogAsync(nameof(CtasCteDialog), cancellationToken: cancellationToken);
             }
             else 
             {
                 await stepContext.Context.SendActivityAsync($"Deberás reunir todos los recaudos, ingresar a este [link](https://www.bancaribe.com.ve/zona-de-informacion-para-cada-mercado/personas/solicitud-mi-cuenta-de-ahorro-bancaribe) y haz clic en ¡Solicítala ya en línea!.", cancellationToken: cancellationToken);
                 return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
             }*/
        }

        private async Task<DialogTurnResult> IntentConexionBancaribe(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync($"Requiero más información para poder ayudarte", cancellationToken: cancellationToken);
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            //return await stepContext.BeginDialogAsync(nameof(ErrorConexDialog), cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> IntentReclamoPOS(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {

            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            ClientData.request = "reclamopos";
            return await stepContext.BeginDialogAsync(nameof(IsTDCDialog), cancellationToken: cancellationToken);

        }

        private async Task<DialogTurnResult> IntentReclamoPagoMovil(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync($"Si posees algún débito o se te fue descontado de manera errada un(os) monto(s) de tu(s) cuenta(s), puedes realizar el reporte en nuestra pagina principal " +
                 $"a través del \"Formulario de Atención\" y luego validar los datos que te solicitan. Recuerda dar la mayor información de lo sucedido y seleccionar de manera correcta " +
                 $"el tipo de reclamo. Este es el [enlace](https://www.bancaribe.com.ve/informacion-vital/atencion-al-cliente/formulario-unico-de-reclamos-de-atencion-al-cliente) del formulario de atención", cancellationToken: cancellationToken);
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            /*
            return await stepContext.BeginDialogAsync(nameof(ReclamoMiPago), cancellationToken: cancellationToken);
            */
        }

        private async Task<DialogTurnResult> IntentReclamoRecarga(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync($"Si posees algún débito o se te fue descontado de manera errada un(os) monto(s) de tu(s) cuenta(s), puedes realizar el reporte en nuestra pagina principal " +
                 $"a través del \"Formulario de Atención\" y luego validar los datos que te solicitan. Recuerda dar la mayor información de lo sucedido y seleccionar de manera correcta " +
                 $"el tipo de reclamo. Este es el [enlace](https://www.bancaribe.com.ve/informacion-vital/atencion-al-cliente/formulario-unico-de-reclamos-de-atencion-al-cliente) del formulario de atención", cancellationToken: cancellationToken);
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            /*
            return await stepContext.BeginDialogAsync(nameof(ReclamoRecarga), cancellationToken: cancellationToken);
            */
        }

        private async Task<DialogTurnResult> IntentReclamoCajero(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync($"Si posees algún débito o se te fue descontado de manera errada un(os) monto(s) de tu(s) cuenta(s), puedes realizar el reporte en nuestra pagina principal " +
                 $"a través del \"Formulario de Atención\" y luego validar los datos que te solicitan. Recuerda dar la mayor información de lo sucedido y seleccionar de manera correcta " +
                 $"el tipo de reclamo. Este es el [enlace](https://www.bancaribe.com.ve/informacion-vital/atencion-al-cliente/formulario-unico-de-reclamos-de-atencion-al-cliente) del formulario de atención", cancellationToken: cancellationToken);
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            /*
            return await stepContext.BeginDialogAsync(nameof(ReclamoCajero), cancellationToken: cancellationToken);
            */
        }
        private async Task<DialogTurnResult> IntentReclamoTDC(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(UsoPagoTDCDialog), cancellationToken: cancellationToken);
            /*
            return await stepContext.BeginDialogAsync(nameof(ReclamoTDC), cancellationToken: cancellationToken);
            */
        }

        private async Task<DialogTurnResult> IntentReclamoTransferencia(WaterfallStepContext stepContext, ClassToGenerate luisResult, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(FechaTransferenciaReclamo), cancellationToken: cancellationToken);
        }

        #endregion LuisIntent


        private async Task<DialogTurnResult> FinalProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());

            if (ClientData.ClienteAtendidoAux)
            {
                ClientData.ClienteAtendido = true;
            }
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
